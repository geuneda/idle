using IdleRPG.Core;

namespace IdleRPG.Skill
{
    /// <summary>
    /// 스킬 관련 순수 계산 함수 모음. 상태를 갖지 않으며 결정론적 결과를 보장한다.
    /// </summary>
    public static class SkillFormula
    {
        private const string GoldColorTag = "<color=#FFD700>";
        private const string ColorEndTag = "</color>";

        /// <summary>
        /// 보유효과 공격력 퍼센트를 계산한다.
        /// </summary>
        public static BigNumber CalcPossessionEffect(SkillEntry entry, int level)
        {
            return CollectibleFormula.CalcPossessionEffect(entry, level);
        }

        /// <summary>
        /// 현재 레벨의 데미지 퍼센트를 계산한다.
        /// </summary>
        public static double CalcDamagePercent(SkillEntry entry, int level)
        {
            if (level <= 0) return entry.BaseDamagePercent;
            return entry.BaseDamagePercent + entry.DamagePercentPerLevel * (level - 1);
        }

        /// <summary>
        /// 영웅 공격력과 데미지 퍼센트로 실제 스킬 데미지를 계산한다.
        /// </summary>
        public static BigNumber CalcSkillDamage(BigNumber heroAttack, double damagePercent)
        {
            return heroAttack * damagePercent / 100.0;
        }

        /// <summary>
        /// 강화에 필요한 소재 수를 반환한다.
        /// </summary>
        public static int GetRequiredCount(UpgradeConfig config, int currentLevel)
        {
            return CollectibleFormula.GetRequiredCount(config, currentLevel);
        }

        /// <summary>
        /// 최대 레벨에 도달했는지 확인한다.
        /// </summary>
        public static bool IsMaxLevel(SkillEntry entry, int currentLevel)
        {
            return CollectibleFormula.IsMaxLevel(entry, currentLevel);
        }

        /// <summary>
        /// 스테이지 진행도에 따라 슬롯이 해금되었는지 확인한다.
        /// </summary>
        public static bool IsSlotUnlocked(SkillSlotConfig slotConfig, int currentChapter, int currentStage)
        {
            if (slotConfig == null) return false;
            if (currentChapter > slotConfig.UnlockChapter) return true;
            return currentChapter == slotConfig.UnlockChapter && currentStage >= slotConfig.UnlockStage;
        }

        /// <summary>
        /// 스킬 설명 템플릿을 실제 값으로 치환한다. 수치는 TMP 금색 리치텍스트로 감싼다.
        /// </summary>
        public static string FormatDescription(SkillEntry entry, int level)
        {
            if (string.IsNullOrEmpty(entry.DescriptionTemplate))
                return string.Empty;

            double damagePercent = CalcDamagePercent(entry, level);
            string result = entry.DescriptionTemplate;

            result = result.Replace("{DamagePercent}", WrapGold(damagePercent.ToString("0.#")));
            result = result.Replace("{HitCount}", WrapGold(entry.HitCount.ToString()));
            result = result.Replace("{Duration}", WrapGold(entry.Duration.ToString("0.#")));
            result = result.Replace("{BuffValue}", WrapGold(entry.BuffValue.ToString("0.#")));

            return result;
        }

        private static string WrapGold(string text)
        {
            return $"{GoldColorTag}{text}{ColorEndTag}";
        }
    }
}
