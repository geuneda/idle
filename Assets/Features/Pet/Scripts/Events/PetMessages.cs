using Geuneda.Services;
using IdleRPG.Core;

namespace IdleRPG.Pet
{
    /// <summary>
    /// 펫 획득 시 발행되는 메시지.
    /// </summary>
    public struct PetAcquiredMessage : IMessage
    {
        /// <summary>획득한 펫 ID</summary>
        public int Id;

        /// <summary>획득 수량</summary>
        public int Count;

        /// <summary>이번 획득으로 처음 해금되었는지 여부</summary>
        public bool IsFirstAcquisition;
    }

    /// <summary>
    /// 펫 강화(레벨업) 완료 시 발행되는 메시지.
    /// </summary>
    public struct PetUpgradedMessage : IMessage
    {
        /// <summary>강화된 펫 ID</summary>
        public int Id;

        /// <summary>강화 후 레벨</summary>
        public int NewLevel;
    }

    /// <summary>
    /// 펫 슬롯 장착 시 발행되는 메시지.
    /// </summary>
    public struct PetEquippedMessage : IMessage
    {
        /// <summary>장착 슬롯 인덱스 (0~4)</summary>
        public int SlotIndex;

        /// <summary>이전에 장착되어 있던 펫 ID. 미장착이었으면 -1</summary>
        public int PreviousId;

        /// <summary>새로 장착된 펫 ID</summary>
        public int NewId;
    }

    /// <summary>
    /// 펫 슬롯 해제 시 발행되는 메시지.
    /// </summary>
    public struct PetUnequippedMessage : IMessage
    {
        /// <summary>해제된 슬롯 인덱스</summary>
        public int SlotIndex;

        /// <summary>해제된 펫 ID</summary>
        public int PreviousId;
    }

    /// <summary>
    /// 펫 보유효과 합산값이 변경되었을 때 발행되는 메시지.
    /// </summary>
    public struct PetEffectsChangedMessage : IMessage
    {
        /// <summary>총 펫 보유효과 공격력 보너스 %</summary>
        public BigNumber TotalPossessionAttackPercent;
    }

    /// <summary>
    /// 펫 슬롯이 새로 해금되었을 때 발행되는 메시지.
    /// </summary>
    public struct PetSlotUnlockedMessage : IMessage
    {
        /// <summary>해금된 슬롯 인덱스</summary>
        public int SlotIndex;
    }
}
