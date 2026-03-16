using System;
using IdleRPG.Core;

namespace IdleRPG.Mine
{
    /// <summary>
    /// 보물상자 등급별 보상 설정.
    /// </summary>
    [Serializable]
    public class MineChestRewardConfig
    {
        /// <summary>상자 등급 (Advanced=1 ~ Myth=5)</summary>
        public ItemGrade Grade = ItemGrade.Advanced;

        /// <summary>광물 보상 수량</summary>
        public int OreAmount = 10;

        /// <summary>경험치 파편 보상 수량</summary>
        public int ExpFragmentAmount = 5;

        /// <summary>보석(Gem) 보상 수량</summary>
        public int GemAmount = 0;
    }
}
