using IdleRPG.Core;

namespace IdleRPG.Skill
{
    /// <summary>
    /// 지속 피해(DoT) 효과. 매초 모든 적에게 데미지를 적용한다.
    /// </summary>
    public class DoTSustainedEffect : ISustainedEffect
    {
        private readonly double _damagePercentPerTick;
        private readonly BigNumber _heroAttack;
        private float _remainingDuration;
        private float _tickTimer;

        /// <inheritdoc/>
        public bool IsExpired => _remainingDuration <= 0f;

        /// <summary>
        /// <see cref="DoTSustainedEffect"/>의 새 인스턴스를 생성한다.
        /// </summary>
        /// <param name="heroAttack">발동 시점의 영웅 공격력 (스냅샷)</param>
        /// <param name="damagePercentPerTick">틱당 데미지 퍼센트 (매초)</param>
        /// <param name="duration">지속시간 (초)</param>
        public DoTSustainedEffect(BigNumber heroAttack, double damagePercentPerTick, float duration)
        {
            _heroAttack = heroAttack;
            _damagePercentPerTick = damagePercentPerTick;
            _remainingDuration = duration;
            _tickTimer = 0f;
        }

        /// <inheritdoc/>
        public void Tick(float dt, SkillExecutionContext context)
        {
            if (IsExpired) return;

            _remainingDuration -= dt;
            _tickTimer += dt;

            while (_tickTimer >= 1f && !IsExpired)
            {
                _tickTimer -= 1f;
                BigNumber damage = SkillFormula.CalcSkillDamage(_heroAttack, _damagePercentPerTick);
                SkillExecutionHelper.DamageAllAlive(context, damage);
            }
        }
    }
}
