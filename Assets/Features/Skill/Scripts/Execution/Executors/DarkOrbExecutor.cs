using IdleRPG.Core;

namespace IdleRPG.Skill
{
    /// <summary>
    /// 3019 어두운구체: 모든 적에게 83000% AoE + 장착 슬롯 중 가장 긴 쿨타임 80% 감소.
    /// </summary>
    public class DarkOrbExecutor : ISkillEffectExecutor
    {
        /// <inheritdoc/>
        public int SkillId => 3019;

        /// <inheritdoc/>
        public void Execute(SkillExecutionContext context, SkillEntry entry, double damagePercent)
        {
            BigNumber damage = SkillFormula.CalcSkillDamage(context.HeroAttack, damagePercent);
            SkillExecutionHelper.DamageAllAlive(context, damage);

            float reductionPercent = entry.BuffValue > 0f ? entry.BuffValue : 80f;
            context.ReduceLongestCooldown?.Invoke(reductionPercent);

            SkillExecutionHelper.PublishActivated(context, entry.Id, context.HeroPosition);
        }
    }
}
