using System.Collections.Generic;
using Geuneda.DataExtensions;

namespace IdleRPG.Equipment
{
    /// <summary>
    /// 장비 시스템의 런타임 데이터 모델.
    /// 도감 상태, 장착 상태, 보유 수량을 관리한다.
    /// </summary>
    public class EquipmentModel
    {
        /// <summary>미장착 상태를 나타내는 센티넬 값</summary>
        public const int UNEQUIPPED = -1;

        private readonly Dictionary<int, EquipmentItemState> _items = new();
        private readonly ObservableDictionary<EquipmentType, int> _equipped;

        /// <summary>장비 타입별 장착 상태의 읽기 전용 뷰. 값이 -1이면 미장착</summary>
        public IObservableDictionaryReader<EquipmentType, int> Equipped => _equipped;

        /// <summary>
        /// 모델을 초기화한다. 모든 슬롯을 미장착 상태로 설정한다.
        /// </summary>
        public EquipmentModel()
        {
            var initial = new Dictionary<EquipmentType, int>
            {
                { EquipmentType.Weapon, UNEQUIPPED },
                { EquipmentType.Armor, UNEQUIPPED },
                { EquipmentType.Accessory, UNEQUIPPED }
            };
            _equipped = new ObservableDictionary<EquipmentType, int>(initial);
        }

        /// <summary>
        /// 지정 장비의 상태를 반환한다. 없으면 새로 생성하여 등록한다.
        /// </summary>
        /// <param name="id">장비 고유 식별자</param>
        /// <returns>장비 아이템 상태</returns>
        public EquipmentItemState GetOrCreateItemState(int id)
        {
            if (!_items.TryGetValue(id, out var state))
            {
                state = new EquipmentItemState(id);
                _items[id] = state;
            }

            return state;
        }

        /// <summary>
        /// 지정 장비의 상태를 반환한다. 없으면 null.
        /// </summary>
        /// <param name="id">장비 고유 식별자</param>
        /// <returns>장비 아이템 상태. 없으면 null</returns>
        public EquipmentItemState GetItemState(int id)
        {
            return _items.TryGetValue(id, out var state) ? state : null;
        }

        /// <summary>
        /// 해금된 모든 장비 상태를 열거한다.
        /// </summary>
        /// <returns>해금된 장비 상태 열거자</returns>
        public IEnumerable<EquipmentItemState> GetAllUnlockedItems()
        {
            foreach (var kvp in _items)
            {
                if (kvp.Value.IsUnlocked)
                    yield return kvp.Value;
            }
        }

        /// <summary>
        /// 모든 장비 상태를 열거한다.
        /// </summary>
        /// <returns>모든 장비 상태 열거자</returns>
        public IEnumerable<EquipmentItemState> GetAllItems()
        {
            return _items.Values;
        }

        /// <summary>
        /// 지정 슬롯에 장착된 장비 ID를 반환한다.
        /// </summary>
        /// <param name="type">장비 슬롯 타입</param>
        /// <returns>장착된 장비 ID. 미장착이면 -1</returns>
        public int GetEquippedId(EquipmentType type)
        {
            return _equipped.TryGetValue(type, out var id) ? id : UNEQUIPPED;
        }

        /// <summary>
        /// 해당 슬롯에 장비가 장착되어 있는지 확인한다.
        /// </summary>
        /// <param name="type">장비 슬롯 타입</param>
        /// <returns>장착 여부</returns>
        public bool IsEquipped(EquipmentType type)
        {
            return GetEquippedId(type) != UNEQUIPPED;
        }

        /// <summary>
        /// 장비를 보유 목록에 추가한다.
        /// </summary>
        /// <param name="id">장비 고유 식별자</param>
        /// <param name="count">추가할 수량</param>
        public void AddItems(int id, int count)
        {
            var state = GetOrCreateItemState(id);
            state.OwnedCount.Value += count;
        }

        /// <summary>
        /// 장비 소재를 소모한다.
        /// </summary>
        /// <param name="id">장비 고유 식별자</param>
        /// <param name="count">소모할 수량</param>
        public void ConsumeItems(int id, int count)
        {
            var state = GetOrCreateItemState(id);
            state.OwnedCount.Value -= count;
        }

        /// <summary>
        /// 장비 레벨을 설정한다.
        /// </summary>
        /// <param name="id">장비 고유 식별자</param>
        /// <param name="level">설정할 레벨</param>
        public void SetLevel(int id, int level)
        {
            var state = GetOrCreateItemState(id);
            state.Level.Value = level;
        }

        /// <summary>
        /// 지정 슬롯에 장비를 장착한다.
        /// </summary>
        /// <param name="type">장비 슬롯 타입</param>
        /// <param name="id">장착할 장비 ID</param>
        public void Equip(EquipmentType type, int id)
        {
            _equipped[type] = id;
        }

        /// <summary>
        /// 지정 슬롯의 장비를 해제한다.
        /// </summary>
        /// <param name="type">장비 슬롯 타입</param>
        public void Unequip(EquipmentType type)
        {
            _equipped[type] = UNEQUIPPED;
        }
    }
}
