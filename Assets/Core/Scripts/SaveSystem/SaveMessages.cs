using Geuneda.Services;

namespace IdleRPG.Core
{
    /// <summary>
    /// 게임 데이터 저장이 완료되었을 때 발행되는 메시지.
    /// </summary>
    public struct GameSavedMessage : IMessage
    {
        /// <summary>저장 시각 (Unix 밀리초)</summary>
        public long Timestamp;
    }

    /// <summary>
    /// 저장 데이터 로드가 완료되었을 때 발행되는 메시지.
    /// </summary>
    public struct GameLoadedMessage : IMessage
    {
        /// <summary>로드된 저장 데이터의 존재 여부. false이면 신규 게임 시작.</summary>
        public bool HasExistingData;
    }
}
