using System;
using System.Collections;
using System.Collections.Generic;
using Geuneda.Services;
using IdleRPG.OfflineReward;
using UnityEngine;
using static IdleRPG.Core.DevLog;

namespace IdleRPG.Core
{
    /// <summary>
    /// <see cref="ISaveService"/>의 구현체.
    /// 2-Tier 저장(즉시 + Dirty Debounce), 30초 주기 자동 저장, 이벤트 기반 dirty 마킹을 처리한다.
    /// 피처별 저장 로직은 <see cref="ISaveDataCollector"/>에 위임한다.
    /// </summary>
    public class SaveService : ISaveService, IDisposable
    {
        private const float AutoSaveInterval = 30f;
        private const float DebounceDuration = 5f;

        private readonly IReadOnlyList<ISaveDataCollector> _collectors;
        private readonly IDataService _dataService;
        private readonly ITickService _tickService;
        private readonly ICoroutineService _coroutineService;
        private readonly IMessageBrokerService _messageBroker;
        private readonly ITimeService _timeService;

        private bool _isDirty;
        private Coroutine _debounceCoroutine;
        private bool _isAutoSaveActive;

        /// <summary>
        /// <see cref="SaveService"/>를 생성한다.
        /// </summary>
        /// <param name="collectors">피처별 저장 데이터 수집기 목록</param>
        /// <param name="dataService">데이터 영속성 서비스</param>
        /// <param name="tickService">주기적 업데이트 서비스</param>
        /// <param name="coroutineService">코루틴 호스트 서비스</param>
        /// <param name="messageBroker">이벤트 통신 서비스</param>
        /// <param name="timeService">시간 관리 서비스</param>
        public SaveService(
            IReadOnlyList<ISaveDataCollector> collectors,
            IDataService dataService,
            ITickService tickService,
            ICoroutineService coroutineService,
            IMessageBrokerService messageBroker,
            ITimeService timeService)
        {
            _collectors = collectors;
            _dataService = dataService;
            _tickService = tickService;
            _coroutineService = coroutineService;
            _messageBroker = messageBroker;
            _timeService = timeService;
        }

        /// <inheritdoc />
        public void Save()
        {
            CancelDebounce();
            _isDirty = false;

            var saveData = ExtractSaveData();
            _dataService.AddOrReplaceData(saveData);
            _dataService.SaveData<GameSaveData>();

            _messageBroker.Publish(new GameSavedMessage
            {
                Timestamp = saveData.LastSaveTimestamp
            });

            Log($"[SaveService] 게임 데이터 저장 완료 (timestamp: {saveData.LastSaveTimestamp})");
        }

        /// <inheritdoc />
        public void SaveIfDirty()
        {
            if (_isDirty)
            {
                Save();
            }
        }

        /// <inheritdoc />
        public void MarkDirty()
        {
            _isDirty = true;

            if (_debounceCoroutine == null)
            {
                _debounceCoroutine = _coroutineService.StartCoroutine(DebounceSaveCoroutine());
            }
        }

        /// <inheritdoc />
        public GameSaveData Load()
        {
            return _dataService.LoadData<GameSaveData>();
        }

        /// <inheritdoc />
        public bool HasSaveData()
        {
            return _dataService.HasData<GameSaveData>();
        }

        /// <inheritdoc />
        public void DeleteSaveData()
        {
            _dataService.AddOrReplaceData(new GameSaveData());
            _dataService.SaveData<GameSaveData>();
            _isDirty = false;
            CancelDebounce();

            Log("[SaveService] 저장 데이터 삭제 완료");
        }

        /// <inheritdoc />
        public void StartAutoSave()
        {
            if (_isAutoSaveActive) return;
            _isAutoSaveActive = true;

            _tickService.SubscribeOnUpdate(OnAutoSaveTick, AutoSaveInterval, false, true);

            // OfflineRewardClaimedMessage: 보상 수령 후 데이터 손실 방지를 위해 즉시 저장한다.
            // 자체 저장 필드가 없으므로 Collector가 아닌 SaveService에서 직접 처리한다.
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
            Save();
        }

        private void OnOfflineRewardClaimed(OfflineRewardClaimedMessage message)
        {
            Save();
        }

        private IEnumerator DebounceSaveCoroutine()
        {
            // 의도적으로 캐싱하지 않음. - StopCoroutine 이후 재사용시 타이밍 오류 방지
            yield return new WaitForSecondsRealtime(DebounceDuration);

            _debounceCoroutine = null;

            if (_isDirty)
            {
                Save();
            }
        }

        private void CancelDebounce()
        {
            if (_debounceCoroutine != null)
            {
                _coroutineService.StopCoroutine(_debounceCoroutine);
                _debounceCoroutine = null;
            }
        }
    }
}
