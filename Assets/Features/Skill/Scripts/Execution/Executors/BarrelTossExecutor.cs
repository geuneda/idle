using IdleRPG.Core;

namespace IdleRPG.Skill
{
    /// <summary>
    /// 3012 통 던지기: 모든 적에게 5000% 광역 데미지.
    /// </summary>
    public class BarrelTossExecutor : ISkillEffectExecutor
    {
        /// <inheritdoc/>
        public int SkillId => 3012;

        /// <inheritdoc/>
        public void Execute(SkillExecutionContext context, SkillEntry entry, double damagePercent)
        {
            BigNumber damage = SkillFormula.CalcSkillDamage(context.HeroAttack, damagePercent);
            SkillExecutionHelper.DamageAllAlive(context, damage);
            SkillExecutionHelper.PublishActivated(context, entry.Id, context.HeroPosition);
        }
    }
}
