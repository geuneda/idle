using System;
using System.Collections.Generic;
using IdleRPG.Hero;

namespace IdleRPG.Growth
{
    /// <summary>
    /// 영웅 성장 시스템 전체 설정. 스탯별 <see cref="StatGrowthEntry"/>를 보유하며
    /// <see cref="HeroStatType"/>으로 조회할 수 있다.
    /// </summary>
    /// <remarks>
    /// 추후 Google Sheets → ConfigsProvider 파이프라인으로 전환 시
    /// ScriptableObject 기반으로 교체할 수 있도록 POCO로 구현한다.
    /// </remarks>
    [Serializable]
    public class GrowthConfig
    {
        /// <summary>스탯별 성장 설정 목록</summary>
        public List<StatGrowthEntry> Entries;

        private Dictionary<HeroStatType, StatGrowthEntry> _lookup;

        /// <summary>
        /// 기본 밸런스 값으로 초기화된 <see cref="GrowthConfig"/>를 생성한다.
        /// </summary>
        public GrowthConfig()
        {
            Entries = new List<StatGrowthEntry>
            {
                new StatGrowthEntry
                {
                    StatType = HeroStatType.Attack,
                    IsExponential = true,
                    BaseValue = 10,
                    GrowthRate = 0.032,
                    BaseCost = 300_000,
                    CostGrowthRate = 0.006,
                    MaxLevel = 0,
                    IsBigNumberStat = true
                },
                new StatGrowthEntry
                {
                    StatType = HeroStatType.Hp,
                    IsExponential = true,
                    BaseValue = 100,
                    GrowthRate = 0.024,
                    BaseCost = 55,
                    CostGrowthRate = 0.0075,
                    MaxLevel = 0,
                    IsBigNumberStat = true
                },
                new StatGrowthEntry
                {
                    StatType = HeroStatType.HpRegen,
                    IsExponential = false,
                    BaseValue = 1,
                    Increment = 3.2,
                    BaseCost = 55,
                    CostGrowthRate = 0.0074,
                    MaxLevel = 0,
                    IsBigNumberStat = true,
                    HpRegenPercent = 0.00001
                },
                new StatGrowthEntry
                {
                    StatType = HeroStatType.AttackSpeed,
                    IsExponential = false,
                    BaseValue = 1.0,
                    Increment = 0.01,
                    BaseCost = 300_000,
                    CostGrowthRate = 0.033,
                    MaxLevel = 200,
                    IsBigNumberStat = false
                },
                new StatGrowthEntry
                {
                    StatType = HeroStatType.CritRate,
                    IsExponential = false,
                    BaseValue = 0,
                    Increment = 0.001,
                    BaseCost = 190_000,
                    CostGrowthRate = 0.036,
                    MaxLevel = 0,
                    IsBigNumberStat = false
                },
                new StatGrowthEntry
                {
                    StatType = HeroStatType.CritDamage,
                    IsExponential = false,
                    BaseValue = 1.5,
                    Increment = 0.01,
                    BaseCost = 68,
                    CostGrowthRate = 0.007,
                    MaxLevel = 0,
                    IsBigNumberStat = false
                },
                new StatGrowthEntry
                {
                    StatType = HeroStatType.DoubleShot,
                    IsExponential = false,
                    BaseValue = 0,
                    Increment = 0.001,
                    BaseCost = 500_000,
                    CostGrowthRate = 0.04,
                    MaxLevel = 0,
                    HasPrerequisite = true,
                    PrerequisiteStat = HeroStatType.AttackSpeed,
                    IsBigNumberStat = false
                },
                new StatGrowthEntry
                {
                    StatType = HeroStatType.TripleShot,
                    IsExponential = false,
                    BaseValue = 0,
                    Increment = 0.001,
                    BaseCost = 1_000_000,
                    CostGrowthRate = 0.04,
                    MaxLevel = 350,
                    HasPrerequisite = true,
                    PrerequisiteStat = HeroStatType.DoubleShot,
                    IsBigNumberStat = false
                },
                new StatGrowthEntry
                {
                    StatType = HeroStatType.AdvancedAttack,
                    IsExponential = false,
                    BaseValue = 0,
                    Increment = 0.005,
                    BaseCost = 2_000_000,
                    CostGrowthRate = 0.04,
                    MaxLevel = 150,
                    HasPrerequisite = true,
                    PrerequisiteStat = HeroStatType.TripleShot,
                    IsBigNumberStat = false
                },
                new StatGrowthEntry
                {
                    StatType = HeroStatType.EnemyBonusDamage,
                    IsExponential = false,
                    BaseValue = 0,
                    Increment = 0.005,
                    BaseCost = 5_000_000,
                    CostGrowthRate = 0.04,
                    MaxLevel = 0,
                    HasPrerequisite = true,
                    PrerequisiteStat = HeroStatType.AdvancedAttack,
                    IsBigNumberStat = false
                }
            };

            BuildLookup();
        }

        /// <summary>
        /// 지정한 <see cref="HeroStatType"/>에 해당하는 성장 엔트리를 반환한다.
        /// </summary>
        /// <param name="statType">조회할 스탯 유형</param>
        /// <returns>해당 스탯의 성장 설정. 없으면 null</returns>
        public StatGrowthEntry GetEntry(HeroStatType statType)
        {
            if (_lookup == null) BuildLookup();
            return _lookup.TryGetValue(statType, out var entry) ? entry : null;
        }

        /// <summary>
        /// 룩업 딕셔너리를 재구성한다. 역직렬화 후 호출해야 한다.
        /// </summary>
        public void BuildLookup()
        {
            _lookup = new Dictionary<HeroStatType, StatGrowthEntry>();
            foreach (var entry in Entries)
            {
                _lookup[entry.StatType] = entry;
            }
        }
    }
}
