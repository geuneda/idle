using IdleRPG.Core;

namespace IdleRPG.Equipment
{
    /// <summary>
    /// 장비 시스템의 비즈니스 로직 인터페이스.
    /// 장비 획득, 강화, 장착 및 효과 적용을 담당한다.
    /// </summary>
    public interface IEquipmentService
    {
        /// <summary>장비 데이터 모델 (읽기용)</summary>
        EquipmentModel Model { get; }

        /// <summary>장비 설정 데이터</summary>
        EquipmentConfigCollection Config { get; }

        /// <summary>
        /// 장비를 획득한다. 첫 획득 시 자동으로 레벨 1로 해금한다.
        /// </summary>
        /// <param name="id">장비 고유 식별자</param>
        /// <param name="count">획득 수량</param>
        void AcquireEquipment(int id, int count);

        /// <summary>
        /// 장비를 강화한다. 같은 장비 소재를 소모한다.
        /// </summary>
        /// <param name="id">강화할 장비 ID</param>
        /// <returns>강화 성공 여부</returns>
        bool TryUpgrade(int id);

        /// <summary>
        /// 지정 타입의 강화 가능한 모든 장비를 강화한다.
        /// </summary>
        /// <param name="type">장비 타입</param>
        /// <returns>강화된 횟수</returns>
        int UpgradeAll(EquipmentType type);

        /// <summary>
        /// 강화 가능 여부를 반환한다.
        /// </summary>
        /// <param name="id">장비 ID</param>
        /// <returns>강화 가능하면 true</returns>
        bool CanUpgrade(int id);

        /// <summary>
        /// 현재 레벨에서 강화에 필요한 소재 개수를 반환한다.
        /// </summary>
        /// <param name="id">장비 ID</param>
        /// <returns>필요 소재 개수</returns>
        int GetUpgradeCost(int id);

        /// <summary>
        /// 장비를 장착한다.
        /// </summary>
        /// <param name="id">장착할 장비 ID</param>
        /// <returns>장착 성공 여부</returns>
        bool TryEquip(int id);

        /// <summary>
        /// 장비를 해제한다.
        /// </summary>
        /// <param name="type">슬롯 타입</param>
        void Unequip(EquipmentType type);

        /// <summary>
        /// 최적 장비를 자동으로 장착한다.
        /// </summary>
        /// <param name="type">슬롯 타입</param>
        void QuickEquip(EquipmentType type);

        /// <summary>전체 보유효과의 공격력 % 합계</summary>
        BigNumber TotalPossessionAttackPercent { get; }

        /// <summary>전체 보유효과의 체력 % 합계</summary>
        BigNumber TotalPossessionHpPercent { get; }

        /// <summary>장착된 무기의 장착효과 공격력 %</summary>
        BigNumber EquippedAttackPercent { get; }

        /// <summary>장착된 방어구의 장착효과 체력 %</summary>
        BigNumber EquippedHpPercent { get; }

        /// <summary>
        /// 지정 장비의 현재 레벨 기준 보유효과를 반환한다.
        /// </summary>
        /// <param name="id">장비 ID</param>
        /// <returns>보유효과 수치 (%)</returns>
        BigNumber GetPossessionEffect(int id);

        /// <summary>
        /// 지정 장비의 현재 레벨 기준 장착효과를 반환한다.
        /// </summary>
        /// <param name="id">장비 ID</param>
        /// <returns>장착효과 수치 (%)</returns>
        BigNumber GetEquipEffect(int id);

        /// <summary>
        /// 보유효과와 장착효과를 재계산하여 HeroModel 스탯에 반영한다.
        /// </summary>
        void RecalculateAndApplyEffects();
    }
}
