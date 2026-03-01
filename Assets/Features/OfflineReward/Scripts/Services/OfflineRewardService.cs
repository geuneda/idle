using System;
using Geuneda.Services;
using IdleRPG.Core;
using IdleRPG.Economy;
using IdleRPG.Reward;
using IdleRPG.Stage;

namespace IdleRPG.OfflineReward
{
    /// <summary>
    /// <see cref="IOfflineRewardService"/>의 구현체.
    /// 오프라인 경과 시간을 기반으로 골드와 아이템 드롭을 계산하고 지급한다.
    /// </summary>
    public class OfflineRewardService : IOfflineRewardService
    {
        private readonly OfflineRewardConfig _config;
        private readonly RewardConfig _rewardConfig;
        private readonly StageConfig _stageConfig;
        private readonly IStageService _stageService;
        private readonly ICurrencyService _currencyService;
        private readonly ITimeService _timeService;
        private readonly IMessageBrokerService _messageBroker;

        /// <inheritdoc />
        public OfflineRewardConfig Config => _config;

        /// <summary>
        /// <see cref="OfflineRewardService"/>를 생성한다.
        /// </summary>
        /// <param name="config">오프라인 보상 설정</param>
        /// <param name="rewardConfig">적 처치 보상 설정</param>
        /// <param name="stageConfig">스테이지 설정</param>
        /// <param name="stageService">스테이지 서비스</param>
        /// <param name="currencyService">재화 서비스</param>
        /// <param name="timeService">시간 서비스</param>
        /// <param name="messageBroker">메시지 브로커</param>
        public OfflineRewardService(
            OfflineRewardConfig config,
            RewardConfig rewardConfig,
            StageConfig stageConfig,
            IStageService stageService,
            ICurrencyService currencyService,
            ITimeService timeService,
            IMessageBrokerService messageBroker)
        {
            _config = config;
            _rewardConfig = rewardConfig;
            _stageConfig = stageConfig;
            _stageService = stageService;
            _currencyService = currencyService;
            _timeService = timeService;
            _messageBroker = messageBroker;
        }

        /// <inheritdoc />
        public bool HasOfflineReward(long lastSaveTimestamp)
        {
            if (lastSaveTimestamp <= 0) return false;

            long currentTimestamp = _timeService.UnixTimeNow;
            double elapsedMinutes = (currentTimestamp - lastSaveTimestamp) / 60000.0;
            return elapsedMinutes >= _config.MinOfflineMinutes;
        }

        /// <inheritdoc />
        public OfflineRewardResult CalculateReward(long lastSaveTimestamp)
        {
            long currentTimestamp = _timeService.UnixTimeNow;
            double elapsedMinutes = (currentTimestamp - lastSaveTimestamp) / 60000.0;
            double maxMinutes = _config.DefaultMaxOfflineHours * 60.0;
            double clampedMinutes = Math.Min(Math.Max(elapsedMinutes, 0), maxMinutes);

            int chapter = _stageService.Model.CurrentChapter.Value;
            int stage = _stageService.Model.CurrentStage.Value;

            BigNumber gold = _config.CalculateOfflineGold(
                clampedMinutes, _rewardConfig, chapter, stage,
                _stageConfig.StagesPerChapter);

            var random = new System.Random(unchecked((int)lastSaveTimestamp));
            var drops = _config.CalculateDrops(
                clampedMinutes, chapter, stage,
                _stageConfig.StagesPerChapter, random);

            return new OfflineRewardResult(elapsedMinutes, clampedMinutes, gold, drops);
        }

        /// <inheritdoc />
        public void ClaimReward(OfflineRewardResult result)
        {
            if (result.Gold > BigNumber.Zero)
            {
                _currencyService.Add(CurrencyType.Gold, result.Gold);
            }

            // TODO: 펫/장비/스킬 시스템 구현 후 Drops를 실제 인벤토리에 추가하는 로직 필요
            // 현재는 메시지 발행만 수행하며, 각 시스템에서 OfflineRewardClaimedMessage를 구독하여 처리할 것
            _messageBroker.Publish(new OfflineRewardClaimedMessage
            {
                Gold = result.Gold,
                Drops = result.Drops,
                IsDoubled = result.IsDoubled,
                ElapsedMinutes = result.ClampedMinutes
            });

            DevLog.Log($"[OfflineReward] 보상 지급 완료: 골드={result.Gold.ToFormattedString()}, " +
                       $"아이템={result.Drops.Count}종, 2배={result.IsDoubled}");
        }
    }
}
