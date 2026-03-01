using IdleRPG.Equipment;

namespace IdleRPG.UI
{
    /// <summary>
    /// 장비 팝업에 전달되는 데이터. 어떤 슬롯 타입의 장비를 표시할지 지정한다.
    /// </summary>
    public struct EquipmentPopupData
    {
        /// <summary>표시할 장비 슬롯 타입</summary>
        public EquipmentType SlotType;
    }
}
