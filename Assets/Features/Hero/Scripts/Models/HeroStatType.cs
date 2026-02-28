namespace IdleRPG.Hero
{
    /// <summary>
    /// 영웅의 스탯 유형을 정의하는 열거형. 성장/강화 시스템에서 스탯을 식별하는 데 사용한다.
    /// </summary>
    public enum HeroStatType
    {
        /// <summary>체력</summary>
        Hp = 0,

        /// <summary>공격력</summary>
        Attack = 1,

        /// <summary>공격 속도</summary>
        AttackSpeed = 2,

        /// <summary>체력 재생</summary>
        HpRegen = 3,

        /// <summary>치명타 확률</summary>
        CritRate = 4,

        /// <summary>치명타 피해 배율</summary>
        CritDamage = 5,

        /// <summary>이중 사격 발동 확률. <see cref="AttackSpeed"/> 만렙 후 해금된다.</summary>
        DoubleShot = 6,

        /// <summary>삼중 사격 발동 확률. <see cref="DoubleShot"/> 만렙 후 해금된다.</summary>
        TripleShot = 7,

        /// <summary>고급 공격 피해 증가율. <see cref="TripleShot"/> 만렙 후 해금된다.</summary>
        AdvancedAttack = 8,

        /// <summary>적 추가 피해 배율. <see cref="AdvancedAttack"/> 만렙 후 해금된다.</summary>
        EnemyBonusDamage = 9
    }
}
