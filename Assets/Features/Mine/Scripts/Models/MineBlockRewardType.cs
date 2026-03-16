namespace IdleRPG.Mine
{
    /// <summary>
    /// 광산 블록 파괴 시 출현하는 보상 종류.
    /// </summary>
    public enum MineBlockRewardType
    {
        /// <summary>빈칸</summary>
        Empty = 0,
        /// <summary>광물</summary>
        Ore = 1,
        /// <summary>곡괭이</summary>
        Pickaxe = 2,
        /// <summary>폭탄</summary>
        Bomb = 3,
        /// <summary>다이너마이트</summary>
        Dynamite = 4,
        /// <summary>경험치 파편</summary>
        ExpFragment = 5,
        /// <summary>보물상자</summary>
        TreasureChest = 6
    }
}
