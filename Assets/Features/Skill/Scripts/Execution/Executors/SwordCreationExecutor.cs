using System.Collections.Generic;
using IdleRPG.Core;

namespace IdleRPG.Skill
{
    /// <summary>
    /// 3002 검생성: 거리순으로 가장 가까운 적부터 9회 타격, 각 95%.
    /// </summary>
    public class SwordCreationExecutor : ISkillEffectExecutor
    {
        private readonly List<ISkillTarget> _sortedTargets = new();

        /// <inheritdoc/>
        public int SkillId => 3002;

        /// <inheritdoc/>
        public void Execute(SkillExecutionContext context, SkillEntry entry, double damagePercent)
        {
            BigNumber damage = SkillFormula.CalcSkillDamage(context.HeroAttack, damagePercent);
            SkillExecutionHelper.BuildSortedAliveTargets(context, _sortedTargets);

            int hits = entry.HitCount > 0 ? entry.HitCount : 9;
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
