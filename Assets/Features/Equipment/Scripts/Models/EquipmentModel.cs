using System.Collections.Generic;
using Geuneda.DataExtensions;
using IdleRPG.Core;

namespace IdleRPG.Equipment
{
    /// <summary>
    /// 장비 시스템의 런타임 데이터 모델. <see cref="CollectibleModel{TSlotKey}"/>을 상속하여
    /// 장비 타입별 장착 슬롯을 관리한다.
    /// </summary>
    public class EquipmentModel : CollectibleModel<EquipmentType>
    {
        private readonly ObservableDictionary<EquipmentType, int> _equipped;

        /// <summary>장비 타입별 장착 상태의 읽기 전용 뷰. 값이 -1이면 미장착</summary>
        public IObservableDictionaryReader<EquipmentType, int> Equipped => _equipped;

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

        /// <inheritdoc />
        public override int GetEquippedId(EquipmentType type)
        {
            return _equipped.TryGetValue(type, out var id) ? id : UNEQUIPPED;
        }

        /// <summary>해당 타입 슬롯에 장비가 장착되어 있는지 확인한다.</summary>
        public bool IsEquipped(EquipmentType type)
        {
            return GetEquippedId(type) != UNEQUIPPED;
        }

        /// <inheritdoc />
        public override bool IsEquipped(int itemId)
        {
            foreach (var kvp in _equipped)
            {
                if (kvp.Value == itemId) return true;
            }
            return false;
        }

        /// <inheritdoc />
        public override void Equip(EquipmentType type, int id)
        {
            _equipped[type] = id;
        }

        /// <inheritdoc />
        public override void Unequip(EquipmentType type)
        {
            _equipped[type] = UNEQUIPPED;
        }
    }
}
