using Geuneda.Services;
using IdleRPG.Core;
using IdleRPG.Economy;

namespace IdleRPG.Dungeon
{
    /// <summary>던전 입장 시 발행되는 메시지</summary>
    public struct DungeonEnteredMessage : IMessage
    {
        public DungeonType Type;
        public int Level;
    }

    /// <summary>던전 웨이브 시작 시 발행되는 메시지</summary>
    public struct DungeonWaveStartedMessage : IMessage
    {
        public DungeonType Type;
        public int WaveIndex;
        public int TotalWaves;
    }

    /// <summary>던전 웨이브 클리어 시 발행되는 메시지</summary>
    public struct DungeonWaveClearedMessage : IMessage
    {
        public DungeonType Type;
        public int WaveIndex;
        public int TotalWaves;
    }

    /// <summary>던전 전체 웨이브 클리어 시 발행되는 메시지</summary>
    public struct DungeonAllWavesClearedMessage : IMessage
    {
        public DungeonType Type;
        public int Level;
    }

    /// <summary>던전 내 영웅 사망 시 발행되는 메시지</summary>
    public struct DungeonHeroDiedMessage : IMessage
    {
        public DungeonType Type;
        public int Level;
    }

    /// <summary>던전 시간 만료 시 발행되는 메시지</summary>
    public struct DungeonTimeExpiredMessage : IMessage
    {
        public DungeonType Type;
        public int Level;
    }

    /// <summary>던전 클리어 완료 시 발행되는 메시지 (보상 지급 후)</summary>
    public struct DungeonCompletedMessage : IMessage
    {
        public DungeonType Type;
        public int Level;
        public CurrencyType RewardType;
        public BigNumber RewardAmount;
    }

    /// <summary>던전 실패 시 발행되는 메시지</summary>
    public struct DungeonFailedMessage : IMessage
    {
        public DungeonType Type;
        public int Level;
        public DungeonFailReason Reason;
    }

    /// <summary>던전 소탕 시 발행되는 메시지</summary>
    public struct DungeonSweptMessage : IMessage
    {
        public DungeonType Type;
        public int Level;
        public CurrencyType RewardType;
        public BigNumber RewardAmount;
    }

    /// <summary>던전 타이머 틱 시 발행되는 메시지</summary>
    public struct DungeonTimerTickMessage : IMessage
    {
        public float RemainingTime;
    }

    /// <summary>던전 일일 리셋 시 발행되는 메시지</summary>
    public struct DungeonDailyResetMessage : IMessage { }

    /// <summary>던전 퇴장 시 발행되는 메시지</summary>
    public struct DungeonExitedMessage : IMessage
    {
        public DungeonType Type;
    }

    /// <summary>던전 실패 사유</summary>
    public enum DungeonFailReason
    {
        /// <summary>제한 시간 초과</summary>
        TimeExpired = 0,

        /// <summary>영웅 사망</summary>
        HeroDied = 1
    }
}
