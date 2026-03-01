using Geuneda.Services;
using IdleRPG.Core;

namespace IdleRPG.Equipment
{
    /// <summary>
    /// 장비 획득 시 발행되는 메시지.
    /// </summary>
    public struct EquipmentAcquiredMessage : IMessage
    {
        /// <summary>획득한 장비 ID</summary>
        public int Id;

        /// <summary>획득 수량</summary>
        public int Count;

        /// <summary>이번 획득으로 처음 해금되었는지 여부</summary>
        public bool IsFirstAcquisition;
    }

    /// <summary>
    /// 장비 강화(레벨업) 완료 시 발행되는 메시지.
    /// </summary>
    public struct EquipmentUpgradedMessage : IMessage
    {
        /// <summary>강화된 장비 ID</summary>
        public int Id;

        /// <summary>강화 후 레벨</summary>
        public int NewLevel;
    }

    /// <summary>
    /// 장비 장착 시 발행되는 메시지.
    /// </summary>
    public struct EquipmentEquippedMessage : IMessage
    {
        /// <summary>장착 슬롯 타입</summary>
        public EquipmentType SlotType;

        /// <summary>이전에 장착되어 있던 장비 ID. 미장착이었으면 -1</summary>
        public int PreviousId;

        /// <summary>새로 장착된 장비 ID</summary>
        public int NewId;
    }

    /// <summary>
    /// 장비 해제 시 발행되는 메시지.
    /// </summary>
    public struct EquipmentUnequippedMessage : IMessage
    {
        /// <summary>해제된 슬롯 타입</summary>
        public EquipmentType SlotType;

        /// <summary>해제된 장비 ID</summary>
        public int PreviousId;
    }

    /// <summary>
    /// 장비 효과(보유효과/장착효과) 합산값이 변경되었을 때 발행되는 메시지.
    /// </summary>
    public struct EquipmentEffectsChangedMessage : IMessage
    {
        /// <summary>총 공격력 보너스 %</summary>
        public BigNumber TotalAttackBonusPercent;

        /// <summary>총 체력 보너스 %</summary>
        public BigNumber TotalHpBonusPercent;
    }
}
