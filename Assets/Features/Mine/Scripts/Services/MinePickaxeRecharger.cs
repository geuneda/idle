using System;
using Geuneda.Services;
using IdleRPG.Core;
using IdleRPG.Economy;

namespace IdleRPG.Mine
{
    /// <summary>
    /// 곡괭이 자동 충전 로직을 담당한다.
    /// </summary>
    internal class MinePickaxeRecharger : IDisposable
    {
        private readonly MineModel _model;
        private readonly int _maxPickaxeCount;
        private readonly long _rechargeIntervalMs;
        private readonly ICurrencyService _currencyService;
        private readonly ITimeService _timeService;
        private readonly ITickService _tickService;
        private readonly IMessageBrokerService _messageBroker;

        private long _nextRechargeTimestamp;
        private Action<float> _tickCallback;

        /// <summary>
        /// <see cref="MinePickaxeRecharger"/>를 생성한다.
        /// </summary>
        internal MinePickaxeRecharger(
            MineModel model,
            MineSettingsConfig settings,
            ICurrencyService currencyService,
            ITimeService timeService,
            ITickService tickService,
            IMessageBrokerService messageBroker)
        {
            _model = model;
            _maxPickaxeCount = settings.MaxPickaxe;
            _rechargeIntervalMs = settings.PickaxeRechargeSeconds * 1000L;
            _currencyService = currencyService;
            _timeService = timeService;
            _tickService = tickService;
            _messageBroker = messageBroker;

            CalculateOfflineRecharge();

            _tickCallback = OnTick;
            _tickService.SubscribeOnUpdate(_tickCallback, 1f);
        }

        /// <summary>다음 충전까지 남은 시간</summary>
        internal TimeSpan NextRechargeTime
        {
            get
            {
                if (IsPickaxeMaxed) return TimeSpan.Zero;
                long now = _timeService.UnixTimeNow;
                long remaining = _nextRechargeTimestamp - now;
                return remaining > 0 ? TimeSpan.FromMilliseconds(remaining) : TimeSpan.Zero;
            }
        }

        private int CurrentPickaxeCount =>
            (int)_currencyService.GetAmount(CurrencyType.Pickaxe).ToDouble();

        private bool IsPickaxeMaxed => CurrentPickaxeCount >= _maxPickaxeCount;

        /// <summary>
        /// 리소스를 해제한다.
        /// </summary>
        public void Dispose()
        {
            if (_tickCallback != null)
            {
                _tickService.Unsubscribe(_tickCallback);
                _tickCallback = null;
            }
        }

        private void OnTick(float deltaTime) => UpdatePickaxeRecharge();

        private void UpdatePickaxeRecharge()
        {
            if (IsPickaxeMaxed)
            {
                _model.SetLastPickaxeRechargeTimestamp(_timeService.UnixTimeNow);
                return;
            }

            long now = _timeService.UnixTimeNow;
            long elapsed = now - _model.LastPickaxeRechargeTimestamp;

            if (elapsed >= _rechargeIntervalMs)
            {
                int recharged = (int)(elapsed / _rechargeIntervalMs);
                int currentCount = CurrentPickaxeCount;
                int toAdd = Math.Min(recharged, _maxPickaxeCount - currentCount);

                if (toAdd > 0)
                {
                    _currencyService.Add(CurrencyType.Pickaxe, new BigNumber(toAdd, 0));
                    _messageBroker.Publish(new MinePickaxeRechargedMessage
                    {
                        NewCount = CurrentPickaxeCount,
                        MaxCount = _maxPickaxeCount
                    });
                }

                _model.SetLastPickaxeRechargeTimestamp(
                    _model.LastPickaxeRechargeTimestamp + recharged * _rechargeIntervalMs);
            }

            _nextRechargeTimestamp = _model.LastPickaxeRechargeTimestamp + _rechargeIntervalMs;
        }

        private void CalculateOfflineRecharge()
        {
            if (_model.LastPickaxeRechargeTimestamp <= 0)
            {
                _model.SetLastPickaxeRechargeTimestamp(_timeService.UnixTimeNow);
                return;
            }

            UpdatePickaxeRecharge();
        }
    }
}
