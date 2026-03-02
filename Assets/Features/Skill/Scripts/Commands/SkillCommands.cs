using Geuneda.Services;

namespace IdleRPG.Skill
{
    /// <summary>
    /// 스킬을 획득하는 커맨드. 첫 획득 시 자동으로 레벨 1로 해금한다.
    /// </summary>
    public struct AcquireSkillCommand : IGameCommand<SkillModel>
    {
        /// <summary>획득할 스킬 ID</summary>
        public int Id;

        /// <summary>획득 수량</summary>
        public int Count;

        /// <inheritdoc />
        public void Execute(SkillModel model, IMessageBrokerService messageBroker)
        {
            var state = model.GetOrCreateItemState(Id);
            bool isFirst = !state.IsUnlocked;

            model.AddItems(Id, Count);

            if (isFirst)
            {
                model.SetLevel(Id, 1);
            }

            messageBroker.Publish(new SkillAcquiredMessage
            {
                Id = Id,
                Count = Count,
                IsFirstAcquisition = isFirst
            });
        }
    }

    /// <summary>
    /// 스킬을 강화하는 커맨드. 같은 스킬 소재를 소모하고 레벨을 증가시킨다.
    /// </summary>
    public struct UpgradeSkillCommand : IGameCommand<SkillModel>
    {
        /// <summary>강화할 스킬 ID</summary>
        public int Id;

        /// <summary>소모할 소재 개수</summary>
        public int RequiredCount;

        /// <inheritdoc />
        public void Execute(SkillModel model, IMessageBrokerService messageBroker)
        {
            var state = model.GetOrCreateItemState(Id);
            model.ConsumeItems(Id, RequiredCount);
            int newLevel = state.Level.Value + 1;
            model.SetLevel(Id, newLevel);

            messageBroker.Publish(new SkillUpgradedMessage
            {
                Id = Id,
                NewLevel = newLevel
            });
        }
    }

    /// <summary>
    /// 스킬을 슬롯에 장착하는 커맨드.
    /// </summary>
    public struct EquipSkillCommand : IGameCommand<SkillModel>
    {
        /// <summary>장착 슬롯 인덱스</summary>
        public int SlotIndex;

        /// <summary>장착할 스킬 ID</summary>
        public int SkillId;

        /// <inheritdoc />
        public void Execute(SkillModel model, IMessageBrokerService messageBroker)
        {
            int previousId = model.GetEquippedSkillId(SlotIndex);
            model.Equip(SlotIndex, SkillId);

            messageBroker.Publish(new SkillEquippedMessage
            {
                SlotIndex = SlotIndex,
                PreviousId = previousId,
                NewId = SkillId
            });
        }
    }

    /// <summary>
    /// 스킬을 슬롯에서 해제하는 커맨드.
    /// </summary>
    public struct UnequipSkillCommand : IGameCommand<SkillModel>
    {
        /// <summary>해제할 슬롯 인덱스</summary>
        public int SlotIndex;

        /// <inheritdoc />
        public void Execute(SkillModel model, IMessageBrokerService messageBroker)
        {
            int previousId = model.GetEquippedSkillId(SlotIndex);
            if (previousId == SkillModel.UNEQUIPPED) return;

            model.Unequip(SlotIndex);

            messageBroker.Publish(new SkillUnequippedMessage
            {
                SlotIndex = SlotIndex,
                PreviousId = previousId
            });
        }
    }
}
