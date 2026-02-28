using Geuneda.Services;

namespace IdleRPG.Core
{
    /// <summary>
    /// 앱 흐름이 InGame 상태에 진입하고 초기 UI 오픈이 완료되었을 때 발행되는 메시지.
    /// 게임플레이 시스템이 이 메시지를 구독하여 초기화를 시작할 수 있다.
    /// </summary>
    public struct AppFlowReadyMessage : IMessage
    {
    }

    /// <summary>
    /// 로딩 진행률을 전달하는 메시지. 로딩 UI가 구독하여 진행 표시줄을 갱신한다.
    /// </summary>
    public struct LoadingProgressMessage : IMessage
    {
        /// <summary>로딩 진행률 (0.0 ~ 1.0)</summary>
        public float Progress;
    }
}
