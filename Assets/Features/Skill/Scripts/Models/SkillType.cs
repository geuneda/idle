namespace IdleRPG.Skill
{
    /// <summary>스킬 실행 타입</summary>
    public enum SkillType
    {
        /// <summary>단일 대상 데미지</summary>
        SingleTarget = 0,
        /// <summary>다수 대상 타격</summary>
        MultiHit = 1,
        /// <summary>광역 데미지</summary>
        AoE = 2,
        /// <summary>지속 광역 데미지</summary>
        DoT = 3,
        /// <summary>스탯 버프</summary>
        Buff = 4,
        /// <summary>체력 회복</summary>
        Heal = 5,
        /// <summary>소환 (지속시간 동안 주기적 공격)</summary>
        Summon = 6
    }
}
