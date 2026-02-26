namespace IdleRPG.Hero
{
    /// <summary>
    /// 영웅의 스탯 유형을 정의하는 열거형. 성장/강화 시스템에서 스탯을 식별하는 데 사용한다.
    /// </summary>
    public enum HeroStatType
    {
        /// <summary>체력</summary>
        Hp,

        /// <summary>공격력</summary>
        Attack,

        /// <summary>방어력</summary>
        Defense,

        /// <summary>공격 속도</summary>
        AttackSpeed,

        /// <summary>체력 재생</summary>
        HpRegen,

        /// <summary>치명타 확률</summary>
        CritRate,

        /// <summary>치명타 피해 배율</summary>
        CritDamage
    }
}
