namespace IdleRPG.Skill
{
    /// <summary>
    /// 3018 신소환: AoE 소환, 초당 1.5회 공격, 8초 지속.
    /// </summary>
    public class GodSummonExecutor : ISkillEffectExecutor
    {
        /// <inheritdoc/>
        public int SkillId => 3018;

        /// <inheritdoc/>
        public void Execute(SkillExecutionContext context, SkillEntry entry, double damagePercent)
        {
            float attacksPerSecond = entry.BuffValue > 0f ? entry.BuffValue : 1.5f;
            var effect = new SummonSustainedEffect(
                context.HeroAttack, damagePercent, attacksPerSecond, entry.Duration, isAoE: true);
            context.RegisterSustainedEffect?.Invoke(effect);

            SkillExecutionHelper.PublishActivated(context, entry.Id, context.HeroPosition);
        }
    }
}
