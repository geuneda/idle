namespace IdleRPG.Core
{
    /// <summary>
    /// 수집형 아이템(장비, 스킬, 펫)의 공통 등급. 숫자가 클수록 높은 등급이다.
    /// </summary>
    public enum ItemGrade
    {
        /// <summary>일반</summary>
        Normal = 0,

        /// <summary>고급</summary>
        Advanced = 1,

        /// <summary>희귀</summary>
        Rare = 2,

        /// <summary>영웅</summary>
        Epic = 3,

        /// <summary>전설</summary>
        Legend = 4,

        /// <summary>신화</summary>
        Myth = 5,

        /// <summary>특별</summary>
        Special = 6
    }
}
