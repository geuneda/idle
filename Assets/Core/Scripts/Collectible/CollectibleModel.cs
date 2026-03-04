using System.Collections.Generic;
using Geuneda.DataExtensions;

namespace IdleRPG.Core
{
    /// <summary>
    /// 수집형 아이템의 공통 데이터 모델. 보유 상태와 장착 슬롯을 관리한다.
    /// <typeparamref name="TSlotKey"/>로 슬롯 키 타입을 매개변수화한다.
    /// Pet/Skill은 int(인덱스), Equipment는 EquipmentType(타입별)을 사용한다.
    /// </summary>
    /// <typeparam name="TSlotKey">슬롯 식별 키 타입</typeparam>
    public abstract class CollectibleModel<TSlotKey>
    {
        /// <summary>미장착 상태를 나타내는 센티넬 값</summary>
        public const int UNEQUIPPED = -1;

        private readonly Dictionary<int, CollectibleItemState> _items = new();

        /// <summary>
        /// 지정 아이템의 상태를 반환한다. 없으면 새로 생성하여 등록한다.
        /// </summary>
        /// <param name="id">아이템 고유 식별자</param>
        /// <returns>아이템 상태</returns>
        public CollectibleItemState GetOrCreateItemState(int id)
        {
            if (!_items.TryGetValue(id, out var state))
            {
                state = new CollectibleItemState(id);
                _items[id] = state;
            }
            return state;
        }

        /// <summary>지정 아이템의 상태를 반환한다. 없으면 null.</summary>
        public CollectibleItemState GetItemState(int id)
        {
            return _items.TryGetValue(id, out var state) ? state : null;
        }

        /// <summary>해금된 모든 아이템 상태를 열거한다.</summary>
        public IEnumerable<CollectibleItemState> GetAllUnlockedItems()
        {
            foreach (var kvp in _items)
            {
                if (kvp.Value.IsUnlocked)
                    yield return kvp.Value;
            }
        }

        /// <summary>모든 아이템 상태를 열거한다.</summary>
        public IEnumerable<CollectibleItemState> GetAllItems()
        {
            return _items.Values;
        }

        /// <summary>아이템을 보유 목록에 추가한다.</summary>
        public void AddItems(int id, int count)
        {
            var state = GetOrCreateItemState(id);
            state.OwnedCount.Value += count;
        }

        /// <summary>아이템 소재를 소모한다.</summary>
        public void ConsumeItems(int id, int count)
        {
            var state = GetOrCreateItemState(id);
            state.OwnedCount.Value -= count;
        }

        /// <summary>아이템 레벨을 설정한다.</summary>
        public void SetLevel(int id, int level)
        {
            var state = GetOrCreateItemState(id);
            state.Level.Value = level;
        }

        /// <summary>지정 슬롯에 장착된 아이템 ID를 반환한다.</summary>
        public abstract int GetEquippedId(TSlotKey key);

        /// <summary>해당 아이템이 장착되어 있는지 확인한다.</summary>
        public abstract bool IsEquipped(int itemId);

        /// <summary>지정 슬롯에 아이템을 장착한다.</summary>
        public abstract void Equip(TSlotKey key, int itemId);

        /// <summary>지정 슬롯의 아이템을 해제한다.</summary>
        public abstract void Unequip(TSlotKey key);
    }

    /// <summary>
    /// 인덱스 기반 슬롯 모델. Pet과 Skill이 사용한다.
    /// 슬롯을 0부터 시작하는 정수 인덱스로 관리한다.
    /// </summary>
    public class IndexedCollectibleModel : CollectibleModel<int>
    {
        private readonly ObservableList<int> _equippedSlots;

        /// <summary>최대 슬롯 수</summary>
        public int MaxSlots { get; }

        /// <summary>장착 슬롯 상태의 읽기 전용 뷰</summary>
        public IObservableListReader<int> EquippedSlots => _equippedSlots;

        public IndexedCollectibleModel(int maxSlots)
        {
            MaxSlots = maxSlots;
            var initial = new System.Collections.Generic.List<int>(maxSlots);
            for (int i = 0; i < maxSlots; i++)
                initial.Add(UNEQUIPPED);
            _equippedSlots = new ObservableList<int>(initial);
        }

        /// <inheritdoc />
        public override int GetEquippedId(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= MaxSlots) return UNEQUIPPED;
            return _equippedSlots[slotIndex];
        }

        /// <summary>해당 아이템이 어느 슬롯에 장착되어 있는지 찾는다. 미장착이면 -1.</summary>
        public int FindEquippedSlotIndex(int itemId)
        {
            for (int i = 0; i < MaxSlots; i++)
            {
                if (_equippedSlots[i] == itemId)
                    return i;
            }
            return -1;
        }

        /// <inheritdoc />
        public override bool IsEquipped(int itemId)
        {
            return FindEquippedSlotIndex(itemId) >= 0;
        }

        /// <inheritdoc />
        public override void Equip(int slotIndex, int itemId)
        {
            if (slotIndex < 0 || slotIndex >= MaxSlots) return;
            _equippedSlots[slotIndex] = itemId;
        }

        /// <inheritdoc />
        public override void Unequip(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= MaxSlots) return;
            _equippedSlots[slotIndex] = UNEQUIPPED;
        }
    }
}
