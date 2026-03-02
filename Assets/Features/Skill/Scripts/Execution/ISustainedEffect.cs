namespace IdleRPG.Skill
{
    /// <summary>
    /// 지속 효과 인터페이스. 소환/DoT 등 시간 경과에 따라 반복 실행되는 효과를 표현한다.
    /// </summary>
    public interface ISustainedEffect
    {
        /// <summary>효과가 만료되었는지 여부</summary>
        bool IsExpired { get; }

        /// <summary>
        /// 매 프레임 호출되어 지속 효과를 갱신한다.
        /// </summary>
        /// <param name="dt">프레임 간격 (초)</param>
        /// <param name="context">스킬 실행 컨텍스트</param>
        void Tick(float dt, SkillExecutionContext context);
    }
}
