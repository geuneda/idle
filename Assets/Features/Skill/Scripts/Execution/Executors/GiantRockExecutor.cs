using IdleRPG.Core;

namespace IdleRPG.Skill
{
    /// <summary>
    /// 3003 거대한 돌: 모든 적에게 780% 광역 데미지.
    /// </summary>
    public class GiantRockExecutor : ISkillEffectExecutor
    {
        /// <inheritdoc/>
        public int SkillId => 3003;

        /// <inheritdoc/>
        public void Execute(SkillExecutionContext context, SkillEntry entry, double damagePercent)
        {
            BigNumber damage = SkillFormula.CalcSkillDamage(context.HeroAttack, damagePercent);
            SkillExecutionHelper.DamageAllAlive(context, damage);
            SkillExecutionHelper.PublishActivated(context, entry.Id, context.HeroPosition);
        }
    }
}
