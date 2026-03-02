using IdleRPG.Core;

namespace IdleRPG.Skill
{
    /// <summary>
    /// 3015 큰검: 모든 적에게 8500% 광역 데미지.
    /// </summary>
    public class GreatSwordExecutor : ISkillEffectExecutor
    {
        /// <inheritdoc/>
        public int SkillId => 3015;

        /// <inheritdoc/>
        public void Execute(SkillExecutionContext context, SkillEntry entry, double damagePercent)
        {
            BigNumber damage = SkillFormula.CalcSkillDamage(context.HeroAttack, damagePercent);
            SkillExecutionHelper.DamageAllAlive(context, damage);
            SkillExecutionHelper.PublishActivated(context, entry.Id, context.HeroPosition);
        }
    }
}
