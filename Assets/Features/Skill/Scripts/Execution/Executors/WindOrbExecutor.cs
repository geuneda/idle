using IdleRPG.Core;

namespace IdleRPG.Skill
{
    /// <summary>
    /// 3013 바람구체: 6번 폭발, 각 폭발마다 모든 적에게 2000% AoE 데미지.
    /// </summary>
    public class WindOrbExecutor : ISkillEffectExecutor
    {
        /// <inheritdoc/>
        public int SkillId => 3013;

        /// <inheritdoc/>
        public void Execute(SkillExecutionContext context, SkillEntry entry, double damagePercent)
        {
            BigNumber damage = SkillFormula.CalcSkillDamage(context.HeroAttack, damagePercent);
            int hits = entry.HitCount > 0 ? entry.HitCount : 6;

            for (int i = 0; i < hits; i++)
            {
                SkillExecutionHelper.DamageAllAlive(context, damage);
            }

            SkillExecutionHelper.PublishActivated(context, entry.Id, context.HeroPosition);
        }
    }
}
