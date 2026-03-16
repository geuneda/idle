using Geuneda.Services;
using IdleRPG.Core;
using IdleRPG.Economy;

namespace IdleRPG.Mine
{
    /// <summary>블록 파괴 메시지</summary>
    public struct MineBlocksDestroyedMessage : IMessage
    {
        /// <summary>파괴된 블록 인덱스 배열</summary>
        public int[] DestroyedIndices;
        /// <summary>사용된 도구</summary>
        public MineToolType ToolUsed;
    }

    /// <summary>보상 수집 메시지</summary>
    public struct MineRewardCollectedMessage : IMessage
    {
        /// <summary>블록 인덱스</summary>
        public int BlockIndex;
        /// <summary>보상 종류</summary>
        public MineBlockRewardType RewardType;
        /// <summary>보상 수량</summary>
        public int Amount;
    }

    /// <summary>보물상자 발견 메시지</summary>
    public struct MineChestRevealedMessage : IMessage
    {
        /// <summary>상자 블록 인덱스</summary>
        public int ChestIndex;
    }

    /// <summary>보물상자 강화 시도 메시지</summary>
    public struct MineChestUpgradeAttemptedMessage : IMessage
    {
        /// <summary>강화 슬롯 인덱스 (0~3)</summary>
        public int SlotIndex;
        /// <summary>성공 여부</summary>
        public bool Success;
        /// <summary>결과 등급</summary>
        public ItemGrade ResultGrade;
    }

    /// <summary>보물상자 보상 수령 메시지</summary>
    public struct MineChestRewardClaimedMessage : IMessage
    {
        /// <summary>최종 등급</summary>
        public ItemGrade FinalGrade;
    }

    /// <summary>층 이동 메시지</summary>
    public struct MineFloorAdvancedMessage : IMessage
    {
        /// <summary>이전 층</summary>
        public int PreviousFloor;
        /// <summary>새 층</summary>
        public int NewFloor;
    }

    /// <summary>진행도 보상 수령 메시지</summary>
    public struct MineProgressRewardClaimedMessage : IMessage
    {
        /// <summary>보상 층</summary>
        public int Floor;
        /// <summary>보상 재화 종류</summary>
        public CurrencyType RewardType;
        /// <summary>보상 수량</summary>
        public BigNumber RewardAmount;
    }

    /// <summary>곡괭이 충전 메시지</summary>
    public struct MinePickaxeRechargedMessage : IMessage
    {
        /// <summary>충전 후 수량</summary>
        public int NewCount;
        /// <summary>최대 수량</summary>
        public int MaxCount;
    }

    /// <summary>빠른 클리어 완료 메시지</summary>
    public struct MineQuickClearCompletedMessage : IMessage
    {
        /// <summary>결과 데이터</summary>
        public MineQuickClearResult Result;
    }

    /// <summary>광산 상태 변경 메시지 (세이브 더티 마킹용)</summary>
    public struct MineStateChangedMessage : IMessage { }
}
