using System;
using System.Collections.Generic;

namespace IdleRPG.Equipment
{
    /// <summary>
    /// 장비 설정 데이터 컬렉션. 모든 장비 항목과 강화 비용 설정을 포함한다.
    /// </summary>
    [Serializable]
    public class EquipmentConfigCollection
    {
        /// <summary>모든 장비 항목 목록</summary>
        public List<EquipmentEntry> Entries = new();

        /// <summary>강화 비용 설정 (모든 장비 공용)</summary>
        public EquipmentUpgradeConfig UpgradeConfig = new();

        private Dictionary<int, EquipmentEntry> _lookup;
        private Dictionary<EquipmentType, List<EquipmentEntry>> _byType;

        /// <summary>
        /// ID 기반 조회 딕셔너리를 구축한다. 역직렬화 후 반드시 호출해야 한다.
        /// </summary>
        public void BuildLookup()
        {
            _lookup = new Dictionary<int, EquipmentEntry>();
            _byType = new Dictionary<EquipmentType, List<EquipmentEntry>>();

            foreach (var entry in Entries)
            {
                _lookup[entry.Id] = entry;

                if (!_byType.TryGetValue(entry.Type, out var list))
                {
                    list = new List<EquipmentEntry>();
                    _byType[entry.Type] = list;
                }

                list.Add(entry);
            }
        }

        /// <summary>
        /// ID로 장비 항목을 조회한다.
        /// </summary>
        /// <param name="id">장비 고유 식별자</param>
        /// <returns>장비 항목. 없으면 null</returns>
        public EquipmentEntry GetEntry(int id)
        {
            if (_lookup == null) BuildLookup();
            return _lookup.TryGetValue(id, out var entry) ? entry : null;
        }

        /// <summary>
        /// 장비 타입별 항목 목록을 반환한다. 등급 오름차순, 같은 등급 내 SubKind 오름차순으로 정렬되어 있다.
        /// </summary>
        /// <param name="type">장비 대분류</param>
        /// <returns>해당 타입의 장비 목록. 없으면 빈 목록</returns>
        public IReadOnlyList<EquipmentEntry> GetEntriesByType(EquipmentType type)
        {
            if (_byType == null) BuildLookup();
            return _byType.TryGetValue(type, out var list) ? list : Array.Empty<EquipmentEntry>();
        }
    }
}
