using UnityEngine;

namespace IdleRPG.Hero
{
    /// <summary>
    /// 영웅의 시각적 표현을 담당하는 뷰 컴포넌트.
    /// 애니메이터를 통해 상태 전환과 공격 애니메이션을 제어한다.
    /// </summary>
    public class HeroView : MonoBehaviour
    {
        /// <summary>영웅 애니메이터</summary>
        [SerializeField] private Animator _animator;

        /// <summary>투사체 발사 위치</summary>
        [SerializeField] private Transform _projectileSpawnPoint;

        private static readonly int StateParam = Animator.StringToHash("State");

        private HeroState _currentState = HeroState.Idle;

        /// <summary>투사체가 생성될 위치의 Transform</summary>
        public Transform ProjectileSpawnPoint => _projectileSpawnPoint;

        /// <summary>영웅의 현재 상태</summary>
        public HeroState CurrentState => _currentState;

        /// <summary>
        /// 영웅의 상태를 변경하고 애니메이터에 반영한다.
        /// </summary>
        /// <param name="state">전환할 대상 상태</param>
        public void SetState(HeroState state)
        {
            if (_currentState == state) return;
            _currentState = state;
            if (_animator != null)
            {
                _animator.SetInteger(StateParam, (int)state);
            }
        }

        /// <summary>
        /// 공격 애니메이션 트리거를 발생시킨다.
        /// </summary>
        public void PlayAttack()
        {
            if (_animator != null)
            {
                _animator.SetTrigger("Attack");
            }
        }
    }
}
