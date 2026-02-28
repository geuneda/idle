using Geuneda.Services;
using IdleRPG.Core;

namespace IdleRPG.Economy
{
    /// <summary>
    /// 재화 보유량이 변경되었을 때 발행되는 메시지.
    /// </summary>
    public struct CurrencyChangedMessage : IMessage
    {
        /// <summary>변경된 재화 종류</summary>
        public CurrencyType Type;

        /// <summary>변경 전 보유량</summary>
        public BigNumber PreviousAmount;

        /// <summary>변경 후 보유량</summary>
        public BigNumber CurrentAmount;

        /// <summary>변경량 (양수: 추가, 음수: 소비)</summary>
        public BigNumber Delta;
    }
}
