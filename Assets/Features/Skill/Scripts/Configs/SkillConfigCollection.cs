using System;
using System.Collections.Generic;

namespace IdleRPG.Skill
{
    /// <summary>
    /// 스킬 설정 컬렉션. ID/등급별 조회 딕셔너리를 관리한다.
    /// </summary>
    [Serializable]
    public class SkillConfigCollection
    {
        /// <summary>모든 스킬 항목</summary>
        public List<SkillEntry> Entries = new();

        /// <summary>강화 비용 설정</summary>
        public SkillUpgradeConfig UpgradeConfig = new();

        /// <summary>슬롯 해금 설정</summary>
        public List<SkillSlotConfig> SlotConfigs = new();

        private Dictionary<int, SkillEntry> _lookup;
        private Dictionary<SkillGrade, List<SkillEntry>> _byGrade;
        private Dictionary<int, SkillSlotConfig> _slotLookup;

        /// <summary>
        /// ID/등급별 조회 딕셔너리를 구축한다. 역직렬화 후 반드시 호출해야 한다.
        /// </summary>
        public void BuildLookup()
        {
            _lookup = new Dictionary<int, SkillEntry>(Entries.Count);
            _byGrade = new Dictionary<SkillGrade, List<SkillEntry>>();

            foreach (var entry in Entries)
            {
                _lookup[entry.Id] = entry;

                if (!_byGrade.TryGetValue(entry.Grade, out var list))
                {
                    list = new List<SkillEntry>();
                    _byGrade[entry.Grade] = list;
                }
                list.Add(entry);
            }

            _slotLookup = new Dictionary<int, SkillSlotConfig>(SlotConfigs.Count);
            foreach (var slot in SlotConfigs)
            {
                _slotLookup[slot.SlotIndex] = slot;
            }
        }

        /// <summary>ID로 스킬 설정을 조회한다.</summary>
        public SkillEntry GetEntry(int id)
        {
            return _lookup != null && _lookup.TryGetValue(id, out var entry) ? entry : null;
        }

        /// <summary>모든 스킬 항목을 반환한다.</summary>
        public IReadOnlyList<SkillEntry> GetAllEntries()
        {
            return Entries;
        }

        /// <summary>등급별 스킬 항목을 반환한다.</summary>
        public IReadOnlyList<SkillEntry> GetEntriesByGrade(SkillGrade grade)
        {
            if (_byGrade != null && _byGrade.TryGetValue(grade, out var list))
                return list;
            return Array.Empty<SkillEntry>();
        }

        /// <summary>슬롯 인덱스로 해금 설정을 조회한다.</summary>
        public SkillSlotConfig GetSlotConfig(int slotIndex)
        {
            return _slotLookup != null && _slotLookup.TryGetValue(slotIndex, out var config) ? config : null;
        }
    }
}
