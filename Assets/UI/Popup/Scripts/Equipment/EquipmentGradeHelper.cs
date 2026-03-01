using IdleRPG.Equipment;
using UnityEngine;

namespace IdleRPG.UI
{
    /// <summary>
    /// 장비 등급에 따른 색상을 제공하는 유틸리티.
    /// 추후 장비 등급에 맞는 백그라운드 스프라이트를 제공하도록 할것임.
    /// </summary>
    public static class EquipmentGradeHelper
    {
        private static readonly Color NormalColor = new(0.76f, 0.76f, 0.76f);
        private static readonly Color AdvancedColor = new(0.30f, 0.75f, 0.30f);
        private static readonly Color RareColor = new(0.25f, 0.55f, 0.95f);
        private static readonly Color EpicColor = new(0.65f, 0.30f, 0.90f);
        private static readonly Color LegendColor = new(0.95f, 0.70f, 0.15f);
        private static readonly Color MythColor = new(0.95f, 0.25f, 0.25f);
        private static readonly Color SpecialColor = new(0.95f, 0.85f, 0.35f);

        /// <summary>
        /// 장비 등급에 대응하는 배경 색상을 반환한다.
        /// </summary>
        /// <param name="grade">장비 등급</param>
        /// <returns>등급에 해당하는 색상</returns>
        public static Color GetColor(EquipmentGrade grade)
        {
            return grade switch
            {
                EquipmentGrade.Normal => NormalColor,
                EquipmentGrade.Advanced => AdvancedColor,
                EquipmentGrade.Rare => RareColor,
                EquipmentGrade.Epic => EpicColor,
                EquipmentGrade.Legend => LegendColor,
                EquipmentGrade.Myth => MythColor,
                EquipmentGrade.Special => SpecialColor,
                _ => NormalColor
            };
        }
    }
}
