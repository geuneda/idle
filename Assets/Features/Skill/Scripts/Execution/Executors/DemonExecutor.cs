namespace IdleRPG.Skill
{
    /// <summary>
    /// 3020 악마: AoE 소환, 초당 1회 공격, 10초 지속.
    /// </summary>
    public class DemonExecutor : ISkillEffectExecutor
    {
        /// <inheritdoc/>
        public int SkillId => 3020;

        /// <inheritdoc/>
        public void Execute(SkillExecutionContext context, SkillEntry entry, double damagePercent)
        {
            float attacksPerSecond = entry.BuffValue > 0f ? entry.BuffValue : 1f;
            var effect = new SummonSustainedEffect(
                context.HeroAttack, damagePercent, attacksPerSecond, entry.Duration, isAoE: true);
            context.RegisterSustainedEffect?.Invoke(effect);

            SkillExecutionHelper.PublishActivated(context, entry.Id, context.HeroPosition);
        }
    }
}
