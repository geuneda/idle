using IdleRPG.Core;
using IdleRPG.Hero;

namespace IdleRPG.Growth
{
    /// <summary>
    /// 영웅 성장 서비스 인터페이스.
    /// 스탯별 레벨업, 비용 조회, 언락 조건 확인, 전투력 계산 기능을 제공한다.
    /// </summary>
    /// <remarks>
    /// <see cref="Geuneda.Services.MainInstaller"/>에 바인딩하여 사용한다.
    /// </remarks>
    public interface IHeroGrowthService
    {
        /// <summary>성장 데이터 모델</summary>
        HeroGrowthModel Model { get; }

        /// <summary>현재 전투력</summary>
        BigNumber CombatPower { get; }

        /// <summary>
        /// 지정한 스탯의 레벨업을 시도한다.
        /// 골드가 부족하거나, 만렙이거나, 해금 조건이 미충족이면 실패한다.
        /// </summary>
        /// <param name="statType">레벨업할 스탯 유형</param>
        /// <returns>레벨업 성공 여부</returns>
        bool TryLevelUp(HeroStatType statType);

        /// <summary>
        /// 지정한 스탯이 해금되었는지 확인한다.
        /// 선행 스탯이 만렙에 도달해야 해금된다.
        /// </summary>
        /// <param name="statType">확인할 스탯 유형</param>
        /// <returns>해금 여부</returns>
        bool IsUnlocked(HeroStatType statType);

        /// <summary>
        /// 지정한 스탯이 만렙에 도달했는지 확인한다.
        /// </summary>
        /// <param name="statType">확인할 스탯 유형</param>
        /// <returns>만렙 여부. 최대 레벨이 0(무제한)이면 항상 false</returns>
        bool IsMaxLevel(HeroStatType statType);

        /// <summary>
        /// 지정한 스탯의 현재 레벨에서 다음 레벨로 올리기 위한 비용을 반환한다.
        /// </summary>
        /// <param name="statType">조회할 스탯 유형</param>
        /// <returns>레벨업 비용 (골드)</returns>
        BigNumber GetLevelUpCost(HeroStatType statType);

        /// <summary>
        /// 지정한 스탯의 현재 값을 반환한다.
        /// BigNumber 스탯은 BigNumber 형태, float 스탯은 double → BigNumber 변환으로 반환한다.
        /// </summary>
        /// <param name="statType">조회할 스탯 유형</param>
        /// <returns>현재 스탯 값</returns>
        BigNumber GetCurrentStatValue(HeroStatType statType);

        /// <summary>
        /// 전투력을 재계산한다. 스탯 변경 후 자동으로 호출된다.
        /// </summary>
        void RecalculateCombatPower();
    }
}
