using System;
using Geuneda.Services;
using IdleRPG.Equipment;

namespace IdleRPG.Core
{
    /// <summary>
    /// 장비 보유/장착 상태의 저장/로드를 담당한다.
    /// </summary>
    public class EquipmentSaveCollector : ISaveDataCollector
    {
        private static readonly EquipmentType[] CachedEquipmentTypes =
            (EquipmentType[])Enum.GetValues(typeof(EquipmentType));

        private readonly EquipmentModel _equipmentModel;
        private Action _markDirty;

        public EquipmentSaveCollector(EquipmentModel equipmentModel)
        {
            _equipmentModel = equipmentModel;
        }

        /// <inheritdoc />
        public void ExtractTo(GameSaveData saveData)
        {
            var data = new EquipmentSaveData();
            foreach (var item in _equipmentModel.GetAllItems())
            {
                if (item.OwnedCount.Value <= 0 && item.Level.Value <= 0) continue;

                data.Items[item.Id] = new EquipmentItemSaveEntry
                {
                    OwnedCount = item.OwnedCount.Value,
                    Level = item.Level.Value
                };
            }

            foreach (var type in CachedEquipmentTypes)
            {
                int equippedId = _equipmentModel.GetEquippedId(type);
                if (equippedId != EquipmentModel.UNEQUIPPED)
                {
                    data.Equipped[(int)type] = equippedId;
                }
            }

            saveData.Equipment = data;
        }

        /// <inheritdoc />
        public void ApplyFrom(GameSaveData saveData)
        {
            var data = saveData.Equipment;
            if (data == null) return;

            foreach (var pair in data.Items)
            {
                var state = _equipmentModel.GetOrCreateItemState(pair.Key);
                state.OwnedCount.Value = pair.Value.OwnedCount;
                state.Level.Value = pair.Value.Level;
            }

            foreach (var pair in data.Equipped)
            {
                if (!Enum.IsDefined(typeof(EquipmentType), pair.Key)) continue;

                var type = (EquipmentType)pair.Key;
                _equipmentModel.Equip(type, pair.Value);
            }
        }

        /// <inheritdoc />
        public void SubscribeDirtyEvents(IMessageBrokerService broker, Action markDirty)
        {
            _markDirty = markDirty;
            broker.Subscribe<EquipmentAcquiredMessage>(OnEquipmentChanged);
            broker.Subscribe<EquipmentUpgradedMessage>(OnEquipmentChanged);
            broker.Subscribe<EquipmentEquippedMessage>(OnEquipmentChanged);
        }

        /// <inheritdoc />
        public void UnsubscribeDirtyEvents(IMessageBrokerService broker)
        {
            broker.Unsubscribe<EquipmentAcquiredMessage>(this);
            broker.Unsubscribe<EquipmentUpgradedMessage>(this);
            broker.Unsubscribe<EquipmentEquippedMessage>(this);
            _markDirty = null;
        }

        private void OnEquipmentChanged<T>(T message) => _markDirty?.Invoke();
    }
}
