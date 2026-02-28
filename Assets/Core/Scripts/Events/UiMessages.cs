using Geuneda.Services;

namespace IdleRPG.Core
{
    /// <summary>
    /// 하단 탭이 열렸을 때 발행되는 메시지.
    /// <see cref="MainBattleUiPresenter"/> 등이 구독하여 표시/숨김을 제어한다.
    /// </summary>
    public struct TabOpenedMessage : IMessage
    {
        /// <summary>열린 탭의 종류 (정수 값으로 전달)</summary>
        public int TabIndex;
    }

    /// <summary>
    /// 열려있던 탭이 닫혔을 때 발행되는 메시지
    /// </summary>
    public struct TabClosedMessage : IMessage
    {
    }
}
