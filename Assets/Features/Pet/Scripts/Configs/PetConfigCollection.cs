using System;
using System.Collections.Generic;

namespace IdleRPG.Pet
{
    /// <summary>
    /// 펫 설정 컬렉션. ID/등급별 조회 딕셔너리를 관리한다.
    /// </summary>
    [Serializable]
    public class PetConfigCollection
    {
        /// <summary>모든 펫 항목</summary>
        public List<PetEntry> Entries = new();

        /// <summary>강화 비용 설정</summary>
        public PetUpgradeConfig UpgradeConfig = new();

        /// <summary>슬롯 해금 설정</summary>
        public List<PetSlotConfig> SlotConfigs = new();

        private Dictionary<int, PetEntry> _lookup;
        private Dictionary<PetGrade, List<PetEntry>> _byGrade;
        private Dictionary<int, PetSlotConfig> _slotLookup;

        /// <summary>
        /// ID/등급별 조회 딕셔너리를 구축한다. 역직렬화 후 반드시 호출해야 한다.
        /// </summary>
        public void BuildLookup()
        {
            _lookup = new Dictionary<int, PetEntry>(Entries.Count);
            _byGrade = new Dictionary<PetGrade, List<PetEntry>>();

            foreach (var entry in Entries)
            {
                _lookup[entry.Id] = entry;

                if (!_byGrade.TryGetValue(entry.Grade, out var list))
                {
                    list = new List<PetEntry>();
                    _byGrade[entry.Grade] = list;
                }
                list.Add(entry);
            }

            _slotLookup = new Dictionary<int, PetSlotConfig>(SlotConfigs.Count);
            foreach (var slot in SlotConfigs)
            {
                _slotLookup[slot.SlotIndex] = slot;
            }
        }

        /// <summary>ID로 펫 설정을 조회한다.</summary>
        public PetEntry GetEntry(int id)
        {
            return _lookup != null && _lookup.TryGetValue(id, out var entry) ? entry : null;
        }

        /// <summary>모든 펫 항목을 반환한다.</summary>
        public IReadOnlyList<PetEntry> GetAllEntries()
        {
            return Entries;
        }

        /// <summary>등급별 펫 항목을 반환한다.</summary>
        public IReadOnlyList<PetEntry> GetEntriesByGrade(PetGrade grade)
        {
            if (_byGrade != null && _byGrade.TryGetValue(grade, out var list))
                return list;
            return Array.Empty<PetEntry>();
        }

        /// <summary>슬롯 인덱스로 해금 설정을 조회한다.</summary>
        public PetSlotConfig GetSlotConfig(int slotIndex)
        {
            return _slotLookup != null && _slotLookup.TryGetValue(slotIndex, out var config) ? config : null;
        }
    }
}
