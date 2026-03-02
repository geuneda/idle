using IdleRPG.Core;

namespace IdleRPG.Skill
{
    /// <summary>
    /// 소환형 지속 효과. 지정 시간 동안 초당 N회 공격을 반복한다.
    /// </summary>
    public class SummonSustainedEffect : ISustainedEffect
    {
        private readonly double _damagePercent;
        private readonly float _attackInterval;
        private readonly bool _isAoE;
        private readonly BigNumber _heroAttack;
        private float _remainingDuration;
        private float _attackTimer;

        /// <inheritdoc/>
        public bool IsExpired => _remainingDuration <= 0f;

        /// <summary>
        /// <see cref="SummonSustainedEffect"/>의 새 인스턴스를 생성한다.
        /// </summary>
        /// <param name="heroAttack">소환 시점의 영웅 공격력 (스냅샷)</param>
        /// <param name="damagePercent">공격당 데미지 퍼센트</param>
        /// <param name="attacksPerSecond">초당 공격 횟수</param>
        /// <param name="duration">지속시간 (초)</param>
        /// <param name="isAoE">true면 전체 공격, false면 가장 가까운 단일 대상</param>
        public SummonSustainedEffect(
            BigNumber heroAttack,
            double damagePercent,
            float attacksPerSecond,
            float duration,
            bool isAoE)
        {
            _heroAttack = heroAttack;
            _damagePercent = damagePercent;
            _attackInterval = attacksPerSecond > 0f ? 1f / attacksPerSecond : 1f;
            _isAoE = isAoE;
            _remainingDuration = duration;
            _attackTimer = 0f;
        }

        /// <inheritdoc/>
        public void Tick(float dt, SkillExecutionContext context)
        {
            if (IsExpired) return;

            _remainingDuration -= dt;
            _attackTimer += dt;

            while (_attackTimer >= _attackInterval && !IsExpired)
            {
                _attackTimer -= _attackInterval;
                PerformAttack(context);
            }
        }

        private void PerformAttack(SkillExecutionContext context)
        {
            BigNumber damage = SkillFormula.CalcSkillDamage(_heroAttack, _damagePercent);

            if (_isAoE)
            {
                SkillExecutionHelper.DamageAllAlive(context, damage);
            }
            else
            {
                var target = SkillExecutionHelper.FindNearestAlive(context);
                target?.TakeDamage(damage);
            }
        }
    }
}
