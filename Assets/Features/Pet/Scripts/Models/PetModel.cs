using System.Collections.Generic;
using Geuneda.DataExtensions;

namespace IdleRPG.Pet
{
    /// <summary>
    /// 펫 시스템의 런타임 데이터 모델. 보유 상태와 5개 장착 슬롯을 관리한다.
    /// </summary>
    public class PetModel
    {
        /// <summary>최대 펫 장착 슬롯 수</summary>
        public const int MAX_SLOTS = 5;

        /// <summary>미장착 상태를 나타내는 센티넬 값</summary>
        public const int UNEQUIPPED = -1;

        private readonly Dictionary<int, PetItemState> _items = new();
        private readonly ObservableList<int> _equippedSlots;

        /// <summary>장착 슬롯 상태의 읽기 전용 뷰. 인덱스가 슬롯 번호, 값이 펫 ID (-1이면 미장착)</summary>
        public IObservableListReader<int> EquippedSlots => _equippedSlots;

        public PetModel()
        {
            var initial = new List<int>(MAX_SLOTS);
            for (int i = 0; i < MAX_SLOTS; i++)
                initial.Add(UNEQUIPPED);
            _equippedSlots = new ObservableList<int>(initial);
        }

        /// <summary>지정 펫의 상태를 반환한다. 없으면 새로 생성하여 등록한다.</summary>
        public PetItemState GetOrCreateItemState(int id)
        {
            if (!_items.TryGetValue(id, out var state))
            {
                state = new PetItemState(id);
                _items[id] = state;
            }
            return state;
        }

        /// <summary>지정 펫의 상태를 반환한다. 없으면 null.</summary>
        public PetItemState GetItemState(int id)
        {
            return _items.TryGetValue(id, out var state) ? state : null;
        }

        /// <summary>해금된 모든 펫 상태를 열거한다.</summary>
        public IEnumerable<PetItemState> GetAllUnlockedItems()
        {
            foreach (var kvp in _items)
            {
                if (kvp.Value.IsUnlocked)
                    yield return kvp.Value;
            }
        }

        /// <summary>모든 펫 상태를 열거한다.</summary>
        public IEnumerable<PetItemState> GetAllItems()
        {
            return _items.Values;
        }

        /// <summary>지정 슬롯에 장착된 펫 ID를 반환한다.</summary>
        public int GetEquippedPetId(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= MAX_SLOTS) return UNEQUIPPED;
            return _equippedSlots[slotIndex];
        }

        /// <summary>해당 펫이 어느 슬롯에 장착되어 있는지 찾는다. 미장착이면 -1.</summary>
        public int FindEquippedSlotIndex(int petId)
        {
            for (int i = 0; i < MAX_SLOTS; i++)
            {
                if (_equippedSlots[i] == petId)
                    return i;
            }
            return -1;
        }

        /// <summary>해당 펫이 장착되어 있는지 확인한다.</summary>
        public bool IsEquipped(int petId)
        {
            return FindEquippedSlotIndex(petId) >= 0;
        }

        /// <summary>펫을 보유 목록에 추가한다.</summary>
        public void AddItems(int id, int count)
        {
            var state = GetOrCreateItemState(id);
            state.OwnedCount.Value += count;
        }

        /// <summary>펫 소재를 소모한다.</summary>
        public void ConsumeItems(int id, int count)
        {
            var state = GetOrCreateItemState(id);
            state.OwnedCount.Value -= count;
        }

        /// <summary>펫 레벨을 설정한다.</summary>
        public void SetLevel(int id, int level)
        {
            var state = GetOrCreateItemState(id);
            state.Level.Value = level;
        }

        /// <summary>지정 슬롯에 펫을 장착한다.</summary>
        public void Equip(int slotIndex, int petId)
        {
            if (slotIndex < 0 || slotIndex >= MAX_SLOTS) return;
            _equippedSlots[slotIndex] = petId;
        }

        /// <summary>지정 슬롯의 펫을 해제한다.</summary>
        public void Unequip(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= MAX_SLOTS) return;
            _equippedSlots[slotIndex] = UNEQUIPPED;
        }
    }
}
