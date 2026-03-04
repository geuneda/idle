using System;
using Geuneda.Services;
using IdleRPG.Economy;

namespace IdleRPG.Core
{
    /// <summary>
    /// 재화 보유량의 저장/로드를 담당한다.
    /// </summary>
    public class CurrencySaveCollector : ISaveDataCollector
    {
        private static readonly CurrencyType[] CachedCurrencyTypes =
            (CurrencyType[])Enum.GetValues(typeof(CurrencyType));

        private readonly CurrencyModel _currencyModel;
        private Action _markDirty;

        public CurrencySaveCollector(CurrencyModel currencyModel)
        {
            _currencyModel = currencyModel;
        }

        /// <inheritdoc />
        public void ExtractTo(GameSaveData saveData)
        {
            var data = new CurrencySaveData();
            foreach (var type in CachedCurrencyTypes)
            {
                data.Currencies[(int)type] = _currencyModel.GetAmount(type).ToString();
            }
            saveData.Currency = data;
        }

        /// <inheritdoc />
        public void ApplyFrom(GameSaveData saveData)
        {
            var data = saveData.Currency;
            if (data == null) return;

            foreach (var pair in data.Currencies)
            {
                if (!Enum.IsDefined(typeof(CurrencyType), pair.Key)) continue;

                var type = (CurrencyType)pair.Key;
                var amount = BigNumber.Parse(pair.Value);
                _currencyModel.SetAmount(type, amount);
            }
        }

        /// <inheritdoc />
        public void SubscribeDirtyEvents(IMessageBrokerService broker, Action markDirty)
        {
            _markDirty = markDirty;
            broker.Subscribe<CurrencyChangedMessage>(OnCurrencyChanged);
        }

        /// <inheritdoc />
        public void UnsubscribeDirtyEvents(IMessageBrokerService broker)
        {
            broker.Unsubscribe<CurrencyChangedMessage>(this);
            _markDirty = null;
        }

        private void OnCurrencyChanged(CurrencyChangedMessage message) => _markDirty?.Invoke();
    }
}
