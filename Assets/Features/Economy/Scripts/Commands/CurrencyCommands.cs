using Geuneda.Services;
using IdleRPG.Core;

namespace IdleRPG.Economy
{
    /// <summary>
    /// 재화를 추가하는 커맨드. <see cref="CommandService{TGameLogic}"/>을 통해 실행된다.
    /// </summary>
    public struct AddCurrencyCommand : IGameCommand<CurrencyModel>
    {
        /// <summary>추가할 재화 종류</summary>
        public CurrencyType Type;

        /// <summary>추가할 양</summary>
        public BigNumber Amount;

        /// <inheritdoc />
        public void Execute(CurrencyModel model, IMessageBrokerService messageBroker)
        {
            var previous = model.GetAmount(Type);
            var current = model.Add(Type, Amount);

            messageBroker.Publish(new CurrencyChangedMessage
            {
                Type = Type,
                PreviousAmount = previous,
                CurrentAmount = current,
                Delta = Amount
            });
        }
    }

    /// <summary>
    /// 재화를 소비하는 커맨드. <see cref="CommandService{TGameLogic}"/>을 통해 실행된다.
    /// </summary>
    public struct SpendCurrencyCommand : IGameCommand<CurrencyModel>
    {
        /// <summary>소비할 재화 종류</summary>
        public CurrencyType Type;

        /// <summary>소비할 양</summary>
        public BigNumber Amount;

        /// <inheritdoc />
        public void Execute(CurrencyModel model, IMessageBrokerService messageBroker)
        {
            var previous = model.GetAmount(Type);
            var current = model.Spend(Type, Amount);

            messageBroker.Publish(new CurrencyChangedMessage
            {
                Type = Type,
                PreviousAmount = previous,
                CurrentAmount = current,
                Delta = -Amount
            });
        }
    }
}
