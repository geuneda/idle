using Geuneda.Services;
using UnityEngine;

namespace IdleRPG.Battle
{
    /// <summary>
    /// 투사체의 이동과 충돌 처리를 담당하는 뷰 컴포넌트.
    /// <see cref="IPoolEntitySpawn{T}"/>을 구현하여 오브젝트 풀에서 재사용된다.
    /// </summary>
    public class ProjectileView : MonoBehaviour, IPoolEntitySpawn<ProjectileData>, IPoolEntityDespawn
    {
        private ProjectileData _data;
        private bool _active;
        private IPoolService _poolService;

        /// <summary>
        /// 풀 서비스 참조를 설정한다. 스폰 후 반드시 호출해야 한다.
        /// </summary>
        /// <param name="poolService">오브젝트 풀 서비스</param>
        public void Init(IPoolService poolService)
        {
            _poolService = poolService;
        }

        /// <summary>
        /// 풀에서 스폰될 때 호출된다. 투사체 데이터를 설정하고 활성화한다.
        /// </summary>
        /// <param name="data">투사체 설정 데이터 (대상, 피해량, 속도 등)</param>
        public void OnSpawn(ProjectileData data)
        {
            _data = data;
            _active = true;
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
