using Geuneda.Services;
using IdleRPG.Core;
using UnityEngine;

namespace IdleRPG.Skill
{
    /// <summary>
    /// 스킬 획득 시 발행되는 메시지.
    /// </summary>
    public struct SkillAcquiredMessage : IMessage
    {
        /// <summary>획득한 스킬 ID</summary>
        public int Id;

        /// <summary>획득 수량</summary>
        public int Count;

        /// <summary>이번 획득으로 처음 해금되었는지 여부</summary>
        public bool IsFirstAcquisition;
    }

    /// <summary>
    /// 스킬 강화(레벨업) 완료 시 발행되는 메시지.
    /// </summary>
    public struct SkillUpgradedMessage : IMessage
    {
        /// <summary>강화된 스킬 ID</summary>
        public int Id;

        /// <summary>강화 후 레벨</summary>
        public int NewLevel;
    }

    /// <summary>
    /// 스킬 슬롯 장착 시 발행되는 메시지.
    /// </summary>
    public struct SkillEquippedMessage : IMessage
    {
        /// <summary>장착 슬롯 인덱스 (0~5)</summary>
        public int SlotIndex;

        /// <summary>이전에 장착되어 있던 스킬 ID. 미장착이었으면 -1</summary>
        public int PreviousId;

        /// <summary>새로 장착된 스킬 ID</summary>
        public int NewId;
    }

    /// <summary>
    /// 스킬 슬롯 해제 시 발행되는 메시지.
    /// </summary>
    public struct SkillUnequippedMessage : IMessage
    {
        /// <summary>해제된 슬롯 인덱스</summary>
        public int SlotIndex;

        /// <summary>해제된 스킬 ID</summary>
        public int PreviousId;
    }

    /// <summary>
    /// 스킬 보유효과 합산값이 변경되었을 때 발행되는 메시지.
    /// </summary>
    public struct SkillEffectsChangedMessage : IMessage
    {
        /// <summary>총 스킬 보유효과 공격력 보너스 %</summary>
        public BigNumber TotalPossessionAttackPercent;
    }

    /// <summary>
    /// 스킬 슬롯이 새로 해금되었을 때 발행되는 메시지.
    /// </summary>
    public struct SkillSlotUnlockedMessage : IMessage
    {
        /// <summary>해금된 슬롯 인덱스</summary>
        public int SlotIndex;
    }

    /// <summary>
    /// 스킬이 전투에서 발동되었을 때 발행되는 메시지. VFX 등 외부 시스템이 구독한다.
    /// </summary>
    public struct SkillActivatedMessage : IMessage
    {
        /// <summary>발동된 스킬의 ID</summary>
        public int SkillId;

        /// <summary>발동 위치</summary>
        public Vector3 Position;
    }

    /// <summary>
    /// 버프 스킬이 적용되었을 때 발행되는 메시지.
    /// </summary>
    public struct SkillBuffAppliedMessage : IMessage
    {
        /// <summary>버프 수치 (공격속도 증가량 등)</summary>
        public float BuffValue;

        /// <summary>버프 지속시간 (초)</summary>
        public float Duration;
    }

    /// <summary>
    /// 힐 스킬이 적용되었을 때 발행되는 메시지.
    /// </summary>
    public struct SkillHealAppliedMessage : IMessage
    {
        /// <summary>힐량</summary>
        public BigNumber HealAmount;
    }

    /// <summary>
    /// HP 비율 회복 스킬이 적용되었을 때 발행되는 메시지. 에너지의 폭발 전용.
    /// </summary>
    public struct SkillHealPercentAppliedMessage : IMessage
    {
        /// <summary>회복할 최대 HP 비율 (%)</summary>
        public float HealPercent;
    }

    /// <summary>
    /// 스킬 쿨타임이 감소되었을 때 발행되는 메시지. 어두운구체 전용.
    /// </summary>
    public struct SkillCooldownReducedMessage : IMessage
    {
        /// <summary>감소된 슬롯 인덱스</summary>
        public int SlotIndex;

        /// <summary>감소 비율 (%)</summary>
        public float ReductionPercent;
    }
}
