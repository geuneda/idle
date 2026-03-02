using IdleRPG.Core;

namespace IdleRPG.Skill
{
    /// <summary>
    /// 3016 마법서클: 6개 구슬이 각각 모든 적에게 6600% AoE 데미지.
    /// </summary>
    public class MagicCircleExecutor : ISkillEffectExecutor
    {
        /// <inheritdoc/>
        public int SkillId => 3016;

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
