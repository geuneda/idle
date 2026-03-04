using System;
using System.Collections;
using System.Globalization;
using Geuneda.Services;
using IdleRPG.Economy;
using IdleRPG.Equipment;
using IdleRPG.Growth;
using IdleRPG.Skill;
using IdleRPG.Pet;
using IdleRPG.Hero;
using IdleRPG.OfflineReward;
using IdleRPG.Stage;
using UnityEngine;

namespace IdleRPG.Core
{
    /// <summary>
    /// <see cref="ISaveService"/>의 구현체.
    /// 2-Tier 저장(즉시 + Dirty Debounce), 30초 주기 자동 저장, 이벤트 기반 dirty 마킹을 처리한다.
    /// </summary>
    public class SaveService : ISaveService, IDisposable
    {
        private const float AutoSaveInterval = 30f;
        private const float DebounceDuration = 5f;

        private readonly StageModel _stageModel;
        private readonly CurrencyModel _currencyModel;
        private readonly HeroGrowthModel _growthModel;
        private readonly EquipmentModel _equipmentModel;
        private readonly EquipmentConfigCollection _equipmentConfig;
        private readonly SkillModel _skillModel;
        private readonly PetModel _petModel;
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
        /// <param name="stageModel">스테이지 진행 모델</param>
        /// <param name="currencyModel">재화 모델</param>
        /// <param name="growthModel">영웅 성장 모델</param>
        /// <param name="equipmentModel">장비 모델</param>
        /// <param name="equipmentConfig">장비 설정 (장비 ID 열거용)</param>
        /// <param name="skillModel">스킬 모델</param>
        /// <param name="petModel">펫 모델</param>
        /// <param name="dataService">데이터 영속성 서비스</param>
        /// <param name="tickService">주기적 업데이트 서비스</param>
        /// <param name="coroutineService">코루틴 호스트 서비스</param>
        /// <param name="messageBroker">이벤트 통신 서비스</param>
        /// <param name="timeService">시간 관리 서비스</param>
        public SaveService(
            StageModel stageModel,
            CurrencyModel currencyModel,
            HeroGrowthModel growthModel,
            EquipmentModel equipmentModel,
            EquipmentConfigCollection equipmentConfig,
            SkillModel skillModel,
            PetModel petModel,
            IDataService dataService,
            ITickService tickService,
            ICoroutineService coroutineService,
            IMessageBrokerService messageBroker,
            ITimeService timeService)
        {
            _stageModel = stageModel;
            _currencyModel = currencyModel;
            _growthModel = growthModel;
            _equipmentModel = equipmentModel;
            _equipmentConfig = equipmentConfig;
            _skillModel = skillModel;
            _petModel = petModel;
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

            Debug.Log($"[SaveService] 게임 데이터 저장 완료 (timestamp: {saveData.LastSaveTimestamp})");
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

            Debug.Log("[SaveService] 저장 데이터 삭제 완료");
        }

        /// <inheritdoc />
        public void StartAutoSave()
        {
            if (_isAutoSaveActive) return;
            _isAutoSaveActive = true;

            _tickService.SubscribeOnUpdate(OnAutoSaveTick, AutoSaveInterval, false, true);

            _messageBroker.Subscribe<StatLevelUpMessage>(OnStatLevelUp);
            _messageBroker.Subscribe<StageClearedMessage>(OnStageCleared);
            _messageBroker.Subscribe<ChapterClearedMessage>(OnChapterCleared);
            _messageBroker.Subscribe<EquipmentAcquiredMessage>(OnEquipmentChanged);
            _messageBroker.Subscribe<EquipmentUpgradedMessage>(OnEquipmentChanged);
            _messageBroker.Subscribe<EquipmentEquippedMessage>(OnEquipmentChanged);
            _messageBroker.Subscribe<CurrencyChangedMessage>(OnCurrencyChanged);
            _messageBroker.Subscribe<OfflineRewardClaimedMessage>(OnOfflineRewardClaimed);
            _messageBroker.Subscribe<SkillAcquiredMessage>(OnSkillChanged);
            _messageBroker.Subscribe<SkillUpgradedMessage>(OnSkillChanged);
            _messageBroker.Subscribe<SkillEquippedMessage>(OnSkillChanged);
            _messageBroker.Subscribe<PetAcquiredMessage>(OnPetChanged);
            _messageBroker.Subscribe<PetUpgradedMessage>(OnPetChanged);
            _messageBroker.Subscribe<PetEquippedMessage>(OnPetChanged);

            Debug.Log("[SaveService] 자동 저장 시작");
        }

