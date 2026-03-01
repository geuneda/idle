using Geuneda.Services;

namespace IdleRPG.Equipment
{
    /// <summary>
    /// 장비를 획득하는 커맨드. 첫 획득 시 자동으로 레벨 1로 해금한다.
    /// </summary>
    public struct AcquireEquipmentCommand : IGameCommand<EquipmentModel>
    {
        /// <summary>획득할 장비 ID</summary>
        public int Id;

        /// <summary>획득 수량</summary>
        public int Count;

        /// <inheritdoc />
        public void Execute(EquipmentModel model, IMessageBrokerService messageBroker)
        {
            var state = model.GetOrCreateItemState(Id);
            bool isFirst = !state.IsUnlocked;

            model.AddItems(Id, Count);

            if (isFirst)
            {
                model.SetLevel(Id, 1);
            }

            messageBroker.Publish(new EquipmentAcquiredMessage
            {
                Id = Id,
                Count = Count,
                IsFirstAcquisition = isFirst
            });
        }
    }

    /// <summary>
    /// 장비를 강화하는 커맨드. 같은 장비 소재를 소모하고 레벨을 증가시킨다.
    /// </summary>
    public struct UpgradeEquipmentCommand : IGameCommand<EquipmentModel>
    {
        /// <summary>강화할 장비 ID</summary>
        public int Id;

        /// <summary>소모할 소재 개수</summary>
        public int RequiredCount;

        /// <inheritdoc />
        public void Execute(EquipmentModel model, IMessageBrokerService messageBroker)
        {
            var state = model.GetOrCreateItemState(Id);
            model.ConsumeItems(Id, RequiredCount);
            int newLevel = state.Level.Value + 1;
            model.SetLevel(Id, newLevel);

            messageBroker.Publish(new EquipmentUpgradedMessage
            {
                Id = Id,
                NewLevel = newLevel
            });
        }
    }

    /// <summary>
    /// 장비를 슬롯에 장착하는 커맨드.
    /// </summary>
    public struct EquipCommand : IGameCommand<EquipmentModel>
    {
        /// <summary>장착 슬롯 타입</summary>
        public EquipmentType SlotType;

        /// <summary>장착할 장비 ID</summary>
        public int EquipmentId;

        /// <inheritdoc />
        public void Execute(EquipmentModel model, IMessageBrokerService messageBroker)
        {
            int previousId = model.GetEquippedId(SlotType);
            model.Equip(SlotType, EquipmentId);

            messageBroker.Publish(new EquipmentEquippedMessage
            {
                SlotType = SlotType,
                PreviousId = previousId,
                NewId = EquipmentId
            });
        }
    }

    /// <summary>
    /// 장비를 슬롯에서 해제하는 커맨드.
    /// </summary>
    public struct UnequipCommand : IGameCommand<EquipmentModel>
    {
        /// <summary>해제할 슬롯 타입</summary>
        public EquipmentType SlotType;

        /// <inheritdoc />
        public void Execute(EquipmentModel model, IMessageBrokerService messageBroker)
        {
            int previousId = model.GetEquippedId(SlotType);
            if (previousId == EquipmentModel.UNEQUIPPED) return;

            model.Unequip(SlotType);

            messageBroker.Publish(new EquipmentUnequippedMessage
            {
                SlotType = SlotType,
                PreviousId = previousId
            });
        }
    }
}
