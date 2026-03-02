using IdleRPG.Core;

namespace IdleRPG.Skill
{
    /// <summary>
    /// 3001 요정소환: 단일 대상에게 초당 2.3회 공격, 5초 지속.
    /// </summary>
    public class FairySummonExecutor : ISkillEffectExecutor
    {
        /// <inheritdoc/>
        public int SkillId => 3001;

        /// <inheritdoc/>
        public void Execute(SkillExecutionContext context, SkillEntry entry, double damagePercent)
        {
            float attacksPerSecond = entry.BuffValue > 0f ? entry.BuffValue : 2.3f;
            var effect = new SummonSustainedEffect(
                context.HeroAttack, damagePercent, attacksPerSecond, entry.Duration, isAoE: false);
            context.RegisterSustainedEffect?.Invoke(effect);

            SkillExecutionHelper.PublishActivated(context, entry.Id, context.HeroPosition);
        }
    }
}
