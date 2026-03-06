using System;
using System.Collections.Generic;

namespace IdleRPG.Economy
{
    /// <summary>
    /// 재화 표시 설정의 런타임 컨테이너. 타입별 O(1) 조회를 지원한다.
    /// </summary>
    [Serializable]
    public class CurrencyDisplayConfigCollection
    {
        /// <summary>모든 재화 표시 설정 항목</summary>
        public List<CurrencyDisplayConfig> Entries = new List<CurrencyDisplayConfig>();

        private Dictionary<CurrencyType, CurrencyDisplayConfig> _lookup;

        /// <summary>
        /// 내부 룩업 테이블을 구축한다. 역직렬화 후 반드시 호출해야 한다.
        /// </summary>
        public void BuildLookup()
        {
            _lookup = new Dictionary<CurrencyType, CurrencyDisplayConfig>();

            foreach (var entry in Entries)
            {
                _lookup[entry.Type] = entry;
            }
        }

        /// <summary>
        /// 지정된 재화 타입의 표시 설정을 반환한다.
        /// </summary>
        /// <param name="type">재화 종류</param>
        /// <returns>해당 재화의 표시 설정. 없으면 null</returns>
        public CurrencyDisplayConfig GetConfig(CurrencyType type)
        {
            if (_lookup != null && _lookup.TryGetValue(type, out var config))
            {
                return config;
            }

            return null;
        }
    }
}
