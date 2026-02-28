using Geuneda.DataExtensions;
using Geuneda.Services;
using IdleRPG.Core;

namespace IdleRPG.Economy
{
    /// <summary>
    /// <see cref="ICurrencyService"/>의 구현체.
    /// 모든 재화 변경을 <see cref="CommandService{TGameLogic}"/>을 통해 처리하여
    /// 일관된 이벤트 발행과 추적을 보장한다.
    /// </summary>
    public class CurrencyService : ICurrencyService
    {
        private readonly CurrencyModel _model;
        private readonly CommandService<CurrencyModel> _commandService;

        /// <inheritdoc />
        public IObservableDictionaryReader<CurrencyType, BigNumber> Currencies => _model.Currencies;

        /// <summary>
        /// <see cref="CurrencyService"/>를 생성한다.
        /// </summary>
        /// <param name="model">재화 데이터 모델</param>
        /// <param name="messageBroker">이벤트 통신용 메시지 브로커</param>
        public CurrencyService(CurrencyModel model, IMessageBrokerService messageBroker)
        {
            _model = model;
            _commandService = new CommandService<CurrencyModel>(model, messageBroker);
        }

        /// <inheritdoc />
        public void Add(CurrencyType type, BigNumber amount)
        {
            if (amount <= BigNumber.Zero) return;

            _commandService.ExecuteCommand(new AddCurrencyCommand
            {
                Type = type,
                Amount = amount
            });
        }

        /// <inheritdoc />
        public bool TrySpend(CurrencyType type, BigNumber amount)
        {
            if (amount <= BigNumber.Zero) return false;
            if (!_model.HasEnough(type, amount)) return false;

            _commandService.ExecuteCommand(new SpendCurrencyCommand
            {
                Type = type,
                Amount = amount
            });
            return true;
        }

        /// <inheritdoc />
        public bool HasEnough(CurrencyType type, BigNumber amount)
        {
            return _model.HasEnough(type, amount);
        }

        /// <inheritdoc />
        public BigNumber GetAmount(CurrencyType type)
        {
            return _model.GetAmount(type);
        }
    }
}
