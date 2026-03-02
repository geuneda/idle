using IdleRPG.Core;

namespace IdleRPG.Skill
{
    /// <summary>
    /// 3010 돌비: 모든 적에게 1000% 광역 데미지.
    /// </summary>
    public class RockRainExecutor : ISkillEffectExecutor
    {
        /// <inheritdoc/>
        public int SkillId => 3010;

        /// <inheritdoc/>
        public void Execute(SkillExecutionContext context, SkillEntry entry, double damagePercent)
        {
            BigNumber damage = SkillFormula.CalcSkillDamage(context.HeroAttack, damagePercent);
            SkillExecutionHelper.DamageAllAlive(context, damage);
            SkillExecutionHelper.PublishActivated(context, entry.Id, context.HeroPosition);
        }
    }
}
