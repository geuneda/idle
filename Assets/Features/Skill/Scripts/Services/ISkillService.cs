using System.Collections.Generic;
using IdleRPG.Core;

namespace IdleRPG.Skill
{
    /// <summary>
    /// 스킬 시스템의 비즈니스 로직 인터페이스.
    /// 스킬 획득, 강화, 장착 및 보유효과 적용을 담당한다.
    /// </summary>
    public interface ISkillService : ISkillBonusProvider
    {
        /// <summary>스킬 데이터 모델 (읽기용)</summary>
        SkillModel Model { get; }

        /// <summary>스킬 설정 데이터</summary>
        SkillConfigCollection Config { get; }

        /// <summary>
        /// 스킬을 획득한다. 첫 획득 시 자동으로 레벨 1로 해금한다.
        /// </summary>
        /// <param name="id">스킬 고유 식별자</param>
        /// <param name="count">획득 수량</param>
        void AcquireSkill(int id, int count);

        /// <summary>
        /// 스킬을 강화한다. 같은 스킬 소재를 소모한다.
        /// </summary>
        /// <param name="id">강화할 스킬 ID</param>
        /// <returns>강화 성공 여부</returns>
        bool TryUpgrade(int id);

        /// <summary>
        /// 강화 가능한 모든 스킬을 강화한다.
        /// </summary>
        /// <returns>강화된 횟수</returns>
        int UpgradeAll();

        /// <summary>
        /// 강화 가능 여부를 반환한다.
        /// </summary>
        /// <param name="id">스킬 ID</param>
        /// <returns>강화 가능하면 true</returns>
        bool CanUpgrade(int id);

        /// <summary>
        /// 현재 레벨에서 강화에 필요한 소재 개수를 반환한다.
        /// </summary>
        /// <param name="id">스킬 ID</param>
        /// <returns>필요 소재 개수</returns>
        int GetUpgradeCost(int id);

        /// <summary>
        /// 스킬을 지정 슬롯에 장착한다.
        /// </summary>
        /// <param name="skillId">장착할 스킬 ID</param>
        /// <param name="slotIndex">장착 슬롯 인덱스 (0~5)</param>
        /// <returns>장착 성공 여부</returns>
        bool TryEquip(int skillId, int slotIndex);

        /// <summary>
        /// 지정 슬롯의 스킬을 해제한다.
        /// </summary>
        /// <param name="slotIndex">슬롯 인덱스 (0~5)</param>
        void Unequip(int slotIndex);

        /// <summary>
        /// 빈 슬롯에 최적 스킬을 자동으로 장착한다.
        /// </summary>
        void QuickEquip();

        /// <summary>
        /// 지정 슬롯이 해금되었는지 확인한다.
        /// </summary>
        /// <param name="slotIndex">슬롯 인덱스 (0~5)</param>
        /// <returns>해금 여부</returns>
        bool IsSlotUnlocked(int slotIndex);

        /// <summary>
        /// 현재 해금된 슬롯 수를 반환한다.
        /// </summary>
        /// <returns>해금 슬롯 수 (0~6)</returns>
        int GetUnlockedSlotCount();

        /// <summary>
        /// 지정 스킬의 현재 레벨 기준 보유효과를 반환한다.
        /// </summary>
        /// <param name="id">스킬 ID</param>
        /// <returns>보유효과 수치 (%)</returns>
        BigNumber GetPossessionEffect(int id);

        /// <summary>
        /// 현재 장착된 스킬 ID 목록을 반환한다.
        /// </summary>
        /// <returns>장착된 스킬 ID 리스트 (UNEQUIPPED 제외)</returns>
        IReadOnlyList<int> GetEquippedSkillIds();

        /// <summary>
        /// 보유효과를 재계산하여 합산값을 갱신한다.
        /// </summary>
        void RecalculateAndApplyEffects();
    }
}
