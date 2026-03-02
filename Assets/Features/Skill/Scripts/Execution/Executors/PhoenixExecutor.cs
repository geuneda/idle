using IdleRPG.Core;

namespace IdleRPG.Skill
{
    /// <summary>
    /// 3007 불사조: 가장 가까운 적 1체에 1460% 단일 대상 데미지.
    /// </summary>
    public class PhoenixExecutor : ISkillEffectExecutor
    {
        /// <inheritdoc/>
        public int SkillId => 3007;

        /// <inheritdoc/>
        public void Execute(SkillExecutionContext context, SkillEntry entry, double damagePercent)
        {
            var target = SkillExecutionHelper.FindNearestAlive(context);
            if (target == null) return;

            BigNumber damage = SkillFormula.CalcSkillDamage(context.HeroAttack, damagePercent);
            target.TakeDamage(damage);

            SkillExecutionHelper.PublishActivated(context, entry.Id, target.Position);
        }
    }
}
