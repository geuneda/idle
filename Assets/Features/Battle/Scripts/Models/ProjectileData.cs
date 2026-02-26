using IdleRPG.Core;

namespace IdleRPG.Battle
{
    /// <summary>
    /// 투사체 생성 시 필요한 데이터를 담는 구조체.
    /// <see cref="ProjectileView"/>의 <see cref="Geuneda.Services.IPoolEntitySpawn{T}"/>에 전달된다.
    /// </summary>
    public struct ProjectileData
    {
        /// <summary>투사체의 대상 적</summary>
        public EnemyView Target;

        /// <summary>투사체가 가하는 피해량</summary>
        public BigNumber Damage;

        /// <summary>치명타 여부</summary>
        public bool IsCritical;

        /// <summary>투사체 이동 속도</summary>
        public float Speed;
    }
}