        /// <inheritdoc />
        public void StopAutoSave()
        {
            if (!_isAutoSaveActive) return;
            _isAutoSaveActive = false;

            _tickService.Unsubscribe(OnAutoSaveTick);
            _messageBroker.Unsubscribe<StatLevelUpMessage>(this);
            _messageBroker.Unsubscribe<StageClearedMessage>(this);
            _messageBroker.Unsubscribe<ChapterClearedMessage>(this);
            _messageBroker.Unsubscribe<EquipmentAcquiredMessage>(this);
            _messageBroker.Unsubscribe<EquipmentUpgradedMessage>(this);
            _messageBroker.Unsubscribe<EquipmentEquippedMessage>(this);
            _messageBroker.Unsubscribe<CurrencyChangedMessage>(this);
            _messageBroker.Unsubscribe<OfflineRewardClaimedMessage>(this);
            _messageBroker.Unsubscribe<SkillAcquiredMessage>(this);
            _messageBroker.Unsubscribe<SkillUpgradedMessage>(this);
            _messageBroker.Unsubscribe<SkillEquippedMessage>(this);
            _messageBroker.Unsubscribe<PetAcquiredMessage>(this);
            _messageBroker.Unsubscribe<PetUpgradedMessage>(this);
            _messageBroker.Unsubscribe<PetEquippedMessage>(this);

            CancelDebounce();

            Debug.Log("[SaveService] 자동 저장 중지");
        }

