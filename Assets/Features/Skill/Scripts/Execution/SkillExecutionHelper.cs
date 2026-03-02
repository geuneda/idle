using System.Collections.Generic;
using IdleRPG.Core;
using UnityEngine;

namespace IdleRPG.Skill
{
    /// <summary>
    /// 스킬 실행기 공통 유틸리티. 코드 중복을 방지하기 위한 정적 헬퍼 메서드 모음.
    /// </summary>
    public static class SkillExecutionHelper
    {
        /// <summary>
        /// 영웅 위치에서 가장 가까운 살아있는 대상을 찾는다.
        /// </summary>
        /// <param name="context">스킬 실행 컨텍스트</param>
        /// <returns>가장 가까운 살아있는 대상. 없으면 null</returns>
        public static ISkillTarget FindNearestAlive(SkillExecutionContext context)
        {
            ISkillTarget nearest = null;
            float nearestDist = float.MaxValue;

            for (int i = 0; i < context.Targets.Count; i++)
            {
                var t = context.Targets[i];
                if (!t.IsAlive) continue;

                float dist = Vector3.Distance(context.HeroPosition, t.Position);
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = t;
                }
            }

            return nearest;
        }

        /// <summary>
        /// 모든 살아있는 적에게 데미지를 적용한다.
        /// </summary>
        /// <param name="context">스킬 실행 컨텍스트</param>
        /// <param name="damage">적용할 데미지</param>
        public static void DamageAllAlive(SkillExecutionContext context, BigNumber damage)
        {
            for (int i = 0; i < context.Targets.Count; i++)
            {
                var target = context.Targets[i];
                if (!target.IsAlive) continue;
                target.TakeDamage(damage);
            }
        }

        /// <summary>
        /// 살아있는 적을 영웅과의 거리 순으로 정렬하여 반환한다.
        /// </summary>
        /// <param name="context">스킬 실행 컨텍스트</param>
        /// <param name="outList">결과를 담을 리스트 (clear 후 채움)</param>
        public static void BuildSortedAliveTargets(SkillExecutionContext context, List<ISkillTarget> outList)
        {
            outList.Clear();
            for (int i = 0; i < context.Targets.Count; i++)
            {
                if (context.Targets[i].IsAlive)
                    outList.Add(context.Targets[i]);
            }

            Vector3 heroPos = context.HeroPosition;
            outList.Sort((a, b) =>
            {
                float distA = Vector3.Distance(heroPos, a.Position);
                float distB = Vector3.Distance(heroPos, b.Position);
                return distA.CompareTo(distB);
            });
        }

        /// <summary>
        /// SkillActivatedMessage를 발행한다.
        /// </summary>
        /// <param name="context">스킬 실행 컨텍스트</param>
        /// <param name="skillId">발동된 스킬 ID</param>
        /// <param name="position">발동 위치</param>
        public static void PublishActivated(SkillExecutionContext context, int skillId, Vector3 position)
        {
            context.MessageBroker.Publish(new SkillActivatedMessage
            {
                SkillId = skillId,
                Position = position
            });
        }
    }
}
