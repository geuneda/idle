using System;
using System.Collections.Generic;
using Geuneda.Services;
using IdleRPG.Core;
using UnityEngine;

namespace IdleRPG.Skill
{
    /// <summary>
    /// 스킬 실행에 필요한 컨텍스트 데이터.
    /// 매 프레임 재사용되어 GC 할당을 방지한다.
    /// </summary>
    public class SkillExecutionContext
    {
        /// <summary>현재 영웅 공격력</summary>
        public BigNumber HeroAttack;

        /// <summary>영웅 월드 좌표</summary>
        public Vector3 HeroPosition;

        /// <summary>현재 활성 적 목록</summary>
        public IReadOnlyList<ISkillTarget> Targets;

        /// <summary>메시지 브로커 (이벤트 발행용)</summary>
        public IMessageBrokerService MessageBroker;

        /// <summary>지속 효과를 등록하는 콜백. 소환/DoT 등이 이를 통해 등록된다.</summary>
        public Action<ISustainedEffect> RegisterSustainedEffect;

        /// <summary>가장 긴 남은 쿨타임을 비율만큼 감소시키는 콜백. 어두운구체 전용.</summary>
        public Action<float> ReduceLongestCooldown;
    }
}
