using System;
using System.Collections.Generic;
using Geuneda.DataExtensions;
using IdleRPG.Core;

namespace IdleRPG.Economy
{
    /// <summary>
    /// 재화 보유량을 관리하는 POCO 데이터 모델.
    /// <see cref="ObservableDictionary{TKey,TValue}"/>를 사용하여 UI 바인딩을 지원한다.
    /// </summary>
    [Serializable]
    public class CurrencyModel
    {
        private readonly ObservableDictionary<CurrencyType, BigNumber> _currencies;

        /// <summary>
        /// 재화 보유량의 읽기 전용 뷰. UI 바인딩에 사용한다.
        /// </summary>
        public IObservableDictionaryReader<CurrencyType, BigNumber> Currencies => _currencies;

        /// <summary>
        /// 모든 <see cref="CurrencyType"/>을 <see cref="BigNumber.Zero"/>로 초기화한다.
        /// </summary>
        public CurrencyModel()
        {
            var initial = new Dictionary<CurrencyType, BigNumber>
            {
                { CurrencyType.Gold, BigNumber.Zero },
                { CurrencyType.Gem, BigNumber.Zero }
            };
            _currencies = new ObservableDictionary<CurrencyType, BigNumber>(initial);
        }

        /// <summary>
        /// 지정한 재화를 추가한다. 0 이하의 값은 무시된다.
        /// </summary>
        /// <param name="type">재화 종류</param>
        /// <param name="amount">추가할 양 (양수)</param>
        /// <returns>추가 후 보유량</returns>
        public BigNumber Add(CurrencyType type, BigNumber amount)
        {
            if (amount <= BigNumber.Zero) return GetAmount(type);

            var current = GetAmount(type);
            var next = current + amount;
            _currencies[type] = next;
            return next;
        }

        /// <summary>
        /// 지정한 재화를 소비한다. 잔액 부족 시 <see cref="InvalidOperationException"/>을 발생시킨다.
        /// </summary>
        /// <param name="type">재화 종류</param>
        /// <param name="amount">소비할 양 (양수)</param>
        /// <returns>소비 후 잔액</returns>
        /// <exception cref="InvalidOperationException">잔액이 부족한 경우</exception>
        public BigNumber Spend(CurrencyType type, BigNumber amount)
        {
            if (amount <= BigNumber.Zero) return GetAmount(type);

            var current = GetAmount(type);
            if (current < amount)
                throw new InvalidOperationException(
                    $"{type} 잔액 부족: 보유 {current.ToFormattedString()}, 필요 {amount.ToFormattedString()}");

            var next = current - amount;
            _currencies[type] = next;
            return next;
        }

        /// <summary>
        /// 지정한 재화의 현재 보유량을 반환한다.
        /// </summary>
        /// <param name="type">재화 종류</param>
        /// <returns>현재 보유량</returns>
        public BigNumber GetAmount(CurrencyType type)
        {
            return _currencies.TryGetValue(type, out var value) ? value : BigNumber.Zero;
        }

        /// <summary>
        /// 지정한 재화가 충분한지 확인한다.
        /// </summary>
        /// <param name="type">재화 종류</param>
        /// <param name="amount">필요한 양</param>
        /// <returns>보유량이 필요량 이상이면 true</returns>
        public bool HasEnough(CurrencyType type, BigNumber amount)
        {
            return GetAmount(type) >= amount;
        }
    }
}
