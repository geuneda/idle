using IdleRPG.Core;
using IdleRPG.Hero;

namespace IdleRPG.Growth
{
    /// <summary>
    /// 영웅의 현재 스탯으로 전투력을 계산하는 순수 함수 모음.
    /// 전투력은 DPS에 치명타, 다중 사격, 적 추가 피해 보정을 곱한 뒤 체력 보정을 합산하여 산출한다.
    /// </summary>
    /// <remarks>
    /// <para>공식: <c>Power = (DPS * CritFactor * MultiShotFactor * EnemyBonusFactor) + MaxHp * HpWeight</c></para>
    /// <para>DPS = Attack * AttackSpeed</para>
    /// <para>CritFactor = 1 + CritRate * (CritDamage - 1)</para>
    /// <para>MultiShotFactor = 1 + DoubleShotRate * 2.65 + TripleShotRate * 3.5 + AdvancedAttackBonus</para>
    /// <para>EnemyBonusFactor = 1 + EnemyBonusDamage</para>
    /// </remarks>
    public static class CombatPowerCalculator
    {
        /// <summary>체력이 전투력에 기여하는 가중치</summary>
        public const double HpWeight = 35.0;

        /// <summary>이중 사격 확률이 전투력에 기여하는 배수</summary>
        public const double DoubleShotPowerFactor = 2.65;

        /// <summary>삼중 사격 확률이 전투력에 기여하는 배수</summary>
        public const double TripleShotPowerFactor = 3.5;

        /// <summary>
        /// 영웅 모델의 현재 스탯으로 전투력을 계산한다.
        /// </summary>
        /// <param name="hero">영웅 상태 모델</param>
        /// <returns>계산된 전투력</returns>
        public static BigNumber Calculate(HeroModel hero)
        {
            BigNumber attack = hero.Attack.Value;
            float attackSpeed = hero.AttackSpeed.Value;
            float critRate = hero.CritRate.Value;
            float critDamage = hero.CritDamage.Value;
            float doubleShotRate = hero.DoubleShotRate.Value;
            float tripleShotRate = hero.TripleShotRate.Value;
            float advancedAttackBonus = hero.AdvancedAttackBonus.Value;
            float enemyBonusDamage = hero.EnemyBonusDamage.Value;

            BigNumber dps = attack * (double)attackSpeed;

            double critFactor = 1.0 + critRate * (critDamage - 1.0);
            dps = dps * critFactor;

            double multiShotFactor = 1.0
                                     + doubleShotRate * DoubleShotPowerFactor
                                     + tripleShotRate * TripleShotPowerFactor
                                     + advancedAttackBonus;

            BigNumber power = dps * multiShotFactor;

            double enemyBonusFactor = 1.0 + enemyBonusDamage;
            power = power * enemyBonusFactor;

            BigNumber hpContribution = hero.MaxHp.Value * HpWeight;
            power = power + hpContribution;

            return power;
        }
    }
}
