using Geuneda.Services;
using IdleRPG.Core;
using IdleRPG.Economy;
using IdleRPG.Stage;

namespace IdleRPG.Reward
{
    /// <summary>
    /// <see cref="IRewardService"/>의 구현체.
    /// <see cref="EnemyDiedMessage"/>를 구독하여 적 처치 시 골드를 자동으로 지급한다.
    /// </summary>
    public class RewardService : IRewardService
    {
        private readonly RewardConfig _config;
        private readonly ICurrencyService _currencyService;
        private readonly IStageService _stageService;
        private readonly StageConfig _stageConfig;
        private readonly IMessageBrokerService _messageBroker;

        /// <inheritdoc />
        public RewardConfig Config => _config;

        /// <summary>
        /// <see cref="RewardService"/>를 생성하고 적 사망 이벤트를 구독한다.
        /// </summary>
        /// <param name="config">보상 설정 데이터</param>
        /// <param name="currencyService">재화 지급용 서비스</param>
        /// <param name="stageService">현재 스테이지 정보 조회용 서비스</param>
        /// <param name="stageConfig">스테이지 구조 설정 (챕터당 스테이지 수)</param>
        /// <param name="messageBroker">이벤트 구독/발행용 메시지 브로커</param>
        public RewardService(
            RewardConfig config,
            ICurrencyService currencyService,
            IStageService stageService,
            StageConfig stageConfig,
            IMessageBrokerService messageBroker)
        {
            _config = config;
            _currencyService = currencyService;
            _stageService = stageService;
            _stageConfig = stageConfig;
            _messageBroker = messageBroker;

            _messageBroker.Subscribe<EnemyDiedMessage>(OnEnemyDied);
        }

        /// <summary>
        /// 적 사망 시 골드 보상을 계산하여 지급한다.
        /// </summary>
        /// <param name="msg">적 사망 메시지</param>
        private void OnEnemyDied(EnemyDiedMessage msg)
        {
            int chapter = _stageService.Model.CurrentChapter.Value;
            int stage = _stageService.Model.CurrentStage.Value;

            BigNumber gold = _config.CalculateGoldReward(
                chapter, stage, _stageConfig.StagesPerChapter, msg.IsBoss);

            _currencyService.Add(CurrencyType.Gold, gold);

            _messageBroker.Publish(new RewardGrantedMessage
            {
                Type = CurrencyType.Gold,
                Amount = gold,
                IsBossReward = msg.IsBoss
            });
        }
    }
}
