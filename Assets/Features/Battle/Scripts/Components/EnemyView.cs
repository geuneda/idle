using Geuneda.Services;
using UnityEngine;

namespace IdleRPG.Battle
{
    /// <summary>
    /// 적의 시각적 표현과 이동/공격 로직을 담당하는 뷰 컴포넌트.
    /// <see cref="IPoolEntitySpawn{T}"/>을 구현하여 오브젝트 풀에서 재사용된다.
    /// </summary>
    public class EnemyView : MonoBehaviour, IPoolEntitySpawn<EnemyModel>, IPoolEntityDespawn
    {
        /// <summary>적 애니메이터</summary>
        [SerializeField] private Animator _animator;

        private static readonly int StateParam = Animator.StringToHash("State");

        private EnemyState _currentState = EnemyState.Move;
        private float _attackTimer;

        /// <summary>현재 적의 데이터 모델</summary>
        public EnemyModel Model { get; private set; }

        /// <summary>현재 적의 상태</summary>
        public EnemyState CurrentState => _currentState;

        /// <summary>웨이브 내 적 인덱스</summary>
        public int Index { get; set; }

        /// <summary>
        /// 풀에서 스폰될 때 호출된다. 모델 데이터를 설정하고 이동 상태로 초기화한다.
        /// </summary>
        /// <param name="data">적 데이터 모델</param>
        public void OnSpawn(EnemyModel data)
        {
            Model = data;
            _currentState = EnemyState.Move;
            _attackTimer = 0f;
            if (_animator != null)
            {
                _animator.SetInteger(StateParam, (int)EnemyState.Move);
            }
        }

        /// <summary>
        /// 풀로 반환될 때 호출된다. 모델 참조를 해제하고 상태를 초기화한다.
        /// </summary>
        public void OnDespawn()
        {
            Model = null;
            _currentState = EnemyState.Move;
            _attackTimer = 0f;
        }

        /// <summary>
        /// 적의 상태를 변경하고 애니메이터에 반영한다.
        /// </summary>
        /// <param name="state">전환할 대상 상태</param>
        public void SetState(EnemyState state)
        {
            if (_currentState == state) return;
            _currentState = state;
            if (_animator != null)
            {
                _animator.SetInteger(StateParam, (int)state);
            }
        }

        /// <summary>
        /// 적을 대상(영웅) 방향으로 이동시킨다. 공격 범위에 도달하면 공격 상태로 전환한다.
        /// </summary>
        /// <param name="dt">프레임 경과 시간</param>
        /// <param name="targetX">대상(영웅)의 X 좌표</param>
        /// <returns>공격 범위에 도달하여 공격 상태로 전환하면 true</returns>
        public bool UpdateMovement(float dt, float targetX)
        {
            if (_currentState != EnemyState.Move || Model == null) return false;

            float currentX = transform.position.x;
            float distanceToTarget = currentX - targetX;

            if (distanceToTarget <= Model.AttackRange)
            {
                SetState(EnemyState.Attack);
                return true;
            }

            float newX = currentX - Model.MoveSpeed * dt;
            transform.position = new Vector3(newX, transform.position.y, transform.position.z);
            return false;
        }

        /// <summary>
        /// 공격 타이머를 갱신하고 공격 간격에 도달하면 공격을 수행한다.
        /// </summary>
        /// <param name="dt">프레임 경과 시간</param>
        /// <returns>공격이 실행되면 true</returns>
        public bool TryAttack(float dt)
        {
            if (_currentState != EnemyState.Attack || Model == null) return false;

            _attackTimer += dt;
            float attackInterval = 1f / Model.AttackSpeed;

            if (_attackTimer >= attackInterval)
            {
                _attackTimer -= attackInterval;
                return true;
            }

            return false;
        }

        /// <summary>
        /// 적을 사망 상태로 전환한다.
        /// </summary>
        public void Die()
        {
            SetState(EnemyState.Death);
        }
    }
}
