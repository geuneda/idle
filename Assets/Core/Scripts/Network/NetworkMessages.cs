using Geuneda.Services;

namespace IdleRPG.Core
{
    /// <summary>서버 연결 상태 변경 메시지</summary>
    public struct ServerConnectionChangedMessage : IMessage
    {
        /// <summary>연결 성공 여부</summary>
        public bool IsConnected;
    }
}
