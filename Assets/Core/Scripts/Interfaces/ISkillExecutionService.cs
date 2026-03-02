using System.Collections.Generic;
using UnityEngine;

namespace IdleRPG.Core
{
    /// <summary>
    /// 전투 중 스킬 실행을 관리하는 서비스 인터페이스.
    /// <see cref="Battle.BattleField"/>가 매 프레임 Tick을 호출하여 스킬 쿨타임과 오토캐스트를 처리한다.
    /// </summary>
    public interface ISkillExecutionService
    {
        /// <summary>
        /// 매 프레임 호출하여 쿨타임 감소 및 오토캐스트를 처리한다.
        /// </summary>
        /// <param name="dt">프레임 경과 시간</param>
        /// <param name="heroAttack">현재 영웅 공격력</param>
        /// <param name="heroPosition">영웅 월드 좌표</param>
        /// <param name="targets">현재 활성 적 목록</param>
        void Tick(float dt, BigNumber heroAttack, Vector3 heroPosition, IReadOnlyList<ISkillTarget> targets);

        /// <summary>
        /// 지정 슬롯의 스킬을 수동으로 발동한다.
        /// </summary>
        /// <param name="slotIndex">스킬 슬롯 인덱스 (0~5)</param>
        /// <param name="heroAttack">현재 영웅 공격력</param>
        /// <param name="heroPosition">영웅 월드 좌표</param>
        /// <param name="targets">현재 활성 적 목록</param>
        /// <returns>발동 성공 여부</returns>
        bool TryManualActivate(int slotIndex, BigNumber heroAttack, Vector3 heroPosition, IReadOnlyList<ISkillTarget> targets);

        /// <summary>
        /// 지정 슬롯의 남은 쿨타임을 반환한다.
        /// </summary>
        /// <param name="slotIndex">스킬 슬롯 인덱스</param>
        /// <returns>남은 쿨타임 (초). 쿨타임이 아니면 0</returns>
        float GetCooldownRemaining(int slotIndex);

        /// <summary>
        /// 지정 슬롯의 전체 쿨타임을 반환한다.
        /// </summary>
        /// <param name="slotIndex">스킬 슬롯 인덱스</param>
        /// <returns>전체 쿨타임 (초). 스킬 미장착이면 0</returns>
        float GetCooldownTotal(int slotIndex);

        /// <summary>
        /// 모든 실행 상태를 초기화한다. 전투 종료/스테이지 전환 시 호출하여
        /// 활성 지속 효과와 쿨타임을 정리한다.
        /// </summary>
        void ResetState();
    }
}
