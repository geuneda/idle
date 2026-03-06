using System;

namespace IdleRPG.Economy
{
    /// <summary>
    /// 재화별 표시 정보. Google Sheets에서 임포트된다.
    /// </summary>
    [Serializable]
    public class CurrencyDisplayConfig
    {
        /// <summary>재화 종류</summary>
        public CurrencyType Type = CurrencyType.Gold;

        /// <summary>한국어 표시 이름</summary>
        public string DisplayName = "";

        /// <summary>재화 설명 텍스트</summary>
        public string Description = "";

        /// <summary>Addressable 스프라이트 키</summary>
        public string IconSpriteKey = "";

        /// <summary>배경색 hex 코드</summary>
        public string BackgroundColorHex = "#FFFFFF";
    }
}
