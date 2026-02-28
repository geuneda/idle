namespace IdleRPG.Economy
{
    /// <summary>
    /// 게임 내 재화 종류를 정의하는 열거형.
    /// </summary>
    public enum CurrencyType
    {
        /// <summary>기본 재화. 적 처치, 스테이지 클리어 등으로 획득한다.</summary>
        Gold = 0,

        /// <summary>프리미엄 재화 (하드 커런시). 주로 과금이나 특별 보상으로 획득한다.</summary>
        Gem = 1
    }
}
