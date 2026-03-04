using System.Collections.Generic;
using UnityEngine;

namespace IdleRPG.Core
{
    /// <summary>
    /// 전투 중 펫 공격을 관리하는 서비스 인터페이스.
    /// <see cref="Battle.BattleField"/>가 매 프레임 Tick을 호출하여 펫 투사체 발사를 처리한다.
    /// </summary>
    public interface IPetCombatService
    {
        /// <summary>
        /// 매 프레임 호출하여 펫 공격 타이머를 갱신하고 투사체를 발사한다.
        /// </summary>
        /// <param name="dt">프레임 경과 시간</param>
        /// <param name="heroAttack">현재 영웅 공격력</param>
        /// <param name="heroPosition">영웅 월드 좌표</param>
        /// <param name="heroAttackRange">영웅 공격 사거리</param>
        /// <param name="targets">현재 활성 적 목록</param>
        void Tick(float dt, BigNumber heroAttack, Vector3 heroPosition, float heroAttackRange,
            IReadOnlyList<ISkillTarget> targets);

        /// <summary>
        /// 전투 시작 시 호출. 장착된 펫의 에셋을 로드하고 타이머를 초기화한다.
        /// </summary>
        void OnBattleStarted();

        /// <summary>
        /// 전투 종료 시 호출. 펫 뷰를 정리하고 타이머를 리셋한다.
        /// </summary>
        void OnBattleEnded();
    }
}
