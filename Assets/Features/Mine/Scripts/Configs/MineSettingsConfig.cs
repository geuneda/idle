using System;

namespace IdleRPG.Mine
{
    /// <summary>
    /// 광산 글로벌 설정.
    /// </summary>
    [Serializable]
    public class MineSettingsConfig
    {
        /// <summary>곡괭이 최대 보유량</summary>
        public int MaxPickaxe = 50;

        /// <summary>곡괭이 충전 주기 (초)</summary>
        public int PickaxeRechargeSeconds = 900;

        /// <summary>최대 도달 가능 층</summary>
        public int MaxFloor = 4000;

        /// <summary>보물상자 강화 성공 확률 (0~1)</summary>
        public float ChestUpgradeChance = 0.25f;

        /// <summary>보물상자 강화 슬롯 수</summary>
        public int ChestUpgradeSlots = 4;
    }
}
