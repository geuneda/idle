using Geuneda.Services;
using IdleRPG.Core;
using IdleRPG.Economy;
using IdleRPG.Hero;

namespace IdleRPG.Growth
{
    /// <summary>
    /// <see cref="IHeroGrowthService"/>의 구현체.
    /// 스탯별 레벨업, 비용 계산, 스탯 갱신, 전투력 재계산을 처리한다.
    /// </summary>
    public class HeroGrowthService : IHeroGrowthService
    {
        private readonly GrowthConfig _config;
        private readonly HeroModel _heroModel;
        private readonly ICurrencyService _currencyService;
        private readonly IMessageBrokerService _messageBroker;

        /// <inheritdoc />
        public HeroGrowthModel Model { get; }

        /// <inheritdoc />
        public BigNumber CombatPower { get; private set; }

        /// <summary>
        /// <see cref="HeroGrowthService"/>를 생성한다.
        /// </summary>
        /// <param name="config">성장 설정 데이터</param>
        /// <param name="growthModel">스탯별 레벨 모델</param>
        /// <param name="heroModel">영웅 상태 모델</param>
        /// <param name="currencyService">재화 서비스</param>
        /// <param name="messageBroker">이벤트 통신용 메시지 브로커</param>
        public HeroGrowthService(
            GrowthConfig config,
            HeroGrowthModel growthModel,
            HeroModel heroModel,
            ICurrencyService currencyService,
            IMessageBrokerService messageBroker)
        {
            _config = config;
            Model = growthModel;
            _heroModel = heroModel;
            _currencyService = currencyService;
            _messageBroker = messageBroker;

            ApplyAllStats();
            RecalculateCombatPower();
        }

        /// <inheritdoc />
        public bool TryLevelUp(HeroStatType statType)
        {
            if (!IsUnlocked(statType)) return false;
            if (IsMaxLevel(statType)) return false;

            var entry = _config.GetEntry(statType);
            if (entry == null) return false;

            int currentLevel = Model.GetLevel(statType);
            BigNumber cost = StatGrowthFormula.CalculateCost(entry, currentLevel);

            if (!_currencyService.TrySpend(CurrencyType.Gold, cost)) return false;

            int newLevel = Model.IncrementLevel(statType);
            ApplyStat(entry, newLevel);

            if (statType == HeroStatType.Hp)
            {
                RefreshHpRegen();
            }

            BigNumber previousPower = CombatPower;
            RecalculateCombatPower();

            _messageBroker.Publish(new StatLevelUpMessage
            {
                StatType = statType,
                NewLevel = newLevel,
                SpentGold = cost
            });

            _messageBroker.Publish(new CombatPowerChangedMessage
            {
                PreviousPower = previousPower,
                CurrentPower = CombatPower
            });

            return true;
        }

        /// <inheritdoc />
        public bool IsUnlocked(HeroStatType statType)
        {
            var entry = _config.GetEntry(statType);
            if (entry == null) return false;
            if (!entry.HasPrerequisite) return true;

            return IsMaxLevel(entry.PrerequisiteStat);
        }

        /// <inheritdoc />
        public bool IsMaxLevel(HeroStatType statType)
        {
            var entry = _config.GetEntry(statType);
            if (entry == null) return false;
            if (entry.MaxLevel <= 0) return false;

            return Model.GetLevel(statType) >= entry.MaxLevel;
        }

        /// <inheritdoc />
        public BigNumber GetLevelUpCost(HeroStatType statType)
        {
            var entry = _config.GetEntry(statType);
            if (entry == null) return BigNumber.Zero;

            int currentLevel = Model.GetLevel(statType);
            return StatGrowthFormula.CalculateCost(entry, currentLevel);
        }

        /// <inheritdoc />
        public BigNumber GetCurrentStatValue(HeroStatType statType)
        {
            var entry = _config.GetEntry(statType);
            if (entry == null) return BigNumber.Zero;

            int level = Model.GetLevel(statType);

            if (statType == HeroStatType.HpRegen)
            {
                return StatGrowthFormula.CalculateHpRegen(entry, level, _heroModel.MaxHp.Value);
            }

            return StatGrowthFormula.CalculateStatValueAsBigNumber(entry, level);
        }

        /// <inheritdoc />
        public void RecalculateCombatPower()
        {
            CombatPower = CombatPowerCalculator.Calculate(_heroModel);
        }

        /// <summary>
        /// 모든 스탯의 현재 레벨에 맞는 값을 <see cref="HeroModel"/>에 적용한다.
        /// 서비스 초기화 및 저장 데이터 복원 시 호출한다.
        /// </summary>
        private void ApplyAllStats()
        {
            foreach (var entry in _config.Entries)
            {
                int level = Model.GetLevel(entry.StatType);
                ApplyStat(entry, level);
            }
        }

        /// <summary>
        /// 개별 스탯의 값을 <see cref="HeroModel"/>에 적용한다.
        /// </summary>
        /// <param name="entry">스탯 성장 설정</param>
        /// <param name="level">적용할 레벨</param>
        private void ApplyStat(StatGrowthEntry entry, int level)
        {
            if (entry.IsBigNumberStat)
            {
                BigNumber value;
                if (entry.StatType == HeroStatType.HpRegen)
                {
                    value = StatGrowthFormula.CalculateHpRegen(entry, level, _heroModel.MaxHp.Value);
                }
                else
                {
                    value = StatGrowthFormula.CalculateStatValueAsBigNumber(entry, level);
                }

                _heroModel.SetBigNumberStat(entry.StatType, value);
            }
            else
            {
                double value = StatGrowthFormula.CalculateStatValue(entry, level);
                _heroModel.SetFloatStat(entry.StatType, (float)value);
            }
        }

        /// <summary>
        /// 체력 변경 후 체력 재생량을 최대 체력 기준으로 재계산한다.
        /// </summary>
        private void RefreshHpRegen()
        {
            var regenEntry = _config.GetEntry(HeroStatType.HpRegen);
            if (regenEntry == null) return;

            int regenLevel = Model.GetLevel(HeroStatType.HpRegen);
            BigNumber regenValue = StatGrowthFormula.CalculateHpRegen(
                regenEntry, regenLevel, _heroModel.MaxHp.Value);
            _heroModel.SetBigNumberStat(HeroStatType.HpRegen, regenValue);
        }
    }
}
