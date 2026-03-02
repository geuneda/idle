using Geuneda.Services;

namespace IdleRPG.Core
{
    /// <summary>
    /// 스킬 보유효과 합산값이 변경되었을 때 발행되는 메시지.
    /// <see cref="ISkillBonusProvider"/>의 값이 갱신된 후 발행된다.
    /// Equipment 등 다른 시스템이 Core 수준에서 구독할 수 있도록 Core에 정의한다.
    /// </summary>
    public struct SkillBonusChangedMessage : IMessage
    {
    }
}
