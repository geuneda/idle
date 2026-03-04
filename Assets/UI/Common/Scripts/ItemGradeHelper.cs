using IdleRPG.Core;
using UnityEngine;

namespace IdleRPG.UI
{
    /// <summary>
    /// 아이템 등급에 따른 색상 및 텍스트를 제공하는 유틸리티.
    /// 장비, 스킬, 펫이 공유하는 공통 등급 색상을 관리한다.
    /// </summary>
    public static class ItemGradeHelper
    {
        private static readonly Color NormalBorderColor = new(0.76f, 0.76f, 0.76f);
        private static readonly Color AdvancedBorderColor = new(0.30f, 0.75f, 0.30f);
        private static readonly Color RareBorderColor = new(0.25f, 0.55f, 0.95f);
        private static readonly Color EpicBorderColor = new(0.65f, 0.30f, 0.90f);
        private static readonly Color LegendBorderColor = new(0.95f, 0.70f, 0.15f);
        private static readonly Color MythBorderColor = new(0.95f, 0.25f, 0.25f);
        private static readonly Color SpecialBorderColor = new(0.95f, 0.85f, 0.35f);

        private static readonly Color NormalBgColor = new(0.60f, 0.60f, 0.60f, 0.8f);
        private static readonly Color AdvancedBgColor = new(0.20f, 0.55f, 0.20f, 0.8f);
        private static readonly Color RareBgColor = new(0.15f, 0.40f, 0.75f, 0.8f);
        private static readonly Color EpicBgColor = new(0.50f, 0.20f, 0.70f, 0.8f);
        private static readonly Color LegendBgColor = new(0.75f, 0.55f, 0.10f, 0.8f);
        private static readonly Color MythBgColor = new(0.75f, 0.15f, 0.15f, 0.8f);
        private static readonly Color SpecialBgColor = new(0.75f, 0.65f, 0.20f, 0.8f);

        /// <summary>
        /// 등급에 대응하는 테두리 색상을 반환한다.
        /// </summary>
        /// <param name="grade">아이템 등급</param>
        /// <returns>등급에 해당하는 테두리 색상</returns>
        public static Color GetBorderColor(ItemGrade grade)
        {
            return grade switch
            {
                ItemGrade.Normal => NormalBorderColor,
                ItemGrade.Advanced => AdvancedBorderColor,
                ItemGrade.Rare => RareBorderColor,
                ItemGrade.Epic => EpicBorderColor,
                ItemGrade.Legend => LegendBorderColor,
                ItemGrade.Myth => MythBorderColor,
                ItemGrade.Special => SpecialBorderColor,
                _ => NormalBorderColor
            };
        }

        /// <summary>
        /// 등급에 대응하는 배경 색상을 반환한다.
        /// </summary>
        /// <param name="grade">아이템 등급</param>
        /// <returns>등급에 해당하는 배경 색상</returns>
        public static Color GetBackgroundColor(ItemGrade grade)
        {
            return grade switch
            {
                ItemGrade.Normal => NormalBgColor,
                ItemGrade.Advanced => AdvancedBgColor,
                ItemGrade.Rare => RareBgColor,
                ItemGrade.Epic => EpicBgColor,
                ItemGrade.Legend => LegendBgColor,
                ItemGrade.Myth => MythBgColor,
                ItemGrade.Special => SpecialBgColor,
                _ => NormalBgColor
            };
        }

        /// <summary>
        /// 등급에 대응하는 한국어 등급 텍스트를 반환한다.
        /// </summary>
        /// <param name="grade">아이템 등급</param>
        /// <returns>등급 표시 문자열</returns>
        public static string GetGradeText(ItemGrade grade)
        {
            return grade switch
            {
                ItemGrade.Normal => "일반",
                ItemGrade.Advanced => "고급",
                ItemGrade.Rare => "희귀",
                ItemGrade.Epic => "영웅",
                ItemGrade.Legend => "전설",
                ItemGrade.Myth => "신화",
                ItemGrade.Special => "특별",
                _ => grade.ToString()
            };
        }
    }
}
