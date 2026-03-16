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
        Gem = 1,

        /// <summary>유물 재화. 던전에서 획득하며 유물 강화에 사용한다.</summary>
        Relic = 2,

        /// <summary>광물. 광산 컨텐츠 전용 재화.</summary>
        MineOre = 10,

        /// <summary>곡괭이. 광산 블록 파괴에 사용한다.</summary>
        Pickaxe = 11,

        /// <summary>폭탄. 광산 3x3 범위 파괴에 사용한다.</summary>
        Bomb = 12,

        /// <summary>다이너마이트. 광산 십자 범위 파괴에 사용한다.</summary>
        Dynamite = 13,

        /// <summary>경험치 파편. 영웅 성장 재료.</summary>
        ExpFragment = 14
    }
}
