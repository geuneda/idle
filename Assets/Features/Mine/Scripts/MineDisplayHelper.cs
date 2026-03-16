namespace IdleRPG.Mine
{
    /// <summary>
    /// 광산 컨텐츠 표시 이름 헬퍼.
    /// </summary>
    public static class MineDisplayHelper
    {
        /// <summary>
        /// 도구 종류의 표시 이름을 반환한다.
        /// </summary>
        public static string GetToolDisplayName(MineToolType tool)
        {
            return tool switch
            {
                MineToolType.Pickaxe => "곡괭이",
                MineToolType.Bomb => "폭탄",
                MineToolType.Dynamite => "다이너마이트",
                _ => "알 수 없음"
            };
        }

        /// <summary>
        /// 블록 보상 종류의 표시 이름을 반환한다.
        /// </summary>
        public static string GetRewardDisplayName(MineBlockRewardType rewardType)
        {
            return rewardType switch
            {
                MineBlockRewardType.Empty => "빈칸",
                MineBlockRewardType.Ore => "광물",
                MineBlockRewardType.Pickaxe => "곡괭이",
                MineBlockRewardType.Bomb => "폭탄",
                MineBlockRewardType.Dynamite => "다이너마이트",
                MineBlockRewardType.ExpFragment => "경험치 파편",
                MineBlockRewardType.TreasureChest => "보물상자",
                _ => "알 수 없음"
            };
        }
    }
}
