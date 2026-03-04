using Geuneda.Services;

namespace IdleRPG.Pet
{
    /// <summary>
    /// 펫을 획득하는 커맨드. 첫 획득 시 자동으로 레벨 1로 해금한다.
    /// </summary>
    public struct AcquirePetCommand : IGameCommand<PetModel>
    {
        /// <summary>획득할 펫 ID</summary>
        public int Id;

        /// <summary>획득 수량</summary>
        public int Count;

        /// <inheritdoc />
        public void Execute(PetModel model, IMessageBrokerService messageBroker)
        {
            var state = model.GetOrCreateItemState(Id);
            bool isFirst = !state.IsUnlocked;

            model.AddItems(Id, Count);

            if (isFirst)
            {
                model.SetLevel(Id, 1);
            }

            messageBroker.Publish(new PetAcquiredMessage
            {
                Id = Id,
                Count = Count,
                IsFirstAcquisition = isFirst
            });
        }
    }

    /// <summary>
    /// 펫을 강화하는 커맨드. 같은 펫 소재를 소모하고 레벨을 증가시킨다.
    /// </summary>
    public struct UpgradePetCommand : IGameCommand<PetModel>
    {
        /// <summary>강화할 펫 ID</summary>
        public int Id;

        /// <summary>소모할 소재 개수</summary>
        public int RequiredCount;

        /// <inheritdoc />
        public void Execute(PetModel model, IMessageBrokerService messageBroker)
        {
            var state = model.GetOrCreateItemState(Id);
            model.ConsumeItems(Id, RequiredCount);
            int newLevel = state.Level.Value + 1;
            model.SetLevel(Id, newLevel);

            messageBroker.Publish(new PetUpgradedMessage
            {
                Id = Id,
                NewLevel = newLevel
            });
        }
    }

    /// <summary>
    /// 펫을 슬롯에 장착하는 커맨드.
    /// </summary>
    public struct EquipPetCommand : IGameCommand<PetModel>
    {
        /// <summary>장착 슬롯 인덱스</summary>
        public int SlotIndex;

        /// <summary>장착할 펫 ID</summary>
        public int PetId;

        /// <inheritdoc />
        public void Execute(PetModel model, IMessageBrokerService messageBroker)
        {
            int previousId = model.GetEquippedPetId(SlotIndex);
            model.Equip(SlotIndex, PetId);

            messageBroker.Publish(new PetEquippedMessage
            {
                SlotIndex = SlotIndex,
                PreviousId = previousId,
                NewId = PetId
            });
        }
    }

    /// <summary>
    /// 펫을 슬롯에서 해제하는 커맨드.
    /// </summary>
    public struct UnequipPetCommand : IGameCommand<PetModel>
    {
        /// <summary>해제할 슬롯 인덱스</summary>
        public int SlotIndex;

        /// <inheritdoc />
        public void Execute(PetModel model, IMessageBrokerService messageBroker)
        {
            int previousId = model.GetEquippedPetId(SlotIndex);
            if (previousId == PetModel.UNEQUIPPED) return;

            model.Unequip(SlotIndex);

            messageBroker.Publish(new PetUnequippedMessage
            {
                SlotIndex = SlotIndex,
                PreviousId = previousId
            });
        }
    }
}
