namespace IdleRPG.Core
{
    /// <summary>
    /// 스킬 보유효과 보너스를 제공하는 인터페이스.
    /// <see cref="Equipment.EquipmentService"/>가 스탯 계산 시 스킬 보유효과를 포함하기 위해 사용한다.
    /// </summary>
    public interface ISkillBonusProvider
    {
        /// <summary>전체 스킬 보유효과 공격력 퍼센트 합산</summary>
        BigNumber TotalPossessionAttackPercent { get; }
    }
}
