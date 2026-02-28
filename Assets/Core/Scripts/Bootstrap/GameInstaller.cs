using Geuneda.Services;
using IdleRPG.Battle;
using IdleRPG.Economy;
using IdleRPG.Hero;
using IdleRPG.Reward;
using IdleRPG.Stage;
using UnityEngine;

namespace IdleRPG.Core
{
    /// <summary>
    /// 게임 진입점. 프레임워크 서비스와 게임 고유 서비스를 <see cref="MainInstaller"/>에 등록한다.
    /// </summary>
    /// <remarks>
    /// <para>씬에 배치되며 <c>DontDestroyOnLoad</c>로 앱 생명주기 동안 유지된다.</para>
    /// <para>서비스 초기화 순서: 프레임워크 → 게임 고유 서비스 (Economy → Stage → Hero → Battle → Reward)</para>
    /// </remarks>
    public class GameInstaller : MonoBehaviour
    {
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            InitializeServices();
        }

        /// <summary>
        /// 프레임워크 핵심 서비스를 <see cref="MainInstaller"/>에 바인딩한다.
        /// </summary>
        private void InitializeServices()
        {
            var messageBroker = new MessageBrokerService();
            MainInstaller.Bind<IMessageBrokerService>(messageBroker);

            MainInstaller.Bind<ITickService>(new TickService());
            MainInstaller.Bind<ICoroutineService>(new CoroutineService());
            MainInstaller.Bind<IPoolService>(new PoolService());
            MainInstaller.Bind<IDataService>(new DataService());

            var timeService = new TimeService();
            MainInstaller.Bind<ITimeService>(timeService);
            MainInstaller.Bind<ITimeManipulator>(timeService);

            InitializeGameServices(messageBroker);
        }

        /// <summary>
        /// 게임 고유 서비스(Economy, Stage, Hero, Battle)를 생성하고 <see cref="MainInstaller"/>에 바인딩한다.
        /// </summary>
        /// <param name="messageBroker">이벤트 통신용 메시지 브로커</param>
        private void InitializeGameServices(IMessageBrokerService messageBroker)
        {
            var currencyModel = new CurrencyModel();
            var currencyService = new CurrencyService(currencyModel, messageBroker);
            MainInstaller.Bind<ICurrencyService>(currencyService);

            var stageConfig = new StageConfig();
            var stageModel = new StageModel();
            var stageService = new StageService(stageConfig, stageModel, messageBroker);
            MainInstaller.Bind<IStageService>(stageService);

            var heroConfig = new HeroConfig();
            var heroModel = new HeroModel(heroConfig);

            var normalEnemy = new EnemyConfig
            {
                Id = 1,
                BaseHp = 30,
                BaseAttack = 5,
                MoveSpeed = 2f,
                AttackRange = 1.2f,
                AttackSpeed = 1f,
                IsBoss = false
            };

            var bossEnemy = new EnemyConfig
            {
                Id = 100,
                BaseHp = 200,
                BaseAttack = 15,
                MoveSpeed = 1.5f,
                AttackRange = 1.5f,
                AttackSpeed = 0.8f,
                IsBoss = true
            };

            var battleService = new BattleService(
                stageService, stageConfig, heroModel,
                normalEnemy, bossEnemy, messageBroker);
            MainInstaller.Bind<IBattleService>(battleService);

            var rewardConfig = new RewardConfig();
            var rewardService = new RewardService(
                rewardConfig, currencyService, stageService, stageConfig, messageBroker);
            MainInstaller.Bind<IRewardService>(rewardService);
        }

        private void OnDestroy()
        {
            MainInstaller.Clean();
        }
    }
}
