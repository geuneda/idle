using System;
using IdleRPG.Core;

namespace IdleRPG.Growth
{
    /// <summary>
    /// 스탯 성장 공식을 담은 순수 함수 모음.
    /// 모든 메서드는 부작용 없이 입력값만으로 결과를 반환하며 테스트가 용이하다.
    /// </summary>
    public static class StatGrowthFormula
    {
        /// <summary>
        /// 지정한 레벨에서의 스탯 값을 계산한다.
        /// </summary>
        /// <param name="entry">스탯 성장 설정</param>
        /// <param name="level">현재 레벨</param>
        /// <returns>해당 레벨에서의 스탯 값</returns>
        /// <remarks>
        /// <para>지수: <c>BaseValue * (1 + GrowthRate)^level</c></para>
        /// <para>선형: <c>BaseValue + Increment * level</c></para>
        /// </remarks>
        public static double CalculateStatValue(StatGrowthEntry entry, int level)
        {
            if (level <= 0) return entry.BaseValue;

            if (entry.IsExponential)
            {
                return entry.BaseValue * Math.Pow(1.0 + entry.GrowthRate, level);
            }

            return entry.BaseValue + entry.Increment * level;
        }

        /// <summary>
        /// 지정한 레벨에서의 스탯 값을 <see cref="BigNumber"/>로 반환한다.
        /// </summary>
        /// <param name="entry">스탯 성장 설정</param>
        /// <param name="level">현재 레벨</param>
        /// <returns>BigNumber로 변환된 스탯 값</returns>
        public static BigNumber CalculateStatValueAsBigNumber(StatGrowthEntry entry, int level)
        {
            double raw = CalculateStatValue(entry, level);
            return new BigNumber(raw, 0);
        }

        /// <summary>
        /// 현재 레벨에서 다음 레벨로 올리기 위한 비용을 계산한다.
        /// </summary>
        /// <param name="entry">스탯 성장 설정</param>
        /// <param name="currentLevel">현재 레벨</param>
        /// <returns>다음 레벨업에 필요한 골드</returns>
        /// <remarks>
        /// <c>cost = BaseCost * (1 + CostGrowthRate)^currentLevel</c>
        /// </remarks>
        public static BigNumber CalculateCost(StatGrowthEntry entry, int currentLevel)
        {
            double cost = entry.BaseCost * Math.Pow(1.0 + entry.CostGrowthRate, currentLevel);
            return new BigNumber(cost, 0);
        }

        /// <summary>
        /// 체력 재생량을 계산한다. 레벨 기반 재생량에 최대 체력 비례 보너스를 더한다.
        /// </summary>
        /// <param name="entry">체력 재생 스탯 설정</param>
        /// <param name="level">체력 재생 레벨</param>
        /// <param name="maxHp">현재 최대 체력</param>
        /// <returns>초당 체력 재생량</returns>
        public static BigNumber CalculateHpRegen(StatGrowthEntry entry, int level, BigNumber maxHp)
        {
            double baseRegen = CalculateStatValue(entry, level);
            BigNumber regenValue = new BigNumber(baseRegen, 0);

            if (entry.HpRegenPercent > 0)
            {
                BigNumber hpBonus = maxHp * entry.HpRegenPercent;
                regenValue = regenValue + hpBonus;
            }

            return regenValue;
        }
    }
}
