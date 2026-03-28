using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Geuneda.Services;
using IdleRPG.OfflineReward;
using Newtonsoft.Json;
using static IdleRPG.Core.DevLog;

namespace IdleRPG.Core
{
    /// <summary>
    /// <see cref="ISaveService"/>의 서버 기반 구현체.
    /// 2-Tier 저장(즉시 + Dirty Debounce), 30초 주기 자동 저장, 이벤트 기반 dirty 마킹을 처리한다.
    /// 모든 저장/로드는 Nakama 서버를 통해 수행된다. 로컬 폴백은 없다.
    /// 피처별 저장 로직은 <see cref="ISaveDataCollector"/>에 위임한다.
    /// </summary>
    public class SaveService : ISaveService, IDisposable
    {
        private const float AutoSaveInterval = 30f;
        private const float DebounceDuration = 5f;

        private readonly IReadOnlyList<ISaveDataCollector> _collectors;
        private readonly INakamaService _nakamaService;
        private readonly ITickService _tickService;
        private readonly IMessageBrokerService _messageBroker;
        private readonly ITimeService _timeService;

        private bool _isDirty;
        private bool _isAutoSaveActive;
        private bool _isSaving;
        private CancellationTokenSource _debounceCts;

        /// <summary>
        /// <see cref="SaveService"/>를 생성한다.
        /// </summary>
        /// <param name="collectors">피처별 저장 데이터 수집기 목록</param>
        /// <param name="nakamaService">Nakama 서버 통신 서비스</param>
        /// <param name="tickService">주기적 업데이트 서비스</param>
        /// <param name="messageBroker">이벤트 통신 서비스</param>
        /// <param name="timeService">시간 관리 서비스</param>
        public SaveService(
            IReadOnlyList<ISaveDataCollector> collectors,
            INakamaService nakamaService,
            ITickService tickService,
            IMessageBrokerService messageBroker,
            ITimeService timeService)
        {
            _collectors = collectors;
            _nakamaService = nakamaService;
            _tickService = tickService;
            _messageBroker = messageBroker;
            _timeService = timeService;
        }

        /// <inheritdoc />
        public async UniTask SaveAsync()
        {
            await SaveInternalAsync();
        }

        /// <inheritdoc />
        public void SaveIfDirty()
        {
            if (_isDirty)
            {
                SaveSilentAsync().Forget();
            }
        }

        /// <inheritdoc />
        public void MarkDirty()
        {
            _isDirty = true;

            if (_debounceCts != null) return;

            _debounceCts = new CancellationTokenSource();
            RunDebounceSaveAsync(_debounceCts.Token).Forget();
        }

        /// <inheritdoc />
        public async UniTask<GameSaveData> LoadAsync()
        {
            var result = await _nakamaService.LoadGameDataAsync();

            if (!result.HasData)
            {
                Log("[SaveService] 서버에 저장 데이터 없음 - 신규 게임 시작");
                return new GameSaveData();
            }

            try
            {
                var saveData = JsonConvert.DeserializeObject<GameSaveData>(result.JsonData);
                Log($"[SaveService] 서버 데이터 로드 완료 (timestamp: {saveData.LastSaveTimestamp})");
                return saveData;
            }
            catch (JsonException ex)
            {
                Log($"[SaveService] 저장 데이터 역직렬화 실패 - 신규 게임 시작: {ex.Message}");
                return new GameSaveData();
            }
        }

        /// <inheritdoc />
        public void StartAutoSave()
        {
            if (_isAutoSaveActive) return;
            _isAutoSaveActive = true;

            _tickService.SubscribeOnUpdate(OnAutoSaveTick, AutoSaveInterval, false, true);

            _messageBroker.Subscribe<OfflineRewardClaimedMessage>(OnOfflineRewardClaimed);

            for (int i = 0; i < _collectors.Count; i++)
            {
                _collectors[i].SubscribeDirtyEvents(_messageBroker, MarkDirty);
            }

            Log("[SaveService] 자동 저장 시작");
        }

