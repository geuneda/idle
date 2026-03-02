namespace IdleRPG.Skill
{
    /// <summary>
    /// 3017 실체소환: 매초 모든 적에게 55000% AoE 데미지, 6초 지속 DoT.
    /// </summary>
    public class EntitySummonExecutor : ISkillEffectExecutor
    {
        /// <inheritdoc/>
        public int SkillId => 3017;

        /// <inheritdoc/>
        public void Execute(SkillExecutionContext context, SkillEntry entry, double damagePercent)
        {
            var effect = new DoTSustainedEffect(context.HeroAttack, damagePercent, entry.Duration);
            context.RegisterSustainedEffect?.Invoke(effect);

            SkillExecutionHelper.PublishActivated(context, entry.Id, context.HeroPosition);
        }
    }
}
