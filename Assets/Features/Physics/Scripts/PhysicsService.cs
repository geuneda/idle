#if UNITY_6000_5_OR_NEWER
using Unity.U2D.Physics;
#else
using UnityEngine.LowLevelPhysics2D;
#endif

namespace IdleRPG.Physics
{
    /// <summary>
    /// PhysicsCore 2D 물리 월드를 초기화하고 관리하는 서비스.
    /// <see cref="PhysicsWorld.defaultWorld"/>의 레이어, Transform 쓰기 모드, 디버그 드로잉을 설정한다.
    /// </summary>
    public class PhysicsService : IPhysicsService
    {
        private readonly PhysicsWorld _world;
        private bool _isDebugDrawEnabled;

        /// <inheritdoc/>
        public PhysicsWorld World => _world;

        /// <inheritdoc/>
        public bool IsDebugDrawEnabled => _isDebugDrawEnabled;

        /// <summary>
        /// PhysicsService를 생성하고 기본 월드를 초기화한다.
        /// </summary>
        /// <param name="enableDebugDraw">개발 빌드에서 디버그 드로잉 활성화 여부</param>
        public PhysicsService(bool enableDebugDraw = false)
        {
            _world = PhysicsWorld.defaultWorld;

            ConfigureWorld();

            if (enableDebugDraw)
            {
                SetDebugDraw(true);
            }
        }

        /// <inheritdoc/>
        public void SetDebugDraw(bool enabled)
        {
            _isDebugDrawEnabled = enabled;

            if (enabled)
            {
                _world.drawOptions = PhysicsWorld.DrawOptions.AllBodies
                                     | PhysicsWorld.DrawOptions.AllShapes;
                _world.drawFillOptions = PhysicsWorld.DrawFillOptions.All;
            }
            else
            {
                _world.drawOptions = PhysicsWorld.DrawOptions.Off;
            }
        }

        /// <summary>
        /// 물리 월드 기본 설정을 구성한다.
        /// Transform 쓰기 모드를 활성화하여 물리 바디가 Transform을 자동 동기화하도록 한다.
        /// </summary>
        private void ConfigureWorld()
        {
            _world.transformWriteMode = PhysicsWorld.TransformWriteMode.Fast2D;
            _world.autoContactCallbacks = true;
            _world.autoTriggerCallbacks = true;
        }
    }
}
