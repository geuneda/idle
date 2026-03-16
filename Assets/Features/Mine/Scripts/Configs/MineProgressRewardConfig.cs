using System;
using IdleRPG.Economy;

namespace IdleRPG.Mine
{
    /// <summary>
    /// 층별 진행도 보상 설정.
    /// </summary>
    [Serializable]
    public class MineProgressRewardConfig
    {
        /// <summary>보상이 지급되는 층 번호</summary>
        public int Floor = 1;

        /// <summary>보상 재화 종류</summary>
        public CurrencyType RewardType = CurrencyType.MineOre;

        /// <summary>보상 수량</summary>
        public int RewardAmount = 10;
    }
}
