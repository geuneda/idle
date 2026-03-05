using System;
using Geuneda.Services;
using IdleRPG.Battle;
using IdleRPG.Core;
using IdleRPG.Economy;

namespace IdleRPG.Dungeon
{
    /// <summary>
    /// <see cref="IDungeonService"/>의 구현체.
    /// 던전 진입/퇴장, 보상 지급, 소탕, 타이머, 일일 리셋을 관리한다.
    /// </summary>
    public class DungeonService : IDungeonService, IDisposable
    {
        private readonly DungeonConfigCollection _config;
        private readonly DungeonModel _model;
        private readonly IBattleService _battleService;
        private readonly ICurrencyService _currencyService;
        private readonly ITimeService _timeService;
        private readonly IMessageBrokerService _messageBroker;
        private readonly IBattleContext _stageBattleContext;

        private DungeonBattleContext _currentContext;

        /// <inheritdoc />
        public DungeonModel Model => _model;

        /// <inheritdoc />
        public DungeonConfigCollection Config => _config;

        /// <inheritdoc />
        public bool IsInDungeon => _currentContext != null;

        /// <inheritdoc />
        public float RemainingTime => _currentContext?.RemainingTime ?? 0f;

        /// <inheritdoc />
        public DungeonBattleContext CurrentContext => _currentContext;

        /// <summary>
        /// <see cref="DungeonService"/>를 생성한다.
        /// </summary>
        /// <param name="config">던전 설정 컬렉션</param>
        /// <param name="model">던전 진행 상태 모델</param>
        /// <param name="battleService">전투 서비스</param>
        /// <param name="currencyService">재화 서비스</param>
        /// <param name="timeService">시간 서비스</param>
        /// <param name="messageBroker">메시지 브로커</param>
        /// <param name="stageBattleContext">스테이지 전투 컨텍스트 (퇴장 시 복귀용)</param>
        public DungeonService(
            DungeonConfigCollection config,
            DungeonModel model,
            IBattleService battleService,
            ICurrencyService currencyService,
            ITimeService timeService,
            IMessageBrokerService messageBroker,
            IBattleContext stageBattleContext)
        {
            _config = config;
            _model = model;
            _battleService = battleService;
            _currencyService = currencyService;
            _timeService = timeService;
            _messageBroker = messageBroker;
            _stageBattleContext = stageBattleContext;

            _messageBroker.Subscribe<DungeonAllWavesClearedMessage>(OnAllWavesCleared);
            _messageBroker.Subscribe<DungeonHeroDiedMessage>(OnHeroDied);
            _messageBroker.Subscribe<DungeonTimeExpiredMessage>(OnTimeExpired);

            CheckAndResetDaily();
        }

        /// <summary>
        /// 메시지 구독을 해제한다.
        /// </summary>
        public void Dispose()
        {
            _messageBroker.UnsubscribeAll(this);
        }

        /// <inheritdoc />
        public bool CanEnter(DungeonType type, int level)
        {
            if (IsInDungeon) return false;

            var levelConfig = _config.GetConfig(type, level);
            if (levelConfig == null) return false;

            int availableMax = GetAvailableMaxLevel(type);
            if (level > availableMax) return false;

            return GetRemainingEntries(type) > 0;
        }

        /// <inheritdoc />
        public void Enter(DungeonType type, int level)
        {
            if (!CanEnter(type, level)) return;

            var levelConfig = _config.GetConfig(type, level);

            _currentContext = new DungeonBattleContext(
                levelConfig, type, level, _messageBroker);

            _battleService.SetBattleContext(_currentContext);

            _messageBroker.Publish(new DungeonEnteredMessage
            {
                Type = type,
                Level = level
            });
        }

        /// <inheritdoc />
        public void Complete(DungeonType type, int level)
        {
            var levelConfig = _config.GetConfig(type, level);
            if (levelConfig == null) return;

            var (rewardCurrency, rewardAmount) = GrantReward(type, levelConfig);

            if (level > _model.GetClearedLevel(type))
            {
                _model.SetClearedLevel(type, level);
            }

            _messageBroker.Publish(new DungeonCompletedMessage
            {
                Type = type,
                Level = level,
                RewardType = rewardCurrency,
                RewardAmount = rewardAmount
            });
        }

