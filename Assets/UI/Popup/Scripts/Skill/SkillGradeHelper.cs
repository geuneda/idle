using IdleRPG.Skill;
using UnityEngine;

namespace IdleRPG.UI
{
    /// <summary>
    /// 스킬 등급에 따른 색상 및 텍스트를 제공하는 유틸리티.
    /// </summary>
    public static class SkillGradeHelper
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
        /// 스킬 등급에 대응하는 테두리 색상을 반환한다.
        /// </summary>
        /// <param name="grade">스킬 등급</param>
        /// <returns>등급에 해당하는 테두리 색상</returns>
        public static Color GetBorderColor(SkillGrade grade)
        {
            return grade switch
            {
                SkillGrade.Normal => NormalBorderColor,
                SkillGrade.Advanced => AdvancedBorderColor,
                SkillGrade.Rare => RareBorderColor,
                SkillGrade.Epic => EpicBorderColor,
                SkillGrade.Legend => LegendBorderColor,
                SkillGrade.Myth => MythBorderColor,
                SkillGrade.Special => SpecialBorderColor,
                _ => NormalBorderColor
            };
        }

        /// <summary>
        /// 스킬 등급에 대응하는 배경 색상을 반환한다.
        /// </summary>
        /// <param name="grade">스킬 등급</param>
        /// <returns>등급에 해당하는 배경 색상</returns>
        public static Color GetBackgroundColor(SkillGrade grade)
        {
            return grade switch
            {
                SkillGrade.Normal => NormalBgColor,
                SkillGrade.Advanced => AdvancedBgColor,
                SkillGrade.Rare => RareBgColor,
                SkillGrade.Epic => EpicBgColor,
                SkillGrade.Legend => LegendBgColor,
                SkillGrade.Myth => MythBgColor,
                SkillGrade.Special => SpecialBgColor,
                _ => NormalBgColor
            };
        }

        /// <summary>
        /// 스킬 등급에 대응하는 한국어 등급 텍스트를 반환한다.
        /// </summary>
        /// <param name="grade">스킬 등급</param>
        /// <returns>등급 표시 문자열</returns>
        public static string GetGradeText(SkillGrade grade)
        {
            return grade switch
            {
                SkillGrade.Normal => "일반",
                SkillGrade.Advanced => "고급",
                SkillGrade.Rare => "희귀",
                SkillGrade.Epic => "영웅",
                SkillGrade.Legend => "전설",
                SkillGrade.Myth => "신화",
                SkillGrade.Special => "특별",
                _ => grade.ToString()
            };
        }
    }
}
