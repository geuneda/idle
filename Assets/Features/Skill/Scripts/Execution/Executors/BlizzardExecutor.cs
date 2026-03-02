namespace IdleRPG.Skill
{
    /// <summary>
    /// 3008 눈보라: 매초 모든 적에게 275% 데미지, 5초 지속 DoT.
    /// </summary>
    public class BlizzardExecutor : ISkillEffectExecutor
    {
        /// <inheritdoc/>
        public int SkillId => 3008;

        /// <inheritdoc/>
        public void Execute(SkillExecutionContext context, SkillEntry entry, double damagePercent)
        {
            var effect = new DoTSustainedEffect(context.HeroAttack, damagePercent, entry.Duration);
            context.RegisterSustainedEffect?.Invoke(effect);

            SkillExecutionHelper.PublishActivated(context, entry.Id, context.HeroPosition);
        }
    }
}
