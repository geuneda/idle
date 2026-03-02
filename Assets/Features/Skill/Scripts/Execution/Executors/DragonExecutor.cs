namespace IdleRPG.Skill
{
    /// <summary>
    /// 3014 드래곤: AoE 소환, 초당 3.5회 공격, 5초 지속.
    /// </summary>
    public class DragonExecutor : ISkillEffectExecutor
    {
        /// <inheritdoc/>
        public int SkillId => 3014;

        /// <inheritdoc/>
        public void Execute(SkillExecutionContext context, SkillEntry entry, double damagePercent)
        {
            float attacksPerSecond = entry.BuffValue > 0f ? entry.BuffValue : 3.5f;
            var effect = new SummonSustainedEffect(
                context.HeroAttack, damagePercent, attacksPerSecond, entry.Duration, isAoE: true);
            context.RegisterSustainedEffect?.Invoke(effect);

            SkillExecutionHelper.PublishActivated(context, entry.Id, context.HeroPosition);
        }
    }
}