        /// <summary>
        /// 로드된 저장 데이터를 모델들에 적용한다. 서비스 생성 전에 호출해야 한다.
        /// </summary>
        /// <param name="saveData">적용할 저장 데이터</param>
        public void ApplyLoadedData(GameSaveData saveData)
        {
            if (saveData == null) return;

            ApplyStageData(saveData.Stage);
            ApplyCurrencyData(saveData.Currency);
            ApplyGrowthData(saveData.Growth);
            ApplyEquipmentData(saveData.Equipment);
            ApplySkillData(saveData.Skill);
            ApplyPetData(saveData.Pet);

            bool hasExisting = saveData.LastSaveTimestamp > 0;

            _messageBroker.Publish(new GameLoadedMessage
            {
                HasExistingData = hasExisting
            });

            Debug.Log(hasExisting
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
            return new GameSaveData
            {
                SaveVersion = 1,
                LastSaveTimestamp = _timeService.UnixTimeNow,
                Stage = ExtractStageData(),
                Currency = ExtractCurrencyData(),
                Growth = ExtractGrowthData(),
                Equipment = ExtractEquipmentData(),
                Skill = ExtractSkillData(),
                Pet = ExtractPetData()
            };
        }

        private StageSaveData ExtractStageData()
        {
            return new StageSaveData
            {
                CurrentChapter = _stageModel.CurrentChapter.Value,
                CurrentStage = _stageModel.CurrentStage.Value,
                CurrentWave = _stageModel.CurrentWave.Value,
                IsBossAutoChallenge = _stageModel.IsBossAutoChallenge.Value,
                HighestChapter = _stageModel.HighestChapter,
                HighestStage = _stageModel.HighestStage
            };
        }

        private CurrencySaveData ExtractCurrencyData()
        {
            var data = new CurrencySaveData();
            foreach (CurrencyType type in Enum.GetValues(typeof(CurrencyType)))
            {
                data.Currencies[(int)type] = _currencyModel.GetAmount(type).ToString();
            }
            return data;
        }

        private GrowthSaveData ExtractGrowthData()
        {
            var data = new GrowthSaveData();
            foreach (HeroStatType type in Enum.GetValues(typeof(HeroStatType)))
            {
                data.StatLevels[(int)type] = _growthModel.GetLevel(type);
            }
            return data;
        }

        private SkillSaveData ExtractSkillData()
        {
            var data = new SkillSaveData();
            foreach (var item in _skillModel.GetAllItems())
            {
                if (item.OwnedCount.Value <= 0 && item.Level.Value <= 0) continue;

                data.Items[item.Id] = new SkillItemSaveEntry
                {
                    OwnedCount = item.OwnedCount.Value,
                    Level = item.Level.Value
                };
            }

            for (int i = 0; i < SkillModel.MAX_SLOTS; i++)
            {
                int skillId = _skillModel.GetEquippedSkillId(i);
                if (skillId != SkillModel.UNEQUIPPED)
                {
                    data.Equipped[i] = skillId;
                }
            }

            return data;
        }

        private PetSaveData ExtractPetData()
        {
            var data = new PetSaveData();
            foreach (var item in _petModel.GetAllItems())
            {
                if (item.OwnedCount.Value <= 0 && item.Level.Value <= 0) continue;

                data.Items[item.Id] = new PetItemSaveEntry
                {
                    OwnedCount = item.OwnedCount.Value,
                    Level = item.Level.Value
                };
            }

            for (int i = 0; i < PetModel.MAX_SLOTS; i++)
            {
                int petId = _petModel.GetEquippedPetId(i);
                if (petId != PetModel.UNEQUIPPED)
                {
                    data.Equipped[i] = petId;
                }
            }

            return data;
        }

        private void ApplyStageData(StageSaveData data)
        {
            if (data == null) return;

            _stageModel.CurrentChapter.Value = data.CurrentChapter;
            _stageModel.CurrentStage.Value = data.CurrentStage;
            _stageModel.CurrentWave.Value = data.CurrentWave;
            _stageModel.IsBossAutoChallenge.Value = data.IsBossAutoChallenge;
            _stageModel.HighestChapter = data.HighestChapter;
            _stageModel.HighestStage = data.HighestStage;
        }

        private void ApplyCurrencyData(CurrencySaveData data)
        {
            if (data == null) return;

            foreach (var pair in data.Currencies)
            {
                if (!Enum.IsDefined(typeof(CurrencyType), pair.Key)) continue;

                var type = (CurrencyType)pair.Key;
                var amount = ParseBigNumber(pair.Value);
                _currencyModel.SetAmount(type, amount);
            }
        }

        private void ApplyGrowthData(GrowthSaveData data)
        {
            if (data == null) return;

            foreach (var pair in data.StatLevels)
            {
                if (!Enum.IsDefined(typeof(HeroStatType), pair.Key)) continue;

                var statType = (HeroStatType)pair.Key;
                _growthModel.SetLevel(statType, pair.Value);
            }
        }

        private EquipmentSaveData ExtractEquipmentData()
        {
            var data = new EquipmentSaveData();
            foreach (var item in _equipmentModel.GetAllItems())
            {
                if (item.OwnedCount.Value <= 0 && item.Level.Value <= 0) continue;

                data.Items[item.Id] = new EquipmentItemSaveEntry
                {
                    OwnedCount = item.OwnedCount.Value,
                    Level = item.Level.Value
                };
            }

            foreach (EquipmentType type in Enum.GetValues(typeof(EquipmentType)))
            {
                int equippedId = _equipmentModel.GetEquippedId(type);
                if (equippedId != EquipmentModel.UNEQUIPPED)
                {
                    data.Equipped[(int)type] = equippedId;
                }
            }

            return data;
        }

        private void ApplyEquipmentData(EquipmentSaveData data)
        {
            if (data == null) return;

            foreach (var pair in data.Items)
            {
                var state = _equipmentModel.GetOrCreateItemState(pair.Key);
                state.OwnedCount.Value = pair.Value.OwnedCount;
                state.Level.Value = pair.Value.Level;
            }

            foreach (var pair in data.Equipped)
            {
                if (!Enum.IsDefined(typeof(EquipmentType), pair.Key)) continue;

                var type = (EquipmentType)pair.Key;
                _equipmentModel.Equip(type, pair.Value);
            }
        }

        private void ApplySkillData(SkillSaveData data)
        {
            if (data == null) return;

            foreach (var pair in data.Items)
            {
                var state = _skillModel.GetOrCreateItemState(pair.Key);
                state.OwnedCount.Value = pair.Value.OwnedCount;
                state.Level.Value = pair.Value.Level;
            }

            foreach (var pair in data.Equipped)
            {
                if (pair.Key < 0 || pair.Key >= SkillModel.MAX_SLOTS) continue;
                _skillModel.Equip(pair.Key, pair.Value);
            }
        }

        private void ApplyPetData(PetSaveData data)
        {
            if (data == null) return;

            foreach (var pair in data.Items)
            {
                var state = _petModel.GetOrCreateItemState(pair.Key);
                state.OwnedCount.Value = pair.Value.OwnedCount;
                state.Level.Value = pair.Value.Level;
            }

            foreach (var pair in data.Equipped)
            {
                if (pair.Key < 0 || pair.Key >= PetModel.MAX_SLOTS) continue;
                _petModel.Equip(pair.Key, pair.Value);
            }
        }

        private void OnAutoSaveTick(float deltaTime)
        {
            Save();
        }

        private void OnStatLevelUp(StatLevelUpMessage message)
        {
            MarkDirty();
        }

        private void OnStageCleared(StageClearedMessage message)
        {
            MarkDirty();
        }

        private void OnChapterCleared(ChapterClearedMessage message)
        {
            MarkDirty();
        }

        private void OnEquipmentChanged<T>(T message)
        {
            MarkDirty();
        }

        private void OnCurrencyChanged(CurrencyChangedMessage message)
        {
            MarkDirty();
        }

        private void OnOfflineRewardClaimed(OfflineRewardClaimedMessage message)
        {
            Save();
        }

        private void OnSkillChanged<T>(T message)
        {
            MarkDirty();
        }

        private void OnPetChanged<T>(T message)
        {
            MarkDirty();
        }

        private IEnumerator DebounceSaveCoroutine()
        {
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

        private static BigNumber ParseBigNumber(string str)
        {
            if (string.IsNullOrEmpty(str)) return BigNumber.Zero;

            int eIndex = str.IndexOf('e');
            if (eIndex >= 0)
            {
                double c = double.Parse(str.Substring(0, eIndex), CultureInfo.InvariantCulture);
                int e = int.Parse(str.Substring(eIndex + 1), CultureInfo.InvariantCulture);
                return new BigNumber(c, e);
            }

            double val = double.Parse(str, CultureInfo.InvariantCulture);
            return new BigNumber(val, 0);
        }
    }
}
