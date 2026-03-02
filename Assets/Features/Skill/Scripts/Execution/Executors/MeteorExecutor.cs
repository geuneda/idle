using IdleRPG.Core;

namespace IdleRPG.Skill
{
    /// <summary>
    /// 3011 운석: 모든 적에게 1250% 광역 데미지.
    /// </summary>
    public class MeteorExecutor : ISkillEffectExecutor
    {
        /// <inheritdoc/>
        public int SkillId => 3011;

        /// <inheritdoc/>
        public void Execute(SkillExecutionContext context, SkillEntry entry, double damagePercent)
        {
            BigNumber damage = SkillFormula.CalcSkillDamage(context.HeroAttack, damagePercent);
            SkillExecutionHelper.DamageAllAlive(context, damage);
            SkillExecutionHelper.PublishActivated(context, entry.Id, context.HeroPosition);
        }
    }
}