        /// <inheritdoc />
        public void StopAutoSave()
        {
            if (!_isAutoSaveActive) return;
            _isAutoSaveActive = false;

            _tickService.Unsubscribe(OnAutoSaveTick);
            _messageBroker.Unsubscribe<OfflineRewardClaimedMessage>(this);

            for (int i = 0; i < _collectors.Count; i++)
            {
                _collectors[i].UnsubscribeDirtyEvents(_messageBroker);
            }

            CancelDebounce();

            Log("[SaveService] 자동 저장 중지");
        }

        /// <summary>
        /// 로드된 저장 데이터를 모델들에 적용한다. 서비스 생성 전에 호출해야 한다.
        /// </summary>
        /// <param name="saveData">적용할 저장 데이터</param>
        public void ApplyLoadedData(GameSaveData saveData)
        {
            if (saveData == null) return;

            for (int i = 0; i < _collectors.Count; i++)
            {
                _collectors[i].ApplyFrom(saveData);
            }

            bool hasExisting = saveData.LastSaveTimestamp > 0;

            _messageBroker.Publish(new GameLoadedMessage
            {
                HasExistingData = hasExisting
            });

            Log(hasExisting
                ? $"[SaveService] 저장 데이터 복원 완료 (timestamp: {saveData.LastSaveTimestamp})"
                : "[SaveService] 신규 게임 시작");
        }

        /// <inheritdoc cref="IDisposable.Dispose"/>
        public void Dispose()
        {
            StopAutoSave();
        }

        /// <summary>
        /// 서버 저장을 수행한다. 실패 시 dirty 플래그를 복원하고 예외를 전파한다.
        /// </summary>
        private async UniTask SaveInternalAsync()
        {
            if (_isSaving) return;
            _isSaving = true;

            try
            {
                CancelDebounce();
                _isDirty = false;

                var saveData = ExtractSaveData();
                var json = JsonConvert.SerializeObject(saveData);

                await _nakamaService.SaveGameDataAsync(json);

                _messageBroker.Publish(new GameSavedMessage
                {
                    Timestamp = _nakamaService.ServerTimeMillis
                });

                Log("[SaveService] 서버 저장 완료");
            }
            catch (Exception ex)
            {
                _isDirty = true;
                Log($"[SaveService] 서버 저장 실패: {ex.Message}");
                throw;
            }
            finally
            {
                _isSaving = false;
            }
        }

        /// <summary>
        /// 서버 저장을 시도하되 실패 시 예외를 삼킨다.
        /// 자동 저장, debounce, OnApplicationPause 등 fire-and-forget 상황에서 사용한다.
        /// </summary>
        private async UniTaskVoid SaveSilentAsync()
        {
            try
            {
                await SaveInternalAsync();
            }
            catch (Exception ex)
            {
                Log($"[SaveService] 서버 저장 실패 (자동 재시도 예정): {ex.Message}");
            }
        }

        private GameSaveData ExtractSaveData()
        {
            var saveData = new GameSaveData
            {
                SaveVersion = 1,
                LastSaveTimestamp = _timeService.UnixTimeNow
            };

            for (int i = 0; i < _collectors.Count; i++)
            {
                _collectors[i].ExtractTo(saveData);
            }

            return saveData;
        }

        private void OnAutoSaveTick(float deltaTime)
        {
            if (!_isDirty) return;
            SaveSilentAsync().Forget();
        }

        private void OnOfflineRewardClaimed(OfflineRewardClaimedMessage message)
        {
            SaveSilentAsync().Forget();
        }

        private async UniTaskVoid RunDebounceSaveAsync(CancellationToken ct)
        {
            try
            {
                await UniTask.Delay(
                    TimeSpan.FromSeconds(DebounceDuration),
                    ignoreTimeScale: true,
                    cancellationToken: ct);

                _debounceCts?.Dispose();
                _debounceCts = null;

                if (_isDirty)
                {
                    await SaveInternalAsync();
                }
            }
            catch (OperationCanceledException)
            {
                // debounce 취소 - 정상 동작 (Save 또는 CancelDebounce에 의해)
            }
            catch (Exception ex)
            {
                Log($"[SaveService] Debounce 저장 실패: {ex.Message}");
            }
        }

        private void CancelDebounce()
        {
            if (_debounceCts != null)
            {
                _debounceCts.Cancel();
                _debounceCts.Dispose();
                _debounceCts = null;
            }
        }
    }
}
