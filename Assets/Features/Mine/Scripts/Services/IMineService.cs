using System;
using System.Collections.Generic;
using IdleRPG.Core;

namespace IdleRPG.Mine
{
    /// <summary>
    /// 광산 컨텐츠 서비스 인터페이스.
    /// </summary>
    public interface IMineService
    {
        /// <summary>광산 모델</summary>
        MineModel Model { get; }

        /// <summary>광산 설정</summary>
        MineConfigCollection Config { get; }

        /// <summary>현재 곡괭이 수량</summary>
        int CurrentPickaxeCount { get; }

        /// <summary>최대 곡괭이 수량</summary>
        int MaxPickaxeCount { get; }

        /// <summary>곡괭이 최대치 여부</summary>
        bool IsPickaxeMaxed { get; }

        /// <summary>다음 충전까지 남은 시간</summary>
        TimeSpan NextRechargeTime { get; }

        /// <summary>보드를 초기화한다 (현재 층 기준).</summary>
        void InitializeBoard();

        /// <summary>해당 도구를 사용할 수 있는지 확인한다.</summary>
        bool CanUseTool(MineToolType tool);

        /// <summary>도구 사용 시 영향받는 블록 인덱스를 반환한다.</summary>
        List<int> GetAffectedBlocks(MineToolType tool, int centerIndex);

        /// <summary>도구를 사용하여 블록을 파괴한다.</summary>
        void UseTool(MineToolType tool, int centerIndex);

        /// <summary>블록의 보상을 수집한다.</summary>
        void CollectReward(int blockIndex);

        /// <summary>미수령 보상을 일괄 수집한다.</summary>
        void CollectAllUncollected();

        /// <summary>보물상자가 발견(Revealed)되었는지 여부</summary>
        bool IsChestRevealed { get; }

        /// <summary>보물상자 강화가 완료되었는지 여부</summary>
        bool IsChestCompleted { get; }

        /// <summary>보물상자 현재 등급을 반환한다.</summary>
        ItemGrade GetChestCurrentGrade();

        /// <summary>보물상자 강화를 시도한다. 성공/실패를 반환한다.</summary>
        bool TryUpgradeChest();

        /// <summary>보물상자 보상을 수령한다.</summary>
        void ClaimChestReward();

        /// <summary>다음 층으로 이동 가능한지 여부</summary>
        bool CanAdvanceFloor { get; }

        /// <summary>다음 층으로 이동한다.</summary>
        void AdvanceFloor();

        /// <summary>진행도 보상을 수령할 수 있는지 확인한다.</summary>
        bool CanClaimProgressReward(int floor);

        /// <summary>진행도 보상을 수령한다.</summary>
        void ClaimProgressReward(int floor);

        /// <summary>다음 미수령 진행도 보상 층을 반환한다.</summary>
        int GetNextUnclaimedProgressFloor();

        /// <summary>현재 진행도 비율을 반환한다 (0~1).</summary>
        float GetProgressFraction();

        /// <summary>빠른 클리어 가능 여부</summary>
        bool CanQuickClear { get; }

        /// <summary>빠른 클리어를 실행하고 결과를 반환한다.</summary>
        MineQuickClearResult ExecuteQuickClear();
    }
}
