using UnityEngine;

namespace IdleRPG.Core
{
    /// <summary>
    /// 스킬 데미지를 받을 수 있는 대상의 추상화.
    /// <see cref="Battle.EnemyView"/>가 이 인터페이스를 구현하여 스킬 실행 시스템과의 순환 의존성을 방지한다.
    /// </summary>
    public interface ISkillTarget
    {
        /// <summary>대상이 살아있는지 여부</summary>
        bool IsAlive { get; }

        /// <summary>대상의 월드 좌표</summary>
        Vector3 Position { get; }

        /// <summary>
        /// 대상에게 데미지를 적용한다.
        /// </summary>
        /// <param name="damage">적용할 데미지 양</param>
        void TakeDamage(BigNumber damage);
    }
}
