using Cysharp.Threading.Tasks;
using Geuneda.Services;
using Geuneda.UiService;
using IdleRPG.Battle;
using IdleRPG.Economy;
using IdleRPG.Growth;
using IdleRPG.Hero;
using IdleRPG.Reward;
using IdleRPG.Stage;
using IdleRPG.UI;
using UnityEngine;

namespace IdleRPG.Core
{
    /// <summary>
    /// 게임 진입점. 프레임워크 서비스와 게임 고유 서비스를 <see cref="MainInstaller"/>에 등록한다.
    /// </summary>
    /// <remarks>
    /// <para>씬에 배치되며 <c>DontDestroyOnLoad</c>로 앱 생명주기 동안 유지된다.</para>
    /// <para>서비스 초기화 순서: 프레임워크 → 저장 데이터 로드 → 모델 복원 → 게임 서비스 → 자동 저장 시작</para>
    /// </remarks>
    public class GameInstaller : MonoBehaviour
    {
        [SerializeField] private PrefabRegistryUiConfigs _uiConfigs;

        private SaveService _saveService;
        private IUiServiceInit _uiService;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            InitializeServices();
        }

        /// <summary>
        /// 모든 서비스 초기화 후 초기 UI를 연다.
        /// <c>Awake</c>에서 서비스 바인딩이 완료된 뒤 <c>Start</c>에서 실행되므로
        /// 프레젠터가 <see cref="MainInstaller.Resolve{T}"/>를 안전하게 호출할 수 있다.
        /// </summary>
        private async void Start()
        {
            await OpenInitialUiAsync();
        }

        /// <summary>
        /// 항상 표시되는 HUD(상단/하단)와 기본 전투 UI를 연다.
        /// </summary>
        private async UniTask OpenInitialUiAsync()
        {
            var uiService = MainInstaller.Resolve<IUiService>();

            await uiService.OpenUiAsync<TopHudPresenter>();
            await uiService.OpenUiAsync<BottomTabBarPresenter>();
            await uiService.OpenUiAsync<MainBattleUiPresenter>();
        }

        /// <summary>
        /// 프레임워크 핵심 서비스를 <see cref="MainInstaller"/>에 바인딩한다.
        /// </summary>
        private void InitializeServices()
        {
            var messageBroker = new MessageBrokerService();
            MainInstaller.Bind<IMessageBrokerService>(messageBroker);

            var tickService = new TickService();
            MainInstaller.Bind<ITickService>(tickService);

            var coroutineService = new CoroutineService();
            MainInstaller.Bind<ICoroutineService>(coroutineService);

            MainInstaller.Bind<IPoolService>(new PoolService());

            var dataService = new DataService();
            MainInstaller.Bind<IDataService>(dataService);

            var timeService = new TimeService();
            MainInstaller.Bind<ITimeService>(timeService);
            MainInstaller.Bind<ITimeManipulator>(timeService);

            InitializeUiService();

            InitializeGameServices(
                messageBroker, tickService, coroutineService, dataService, timeService);
        }

        /// <summary>
        /// <see cref="IUiService"/>를 생성하고 <see cref="MainInstaller"/>에 바인딩한다.
        /// </summary>
        private void InitializeUiService()
        {
            var loader = new PrefabRegistryUiAssetLoader(_uiConfigs);
            _uiService = new Geuneda.UiService.UiService(loader);
            _uiService.Init(_uiConfigs);
            MainInstaller.Bind<IUiService>(_uiService);
        }

        /// <summary>
        /// 게임 고유 서비스를 생성하고 <see cref="MainInstaller"/>에 바인딩한다.
        /// 저장 데이터가 있으면 로드하여 모델에 적용한 뒤 서비스를 초기화한다.
        /// </summary>
        private void InitializeGameServices(
            IMessageBrokerService messageBroker,
            ITickService tickService,
            ICoroutineService coroutineService,
            IDataService dataService,
            ITimeService timeService)
        {
            var currencyModel = new CurrencyModel();
            var stageModel = new StageModel();
            var growthModel = new HeroGrowthModel();

            _saveService = new SaveService(
                stageModel, currencyModel, growthModel,
                dataService, tickService, coroutineService, messageBroker, timeService);

            var saveData = _saveService.Load();
            _saveService.ApplyLoadedData(saveData);

            MainInstaller.Bind<ISaveService>(_saveService);

            var currencyService = new CurrencyService(currencyModel, messageBroker);
            MainInstaller.Bind<ICurrencyService>(currencyService);

            var stageConfig = new StageConfig();
            var stageService = new StageService(stageConfig, stageModel, messageBroker);
            MainInstaller.Bind<IStageService>(stageService);

            var heroConfig = new HeroConfig();
            var heroModel = new HeroModel(heroConfig);

            var growthConfig = new GrowthConfig();
            var growthService = new HeroGrowthService(
                growthConfig, growthModel, heroModel, currencyService, messageBroker);
            MainInstaller.Bind<IHeroGrowthService>(growthService);

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

            _saveService.StartAutoSave();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                _saveService?.SaveIfDirty();
            }
        }

        private void OnApplicationQuit()
        {
            _saveService?.SaveIfDirty();
        }

        private void OnDestroy()
        {
            _uiService?.Dispose();
            _saveService?.Dispose();
            MainInstaller.Clean();
        }
    }
}