        /// <inheritdoc />
        public void Fail(DungeonType type, int level, DungeonFailReason reason)
        {
            _messageBroker.Publish(new DungeonFailedMessage
            {
                Type = type,
                Level = level,
                Reason = reason
            });
        }

        /// <inheritdoc />
        public bool CanSweep(DungeonType type, int level)
        {
            if (IsInDungeon) return false;

            var levelConfig = _config.GetConfig(type, level);
            if (levelConfig == null) return false;

            if (level > _model.GetClearedLevel(type)) return false;

            // TODO: 추후 멤버십 확인 로직 추가
            return GetRemainingEntries(type) > 0;
        }

        /// <inheritdoc />
        public bool Sweep(DungeonType type, int level)
        {
            if (!CanSweep(type, level)) return false;

            var levelConfig = _config.GetConfig(type, level);
            var (rewardCurrency, rewardAmount) = GrantReward(type, levelConfig);

            _messageBroker.Publish(new DungeonSweptMessage
            {
                Type = type,
                Level = level,
                RewardType = rewardCurrency,
                RewardAmount = rewardAmount
            });

            return true;
        }

        /// <inheritdoc />
        public void ExitDungeon()
        {
            if (!IsInDungeon) return;

            var type = _currentContext.DungeonType;

            _currentContext.StopTimer();
            _currentContext = null;

            _battleService.SetBattleContext(_stageBattleContext);

            _messageBroker.Publish(new DungeonExitedMessage
            {
                Type = type
            });
        }

        /// <inheritdoc />
        public int GetRemainingEntries(DungeonType type)
        {
            int max = GetMaxDailyEntries(type);
            int used = _model.GetUsedEntries(type);
            return Math.Max(0, max - used);
        }

        /// <inheritdoc />
        public int GetMaxDailyEntries(DungeonType type)
        {
            var firstLevel = _config.GetConfig(type, 1);
            return firstLevel?.DailyEntryLimit ?? 0;
        }

        /// <inheritdoc />
        public int GetAvailableMaxLevel(DungeonType type)
        {
            int cleared = _model.GetClearedLevel(type);
            int maxConfigLevel = _config.GetMaxLevel(type);
            return Math.Min(cleared + 1, maxConfigLevel);
        }

        /// <inheritdoc />
        public void CheckAndResetDaily()
        {
            var now = _timeService.DateTimeUtcNow;
            var todayMidnight = now.Date;
            long todayTimestamp = new DateTimeOffset(todayMidnight, TimeSpan.Zero).ToUnixTimeMilliseconds();

            if (_model.LastDailyResetTimestamp < todayTimestamp)
            {
                _model.ResetDailyEntries(todayTimestamp);
                _messageBroker.Publish(new DungeonDailyResetMessage());
            }
        }

        /// <summary>
        /// 보상을 지급하고 입장 횟수를 소모한다.
        /// </summary>
        /// <param name="type">던전 종류</param>
        /// <param name="levelConfig">레벨 설정</param>
        /// <returns>지급된 재화 종류와 수량</returns>
        private (CurrencyType currency, BigNumber amount) GrantReward(DungeonType type, DungeonLevelConfig levelConfig)
        {
            _model.IncrementUsedEntries(type);

            var rewardCurrency = DungeonConfigCollection.GetRewardCurrencyType(type);
            var rewardAmount = (BigNumber)levelConfig.RewardAmount;
            _currencyService.Add(rewardCurrency, rewardAmount);

            return (rewardCurrency, rewardAmount);
        }

        private void OnAllWavesCleared(DungeonAllWavesClearedMessage msg)
        {
            Complete(msg.Type, msg.Level);
        }

        private void OnHeroDied(DungeonHeroDiedMessage msg)
        {
            Fail(msg.Type, msg.Level, DungeonFailReason.HeroDied);
        }

        private void OnTimeExpired(DungeonTimeExpiredMessage msg)
        {
            _battleService.Model.IsBattleActive.Value = false;
            Fail(msg.Type, msg.Level, DungeonFailReason.TimeExpired);
        }
    }
}
