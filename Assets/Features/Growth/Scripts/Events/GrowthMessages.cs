using Geuneda.Services;
using IdleRPG.Core;
using IdleRPG.Hero;

namespace IdleRPG.Growth
{
    /// <summary>
    /// 스탯 레벨업 완료 시 발행되는 메시지.
    /// </summary>
    public struct StatLevelUpMessage : IMessage
    {
        /// <summary>레벨업된 스탯 유형</summary>
        public HeroStatType StatType;

        /// <summary>레벨업 후 레벨</summary>
        public int NewLevel;

        /// <summary>레벨업에 소비된 골드</summary>
        public BigNumber SpentGold;
    }

    /// <summary>
    /// 전투력이 변경되었을 때 발행되는 메시지.
    /// </summary>
    public struct CombatPowerChangedMessage : IMessage
    {
        /// <summary>이전 전투력</summary>
        public BigNumber PreviousPower;

        /// <summary>변경 후 전투력</summary>
        public BigNumber CurrentPower;
    }
}
