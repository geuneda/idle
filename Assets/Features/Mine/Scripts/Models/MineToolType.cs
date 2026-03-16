namespace IdleRPG.Mine
{
    /// <summary>
    /// 광산에서 사용할 수 있는 도구 종류.
    /// </summary>
    public enum MineToolType
    {
        /// <summary>곡괭이. 1칸 파괴.</summary>
        Pickaxe = 0,
        /// <summary>폭탄. 3x3 범위 파괴.</summary>
        Bomb = 1,
        /// <summary>다이너마이트. 십자(행+열) 범위 파괴.</summary>
        Dynamite = 2
    }
}
