using IdleRPG.Economy;

namespace IdleRPG.Dungeon
{
    /// <summary>
    /// 던전 관련 표시 이름을 제공하는 정적 헬퍼.
    /// </summary>
    public static class DungeonDisplayHelper
    {
        /// <summary>
        /// 던전 종류의 표시 이름을 반환한다.
        /// </summary>
        /// <param name="type">던전 종류</param>
        /// <returns>한국어 표시 이름</returns>
        public static string GetDungeonName(DungeonType type)
        {
            return type switch
            {
                DungeonType.Gem => "보석던전",
                DungeonType.Gold => "골드던전",
                DungeonType.Relic => "유물던전",
                _ => "던전"
            };
        }

        /// <summary>
        /// 재화 종류의 표시 이름을 반환한다.
        /// </summary>
        /// <param name="type">재화 종류</param>
        /// <returns>한국어 표시 이름</returns>
        public static string GetCurrencyName(CurrencyType type)
        {
            return type switch
            {
                CurrencyType.Gold => "골드",
                CurrencyType.Gem => "보석",
                CurrencyType.Relic => "유물",
                _ => "재화"
            };
        }
    }
}
