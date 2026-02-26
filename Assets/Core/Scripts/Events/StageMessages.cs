using Geuneda.Services;

namespace IdleRPG.Core
{
    /// <summary>
    /// 웨이브가 시작되었을 때 발행되는 메시지
    /// </summary>
    public struct WaveStartedMessage : IMessage
    {
        /// <summary>현재 챕터 번호 (1-based)</summary>
        public int Chapter;

        /// <summary>현재 스테이지 번호 (1-based, 챕터 내)</summary>
        public int Stage;

        /// <summary>현재 웨이브 인덱스 (0-based)</summary>
        public int Wave;

        /// <summary>보스 웨이브 여부</summary>
        public bool IsBossWave;
    }

    /// <summary>
    /// 웨이브를 클리어했을 때 발행되는 메시지
    /// </summary>
    public struct WaveClearedMessage : IMessage
    {
        /// <summary>클리어한 챕터 번호</summary>
        public int Chapter;

        /// <summary>클리어한 스테이지 번호</summary>
        public int Stage;

        /// <summary>클리어한 웨이브 인덱스</summary>
        public int Wave;
    }

    /// <summary>
    /// 스테이지의 모든 웨이브(보스 포함)를 클리어했을 때 발행되는 메시지
    /// </summary>
    public struct StageClearedMessage : IMessage
    {
        /// <summary>클리어한 챕터 번호</summary>
        public int Chapter;

        /// <summary>클리어한 스테이지 번호</summary>
        public int Stage;
    }

    /// <summary>
    /// 챕터의 모든 스테이지를 클리어했을 때 발행되는 메시지
    /// </summary>
    public struct ChapterClearedMessage : IMessage
    {
        /// <summary>클리어한 챕터 번호</summary>
        public int Chapter;
    }

    /// <summary>
    /// 보스 도전에 실패했을 때 발행되는 메시지.
    /// 보스 자동 도전이 해제되고 일반 웨이브 무한 반복 모드로 전환된다.
    /// </summary>
    public struct BossChallengeFailedMessage : IMessage
    {
        /// <summary>실패한 챕터 번호</summary>
        public int Chapter;

        /// <summary>실패한 스테이지 번호</summary>
        public int Stage;
    }

    /// <summary>
    /// 보스 자동 도전 설정이 변경되었을 때 발행되는 메시지
    /// </summary>
    public struct BossAutoChallengeChangedMessage : IMessage
    {
        /// <summary>보스 자동 도전 활성화 여부</summary>
        public bool IsEnabled;
    }
}
