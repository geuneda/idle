using Geuneda.Services;
using UnityEngine;

namespace IdleRPG.Pet
{
    /// <summary>
    /// 전투 중 펫의 시각적 표현을 담당하는 뷰 컴포넌트.
    /// <see cref="IPoolEntitySpawn{T}"/>을 구현하여 오브젝트 풀에서 재사용된다.
    /// </summary>
    public class PetView : MonoBehaviour, IPoolEntitySpawn<PetViewData>, IPoolEntityDespawn
    {
        private SpriteRenderer _spriteRenderer;
        private Animator _animator;

        private static readonly int AttackTrigger = Animator.StringToHash("Attack");

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _animator = GetComponent<Animator>();
        }

        /// <summary>
        /// 풀에서 스폰될 때 호출된다. 애니메이터 컨트롤러를 설정한다.
        /// </summary>
        /// <param name="data">펫 뷰 설정 데이터</param>
        public void OnSpawn(PetViewData data)
        {
            if (_animator != null && data.AnimatorController != null)
            {
                _animator.runtimeAnimatorController = data.AnimatorController;
            }
        }

        /// <summary>
        /// 풀로 반환될 때 호출된다.
        /// </summary>
        public void OnDespawn()
        {
        }

        /// <summary>
        /// 공격 애니메이션을 재생한다.
        /// </summary>
        public void PlayAttack()
        {
            if (_animator != null)
            {
                _animator.SetTrigger(AttackTrigger);
            }
        }

        /// <summary>
        /// 펫의 월드 좌표를 설정한다.
        /// </summary>
        /// <param name="pos">설정할 월드 좌표</param>
        public void SetPosition(Vector3 pos)
        {
            transform.position = pos;
        }
    }

    /// <summary>
    /// 펫 뷰 스폰 시 필요한 데이터를 담는 구조체.
    /// </summary>
    public struct PetViewData
    {
        /// <summary>펫 본체의 애니메이터 컨트롤러</summary>
        public RuntimeAnimatorController AnimatorController;
    }
}
