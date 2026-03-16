namespace IdleRPG.Mine
{
    /// <summary>
    /// 광산 보드판 개별 블록의 상태.
    /// </summary>
    public enum MineBlockState
    {
        /// <summary>가려진 상태 (바위)</summary>
        Hidden = 0,
        /// <summary>파괴됨, 아이템이 보이지만 아직 수집하지 않음</summary>
        Revealed = 1,
        /// <summary>아이템 수집 완료 또는 빈칸</summary>
        Collected = 2,
        /// <summary>계단 (다음 층 이동 아이콘)</summary>
        Staircase = 3
    }
}
