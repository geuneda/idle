using System;
using IdleRPG.Core;

namespace IdleRPG.Pet
{
    /// <summary>
    /// 펫 관련 순수 계산 함수 모음. 상태를 갖지 않으며 결정론적 결과를 보장한다.
    /// </summary>
    public static class PetFormula
    {
        /// <summary>
        /// 보유효과 공격력 퍼센트를 계산한다.
        /// </summary>
        public static BigNumber CalcPossessionEffect(PetEntry entry, int level)
        {
            if (level <= 0) return BigNumber.Zero;
            double value = entry.BasePossessionEffect + entry.PossessionEffectPerLevel * (level - 1);
            return new BigNumber(value, 0);
        }

        /// <summary>
        /// 현재 레벨의 데미지 퍼센트를 계산한다.
        /// </summary>
        public static double CalcDamagePercent(PetEntry entry, int level)
        {
            if (level <= 0) return entry.BaseDamagePercent;
            return entry.BaseDamagePercent + entry.DamagePercentPerLevel * (level - 1);
        }

        /// <summary>
        /// 영웅 공격력과 데미지 퍼센트로 실제 펫 공격력을 계산한다.
        /// </summary>
        public static BigNumber CalcPetDamage(BigNumber heroAttack, double damagePercent)
        {
            return heroAttack * damagePercent / 100.0;
        }

        /// <summary>
        /// 유효 DPS를 계산한다. 빠른 장착 시 정렬 기준으로 사용한다.
        /// </summary>
        public static double CalcEffectiveDps(PetEntry entry, int level)
        {
            double damagePercent = CalcDamagePercent(entry, level);
            return damagePercent * entry.AttackSpeed;
        }

        /// <summary>
        /// 강화에 필요한 소재 수를 반환한다.
        /// </summary>
        public static int GetRequiredCount(PetUpgradeConfig config, int currentLevel)
        {
            if (currentLevel <= 0) return 0;
            int index = currentLevel - 1;

            if (index < config.RequiredCountPerLevel.Count)
                return config.RequiredCountPerLevel[index];

            int overflow = index - config.RequiredCountPerLevel.Count;
            double value = config.OverflowBase * Math.Pow(1.0 + config.OverflowGrowthRate, overflow + 1);
            return (int)Math.Ceiling(value);
        }

        /// <summary>
        /// 최대 레벨에 도달했는지 확인한다.
        /// </summary>
        public static bool IsMaxLevel(PetEntry entry, int currentLevel)
        {
            if (entry.MaxLevel <= 0) return false;
            return currentLevel >= entry.MaxLevel;
        }

        /// <summary>
        /// 스테이지 진행도에 따라 슬롯이 해금되었는지 확인한다.
        /// </summary>
        public static bool IsSlotUnlocked(PetSlotConfig slotConfig, int currentChapter, int currentStage)
        {
            if (slotConfig == null) return false;
            if (currentChapter > slotConfig.UnlockChapter) return true;
            return currentChapter == slotConfig.UnlockChapter && currentStage >= slotConfig.UnlockStage;
        }
    }
}
