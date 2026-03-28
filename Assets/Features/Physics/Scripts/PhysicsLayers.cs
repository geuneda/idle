#if UNITY_6000_5_OR_NEWER
using Unity.U2D.Physics;
#else
using UnityEngine.LowLevelPhysics2D;
#endif

namespace IdleRPG.Physics
{
    /// <summary>
    /// PhysicsCore 2D 물리 레이어 상수 정의.
    /// <see cref="PhysicsMask"/> 비트 인덱스 기반 (0~63).
    /// </summary>
    public static class PhysicsLayers
    {
        /// <summary>기본 레이어</summary>
        public const int Default = 0;

        /// <summary>영웅 레이어</summary>
        public const int Hero = 1;

        /// <summary>적 레이어</summary>
        public const int Enemy = 2;

        /// <summary>투사체 레이어</summary>
        public const int Projectile = 3;

        /// <summary>트리거 영역 (보상, 이벤트 등)</summary>
        public const int Trigger = 4;

        /// <summary>지형/벽 레이어</summary>
        public const int Environment = 5;

        /// <summary>영웅 카테고리 마스크</summary>
        public static readonly PhysicsMask HeroMask = new PhysicsMask(Hero);

        /// <summary>적 카테고리 마스크</summary>
        public static readonly PhysicsMask EnemyMask = new PhysicsMask(Enemy);

        /// <summary>투사체 카테고리 마스크</summary>
        public static readonly PhysicsMask ProjectileMask = new PhysicsMask(Projectile);

        /// <summary>트리거 카테고리 마스크</summary>
        public static readonly PhysicsMask TriggerMask = new PhysicsMask(Trigger);

        /// <summary>환경 카테고리 마스크</summary>
        public static readonly PhysicsMask EnvironmentMask = new PhysicsMask(Environment);

        /// <summary>영웅이 충돌할 대상 (적 + 환경 + 트리거)</summary>
        public static readonly PhysicsMask HeroContacts = new PhysicsMask(Enemy, Environment, Trigger);

        /// <summary>적이 충돌할 대상 (영웅 + 투사체 + 환경)</summary>
        public static readonly PhysicsMask EnemyContacts = new PhysicsMask(Hero, Projectile, Environment);

        /// <summary>투사체가 충돌할 대상 (적 + 환경)</summary>
        public static readonly PhysicsMask ProjectileContacts = new PhysicsMask(Enemy, Environment);
    }
}
