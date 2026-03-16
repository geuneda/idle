using System;

namespace IdleRPG.Mine
{
    /// <summary>
    /// 광산 보드판의 개별 블록 데이터. 불변 구조체이다.
    /// </summary>
    [Serializable]
    public readonly struct MineBlock
    {
        /// <summary>블록 현재 상태</summary>
        public MineBlockState State { get; }

        /// <summary>블록에 포함된 보상 종류</summary>
        public MineBlockRewardType RewardType { get; }

        /// <summary>보상 수량</summary>
        public int RewardAmount { get; }

        /// <summary>
        /// MineBlock을 생성한다.
        /// </summary>
        /// <param name="state">블록 상태</param>
        /// <param name="rewardType">보상 종류</param>
        /// <param name="rewardAmount">보상 수량</param>
        public MineBlock(MineBlockState state, MineBlockRewardType rewardType, int rewardAmount)
        {
            State = state;
            RewardType = rewardType;
            RewardAmount = rewardAmount;
        }

        /// <summary>
        /// 상태만 변경한 새 블록을 반환한다.
        /// </summary>
        /// <param name="newState">새 상태</param>
        /// <returns>상태가 변경된 새 MineBlock</returns>
        public MineBlock WithState(MineBlockState newState) =>
            new MineBlock(newState, RewardType, RewardAmount);
    }
}
