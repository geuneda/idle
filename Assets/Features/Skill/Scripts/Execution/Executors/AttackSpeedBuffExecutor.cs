namespace IdleRPG.Skill
{
    /// <summary>
    /// 3004 공격속도 버프: 10초간 공격속도 30% 증가. SkillBuffAppliedMessage 발행.
    /// </summary>
    public class AttackSpeedBuffExecutor : ISkillEffectExecutor
    {
        /// <inheritdoc/>
        public int SkillId => 3004;

        /// <inheritdoc/>
        public void Execute(SkillExecutionContext context, SkillEntry entry, double damagePercent)
        {
            context.MessageBroker.Publish(new SkillBuffAppliedMessage
            {
                BuffValue = entry.BuffValue,
                Duration = entry.Duration
            });

            SkillExecutionHelper.PublishActivated(context, entry.Id, context.HeroPosition);
        }
    }
}
