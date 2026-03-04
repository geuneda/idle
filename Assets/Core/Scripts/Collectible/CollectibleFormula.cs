using System;

namespace IdleRPG.Core
{
    /// <summary>
    /// 수집형 아이템의 공통 계산 함수 모음. 상태를 갖지 않으며 결정론적 결과를 보장한다.
    /// 장비, 스킬, 펫이 공유하는 보유효과, 강화 비용, 최대 레벨 계산을 담당한다.
    /// </summary>
    public static class CollectibleFormula
    {
        /// <summary>
        /// 보유효과 수치를 계산한다.
        /// 공식: BasePossessionEffect + PossessionEffectPerLevel * (level - 1)
        /// </summary>
        /// <param name="entry">아이템 설정</param>
        /// <param name="level">현재 레벨 (1 이상)</param>
        /// <returns>보유효과 수치</returns>
        public static BigNumber CalcPossessionEffect(ICollectibleEntry entry, int level)
        {
            if (level <= 0) return BigNumber.Zero;
            double value = entry.BasePossessionEffect + entry.PossessionEffectPerLevel * (level - 1);
            return new BigNumber(value, 0);
        }

        /// <summary>
        /// 강화에 필요한 소재 수를 반환한다.
        /// </summary>
        /// <param name="config">강화 비용 설정</param>
        /// <param name="currentLevel">현재 레벨</param>
        /// <returns>필요한 소재 개수</returns>
        public static int GetRequiredCount(UpgradeConfig config, int currentLevel)
        {
            if (currentLevel <= 0) return 0;
            int index = currentLevel - 1;

            if (index < config.RequiredCountPerLevel.Count)
                return config.RequiredCountPerLevel[index];

            int overflow = index - config.RequiredCountPerLevel.Count;
            double value = config.OverflowBase * Math.Pow(1.0 + config.OverflowGrowthRate, overflow + 1);
            return (int)Math.Ceiling(value);
        }

        /// <summary>
        /// 최대 레벨에 도달했는지 확인한다.
        /// </summary>
        /// <param name="entry">아이템 설정</param>
        /// <param name="currentLevel">현재 레벨</param>
        /// <returns>최대 레벨이면 true</returns>
        public static bool IsMaxLevel(ICollectibleEntry entry, int currentLevel)
        {
            return entry.MaxLevel > 0 && currentLevel >= entry.MaxLevel;
        }
    }
}
