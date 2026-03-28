#if UNITY_6000_5_OR_NEWER
using Unity.U2D.Physics;
#else
using UnityEngine.LowLevelPhysics2D;
#endif

namespace IdleRPG.Physics
{
    /// <summary>
    /// PhysicsCore 2D 월드 관리 서비스 인터페이스.
    /// 물리 월드 초기화, 레이어 설정, 디버그 옵션을 제공한다.
    /// </summary>
    public interface IPhysicsService
    {
        /// <summary>기본 물리 월드</summary>
        PhysicsWorld World { get; }

        /// <summary>디버그 드로잉 활성화 여부</summary>
        bool IsDebugDrawEnabled { get; }

        /// <summary>디버그 드로잉을 토글한다.</summary>
        /// <param name="enabled">활성화 여부</param>
        void SetDebugDraw(bool enabled);
    }
}
