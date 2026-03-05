namespace IdleRPG.Dungeon
{
    /// <summary>
    /// 던전 종류를 정의하는 열거형.
    /// </summary>
    public enum DungeonType
    {
        /// <summary>보석 던전. 클리어 시 보석(Gem) 재화를 보상으로 획득한다.</summary>
        Gem = 0,

        /// <summary>골드 던전. 클리어 시 골드(Gold) 재화를 보상으로 획득한다.</summary>
        Gold = 1,

        /// <summary>유물 던전. 클리어 시 유물(Relic) 재화를 보상으로 획득한다.</summary>
        Relic = 2
    }
}
