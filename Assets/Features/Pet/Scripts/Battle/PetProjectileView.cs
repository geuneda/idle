using Geuneda.Services;
using IdleRPG.Core;
using UnityEngine;

namespace IdleRPG.Pet
{
    /// <summary>
    /// 펫 투사체의 이동과 충돌 처리를 담당하는 뷰 컴포넌트.
    /// <see cref="IPoolEntitySpawn{T}"/>을 구현하여 오브젝트 풀에서 재사용된다.
    /// </summary>
    public class PetProjectileView : MonoBehaviour, IPoolEntitySpawn<PetProjectileData>, IPoolEntityDespawn
    {
        private SpriteRenderer _spriteRenderer;
        private Animator _animator;

        private PetProjectileData _data;
        private bool _active;
        private IPoolService _poolService;
        private IMessageBrokerService _messageBroker;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _animator = GetComponent<Animator>();
        }

        /// <summary>
        /// 풀 서비스 및 메시지 브로커 참조를 설정한다. 스폰 후 반드시 호출해야 한다.
        /// </summary>
        /// <param name="poolService">오브젝트 풀 서비스</param>
        /// <param name="messageBroker">메시지 브로커 서비스</param>
        public void Init(IPoolService poolService, IMessageBrokerService messageBroker)
        {
            _poolService = poolService;
            _messageBroker = messageBroker;
        }

        /// <summary>
        /// 풀에서 스폰될 때 호출된다. 투사체 데이터를 설정하고 활성화한다.
        /// </summary>
        /// <param name="data">펫 투사체 설정 데이터 (대상, 피해량, 속도, 애니메이터 등)</param>
        public void OnSpawn(PetProjectileData data)
        {
            _data = data;
            _active = true;

            if (_animator != null && data.AnimatorController != null)
            {
                _animator.runtimeAnimatorController = data.AnimatorController;
            }
        }

        /// <summary>
        /// 풀로 반환될 때 호출된다. 투사체를 비활성화한다.
        /// </summary>
        public void OnDespawn()
        {
            _active = false;
        }

        private void Update()
        {
            if (!_active) return;

            if (_data.Target == null || _data.Target.Model == null || !_data.Target.Model.IsAlive)
            {
                Despawn();
                return;
            }

            Vector3 targetPos = _data.Target.transform.position;
            Vector3 direction = (targetPos - transform.position).normalized;
            transform.position += direction * _data.Speed * Time.deltaTime;

            float distance = Vector3.Distance(transform.position, targetPos);
            if (distance < 0.3f)
            {
                _data.Target.Model.TakeDamage(_data.Damage);

                _messageBroker?.Publish(new EnemyDamagedMessage
                {
                    EnemyIndex = _data.Target.Index,
                    Damage = _data.Damage,
                    IsCritical = _data.IsCritical
                });

                if (!_data.Target.Model.IsAlive)
                {
                    _data.Target.Die();
                }

                Despawn();
            }
        }

        private void Despawn()
        {
            _active = false;
            if (_poolService != null)
            {
                _poolService.Despawn(this);
            }
        }
    }
}
