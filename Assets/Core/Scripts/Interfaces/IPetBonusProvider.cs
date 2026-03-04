namespace IdleRPG.Core
{
    /// <summary>
    /// 펫 보유효과 보너스를 제공하는 인터페이스.
    /// <see cref="Equipment.EquipmentService"/>가 스탯 계산 시 펫 보유효과를 포함하기 위해 사용한다.
    /// </summary>
    public interface IPetBonusProvider
    {
        /// <summary>전체 펫 보유효과 공격력 퍼센트 합산</summary>
        BigNumber TotalPossessionAttackPercent { get; }
    }
}
