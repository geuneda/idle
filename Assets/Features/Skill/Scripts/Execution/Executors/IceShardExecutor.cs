using System.Collections.Generic;
using IdleRPG.Core;

namespace IdleRPG.Skill
{
    /// <summary>
    /// 3005 얼음조각: 거리순으로 가장 가까운 적부터 5회 타격, 각 230%.
    /// </summary>
    public class IceShardExecutor : ISkillEffectExecutor
    {
        private readonly List<ISkillTarget> _sortedTargets = new();

        /// <inheritdoc/>
        public int SkillId => 3005;

        /// <inheritdoc/>
        public void Execute(SkillExecutionContext context, SkillEntry entry, double damagePercent)
        {
            BigNumber damage = SkillFormula.CalcSkillDamage(context.HeroAttack, damagePercent);
            SkillExecutionHelper.BuildSortedAliveTargets(context, _sortedTargets);

            int hits = entry.HitCount > 0 ? entry.HitCount : 5;
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
