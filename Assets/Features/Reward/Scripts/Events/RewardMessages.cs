using Geuneda.Services;
using IdleRPG.Core;
using IdleRPG.Economy;

namespace IdleRPG.Reward
{
    /// <summary>
    /// 보상이 지급되었을 때 발행되는 메시지. UI에서 보상 획득 연출에 사용한다.
    /// </summary>
    public struct RewardGrantedMessage : IMessage
    {
        /// <summary>지급된 재화 종류</summary>
        public CurrencyType Type;

        /// <summary>지급된 양</summary>
        public BigNumber Amount;

        /// <summary>보스 처치 보상 여부</summary>
        public bool IsBossReward;
    }
}
