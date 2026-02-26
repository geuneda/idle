namespace IdleRPG.Hero
{
    /// <summary>
    /// 영웅의 상태를 나타내는 열거형. 애니메이터의 State 파라미터와 매핑된다.
    /// </summary>
    public enum HeroState
    {
        /// <summary>대기 상태</summary>
        Idle = 0,

        /// <summary>공격 상태</summary>
        Attack = 1,

        /// <summary>사망 상태</summary>
        Death = 2
    }
}
