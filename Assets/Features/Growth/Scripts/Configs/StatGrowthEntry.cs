using System;
using IdleRPG.Hero;

namespace IdleRPG.Growth
{
    /// <summary>
    /// 개별 스탯의 성장 파라미터를 정의하는 설정 데이터.
    /// 스탯 값 공식과 레벨업 비용 공식, 최대 레벨, 언락 조건을 포함한다.
    /// </summary>
    /// <remarks>
    /// <para>지수 성장: <c>value = BaseValue * (1 + GrowthRate)^level</c></para>
    /// <para>선형 성장: <c>value = BaseValue + Increment * level</c></para>
    /// <para>비용: <c>cost = BaseCost * (1 + CostGrowthRate)^level</c></para>
    /// </remarks>
    [Serializable]
    public class StatGrowthEntry
    {
        /// <summary>대상 스탯 유형</summary>
        public HeroStatType StatType;

        /// <summary>true이면 지수 성장, false이면 선형 성장 공식을 사용한다.</summary>
        public bool IsExponential;

        /// <summary>레벨 0에서의 기본값</summary>
        public double BaseValue;

        /// <summary>지수 성장률. <see cref="IsExponential"/>이 true일 때 사용한다.</summary>
        public double GrowthRate;

        /// <summary>레벨당 선형 증가량. <see cref="IsExponential"/>이 false일 때 사용한다.</summary>
        public double Increment;

        /// <summary>레벨 1에서의 레벨업 비용</summary>
        public double BaseCost;

        /// <summary>레벨당 비용 성장률. <c>cost = BaseCost * (1 + CostGrowthRate)^level</c></summary>
        public double CostGrowthRate;

        /// <summary>최대 레벨. 0이면 제한 없음</summary>
        public int MaxLevel;

        /// <summary>이 스탯이 해금되려면 만렙이어야 하는 선행 스탯</summary>
        public HeroStatType PrerequisiteStat;

        /// <summary>선행 스탯 조건이 있는지 여부</summary>
        public bool HasPrerequisite;

        /// <summary>
        /// BigNumber 계열 스탯인지 여부.
        /// <see cref="HeroStatType.Attack"/>, <see cref="HeroStatType.Hp"/>,
        /// <see cref="HeroStatType.HpRegen"/>이 해당한다.
        /// </summary>
        public bool IsBigNumberStat;

        /// <summary>
        /// 체력 재생 전용: 최대 체력 대비 추가 재생 비율 (0.00001 = 0.001%).
        /// 체력이 증가하면 재생량도 소폭 증가하는 효과를 구현한다.
        /// </summary>
        public double HpRegenPercent;
    }
}
