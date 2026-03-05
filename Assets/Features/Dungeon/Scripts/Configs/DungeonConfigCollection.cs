using System;
using System.Collections.Generic;
using IdleRPG.Economy;

namespace IdleRPG.Dungeon
{
    /// <summary>
    /// 던전 레벨 설정의 런타임 컨테이너. 타입별/레벨별 O(1) 조회를 지원한다.
    /// </summary>
    [Serializable]
    public class DungeonConfigCollection
    {
        /// <summary>모든 던전 레벨 설정 항목</summary>
        public List<DungeonLevelConfig> Entries = new List<DungeonLevelConfig>();

        private Dictionary<DungeonType, Dictionary<int, DungeonLevelConfig>> _lookup;
        private Dictionary<DungeonType, int> _maxLevels;

        /// <summary>
        /// 내부 룩업 테이블을 구축한다. 역직렬화 후 반드시 호출해야 한다.
        /// </summary>
        public void BuildLookup()
        {
            _lookup = new Dictionary<DungeonType, Dictionary<int, DungeonLevelConfig>>();
            _maxLevels = new Dictionary<DungeonType, int>();

            foreach (var entry in Entries)
            {
                if (!_lookup.TryGetValue(entry.Type, out var levelDict))
                {
                    levelDict = new Dictionary<int, DungeonLevelConfig>();
                    _lookup[entry.Type] = levelDict;
                }

                levelDict[entry.Level] = entry;

                if (!_maxLevels.TryGetValue(entry.Type, out var currentMax) || entry.Level > currentMax)
                {
                    _maxLevels[entry.Type] = entry.Level;
                }
            }
        }

        /// <summary>
        /// 지정된 던전 타입과 레벨의 설정을 반환한다.
        /// </summary>
        /// <param name="type">던전 종류</param>
        /// <param name="level">레벨 (1-based)</param>
        /// <returns>해당 레벨 설정. 없으면 null</returns>
        public DungeonLevelConfig GetConfig(DungeonType type, int level)
        {
            if (_lookup != null
                && _lookup.TryGetValue(type, out var levelDict)
                && levelDict.TryGetValue(level, out var config))
            {
                return config;
            }

            return null;
        }

        /// <summary>
        /// 지정된 던전 타입의 최대 레벨을 반환한다.
        /// </summary>
        /// <param name="type">던전 종류</param>
        /// <returns>최대 레벨. 데이터가 없으면 0</returns>
        public int GetMaxLevel(DungeonType type)
        {
            if (_maxLevels != null && _maxLevels.TryGetValue(type, out var max))
            {
                return max;
            }

            return 0;
        }

        /// <summary>
        /// 던전 타입에 대응하는 보상 재화 타입을 반환한다.
        /// </summary>
        /// <param name="type">던전 종류</param>
        /// <returns>보상 <see cref="CurrencyType"/></returns>
        public static CurrencyType GetRewardCurrencyType(DungeonType type)
        {
            return type switch
            {
                DungeonType.Gem => CurrencyType.Gem,
                DungeonType.Gold => CurrencyType.Gold,
                DungeonType.Relic => CurrencyType.Relic,
                _ => CurrencyType.Gold
            };
        }
    }
}
