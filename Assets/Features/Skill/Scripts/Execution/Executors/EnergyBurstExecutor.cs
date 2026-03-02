using IdleRPG.Core;

namespace IdleRPG.Skill
{
    /// <summary>
    /// 3021 에너지의 폭발: 모든 적에게 92000% AoE + 최대 HP의 50% 회복.
    /// </summary>
    public class EnergyBurstExecutor : ISkillEffectExecutor
    {
        /// <inheritdoc/>
        public int SkillId => 3021;

        /// <inheritdoc/>
        public void Execute(SkillExecutionContext context, SkillEntry entry, double damagePercent)
        {
            BigNumber damage = SkillFormula.CalcSkillDamage(context.HeroAttack, damagePercent);
            SkillExecutionHelper.DamageAllAlive(context, damage);

            float healPercent = entry.BuffValue > 0f ? entry.BuffValue : 50f;
            context.MessageBroker.Publish(new SkillHealPercentAppliedMessage
            {
                HealPercent = healPercent
            });

            SkillExecutionHelper.PublishActivated(context, entry.Id, context.HeroPosition);
        }
    }
}
