using System.Collections.Generic;
using IdleRPG.Core;

namespace IdleRPG.Skill
{
    /// <summary>
    /// 3006 번개: 거리순으로 가장 가까운 적부터 4회 타격, 각 400%.
    /// </summary>
    public class LightningExecutor : ISkillEffectExecutor
    {
        private readonly List<ISkillTarget> _sortedTargets = new();

        /// <inheritdoc/>
        public int SkillId => 3006;

        /// <inheritdoc/>
        public void Execute(SkillExecutionContext context, SkillEntry entry, double damagePercent)
        {
            BigNumber damage = SkillFormula.CalcSkillDamage(context.HeroAttack, damagePercent);
            SkillExecutionHelper.BuildSortedAliveTargets(context, _sortedTargets);

            int hits = entry.HitCount > 0 ? entry.HitCount : 4;
            for (int i = 0; i < hits && _sortedTargets.Count > 0; i++)
            {
                var target = _sortedTargets[i % _sortedTargets.Count];
                if (target.IsAlive)
                    target.TakeDamage(damage);
            }

            SkillExecutionHelper.PublishActivated(context, entry.Id, context.HeroPosition);
        }
    }
}
