using Geuneda.Services;

namespace IdleRPG.Core
{
    public struct WaveStartedMessage : IMessage
    {
        public int Chapter;
        public int Stage;
        public int Wave;
        public bool IsBossWave;
    }

    public struct WaveClearedMessage : IMessage
    {
        public int Chapter;
        public int Stage;
        public int Wave;
    }

    public struct StageClearedMessage : IMessage
    {
        public int Chapter;
        public int Stage;
    }

    public struct ChapterClearedMessage : IMessage
    {
        public int Chapter;
    }

    public struct BossChallengeFailedMessage : IMessage
    {
        public int Chapter;
        public int Stage;
    }

    public struct BossAutoChallengeChangedMessage : IMessage
    {
        public bool IsEnabled;
    }
}
