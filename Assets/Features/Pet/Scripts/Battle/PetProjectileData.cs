using IdleRPG.Battle;
using IdleRPG.Core;
using UnityEngine;

namespace IdleRPG.Pet
{
    /// <summary>
    /// 펫 투사체 생성 시 필요한 데이터를 담는 구조체.
    /// </summary>
    public struct PetProjectileData
    {
        /// <summary>투사체의 대상 적</summary>
        public EnemyView Target;

        /// <summary>투사체가 가하는 피해량</summary>
        public BigNumber Damage;

        /// <summary>치명타 여부</summary>
        public bool IsCritical;

        /// <summary>투사체 이동 속도</summary>
        public float Speed;

        /// <summary>펫별 투사체 애니메이터 컨트롤러</summary>
        public RuntimeAnimatorController AnimatorController;
    }
}
