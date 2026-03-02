using IdleRPG.Core;

namespace IdleRPG.Skill
{
    /// <summary>
    /// 3009 동물무리: 모든 적에게 1050% 광역 데미지.
    /// </summary>
    public class AnimalPackExecutor : ISkillEffectExecutor
    {
        /// <inheritdoc/>
        public int SkillId => 3009;

        /// <inheritdoc/>
        public void Execute(SkillExecutionContext context, SkillEntry entry, double damagePercent)
        {
            BigNumber damage = SkillFormula.CalcSkillDamage(context.HeroAttack, damagePercent);
            SkillExecutionHelper.DamageAllAlive(context, damage);
            SkillExecutionHelper.PublishActivated(context, entry.Id, context.HeroPosition);
        }
    }
}
