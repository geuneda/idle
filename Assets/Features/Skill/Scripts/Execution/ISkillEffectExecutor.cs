namespace IdleRPG.Skill
{
    /// <summary>
    /// 스킬 효과를 실행하는 전략 인터페이스.
    /// 스킬 ID별로 구현체를 등록하여 각 스킬 고유의 효과를 처리한다.
    /// </summary>
    public interface ISkillEffectExecutor
    {
        /// <summary>이 실행기가 처리하는 스킬 ID</summary>
        int SkillId { get; }

        /// <summary>
        /// 스킬 효과를 실행한다.
        /// </summary>
        /// <param name="context">스킬 실행에 필요한 컨텍스트 데이터</param>
        /// <param name="entry">스킬 설정 데이터</param>
        /// <param name="damagePercent">현재 레벨 기준 데미지 퍼센트</param>
        void Execute(SkillExecutionContext context, SkillEntry entry, double damagePercent);
    }
}
