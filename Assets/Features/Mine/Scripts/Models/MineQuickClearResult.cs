using System.Collections.Generic;
using IdleRPG.Core;
using IdleRPG.Economy;

namespace IdleRPG.Mine
{
    /// <summary>
    /// 광산 빠른 클리어 결과 데이터.
    /// </summary>
    public class MineQuickClearResult
    {
        /// <summary>소모한 곡괭이 수</summary>
        public int PickaxesUsed;

        /// <summary>클리어한 층 수</summary>
        public int FloorsCleared;

        /// <summary>획득 보상 목록 (재화별 합산)</summary>
        public Dictionary<CurrencyType, BigNumber> TotalRewards = new Dictionary<CurrencyType, BigNumber>();

        /// <summary>
        /// 보상을 누적 추가한다.
        /// </summary>
        public void AddReward(CurrencyType type, BigNumber amount)
        {
            if (TotalRewards.TryGetValue(type, out var existing))
                TotalRewards[type] = existing + amount;
            else
                TotalRewards[type] = amount;
        }
    }
}
