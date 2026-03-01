namespace IdleRPG.Equipment
{
    /// <summary>
    /// 장비 대분류. 슬롯 타입을 결정한다.
    /// </summary>
    public enum EquipmentType
    {
        /// <summary>무기 - 공격력 % 증가</summary>
        Weapon = 0,

        /// <summary>방어구 - 체력 % 증가</summary>
        Armor = 1,

        /// <summary>장신구 - 추후 구현</summary>
        Accessory = 2
    }
}
