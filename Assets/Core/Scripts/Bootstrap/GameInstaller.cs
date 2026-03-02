using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Geuneda.DataExtensions;
using Geuneda.Services;
using Geuneda.UiService;
using IdleRPG.Battle;
using IdleRPG.Economy;
using IdleRPG.Equipment;
using IdleRPG.Growth;
using IdleRPG.Skill;
using IdleRPG.Hero;
using IdleRPG.OfflineReward;
using IdleRPG.Reward;
using IdleRPG.Stage;
using IdleRPG.UI;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace IdleRPG.Core
{
    /// <summary>
    /// 게임 진입점. 프레임워크 서비스와 게임 고유 서비스를 <see cref="MainInstaller"/>에 등록한다.
    /// </summary>
    /// <remarks>
    /// <para>씬에 배치되며 <c>DontDestroyOnLoad</c>로 앱 생명주기 동안 유지된다.</para>
    /// <para>초기화 흐름: Awake(프레임워크 서비스) → Loading 단계(Config 로드 + 게임 서비스) → InGame</para>
    /// </remarks>
    public class GameInstaller : MonoBehaviour
    {
        [SerializeField] private AddressablesUiConfigs _uiConfigs;

        private SaveService _saveService;
        private IUiServiceInit _uiService;
        private AppFlowStatechart _appFlowStatechart;
        private bool _initialUiOpened;
        private long _lastSaveTimestamp;

        private HeroConfigAsset _heroConfigAsset;
        private EnemyConfigsAsset _enemyConfigsAsset;
        private StageConfigAsset _stageConfigAsset;
        private GrowthConfigAsset _growthConfigAsset;
        private RewardConfigAsset _rewardConfigAsset;
        private OfflineRewardConfigAsset _offlineRewardConfigAsset;
        private EquipmentConfigAsset _equipmentConfigAsset;
        private SkillConfigAsset _skillConfigAsset;
        private readonly List<AsyncOperationHandle> _configHandles = new List<AsyncOperationHandle>();

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            InitializeFrameworkServices();
        }

        /// <summary>
        /// 프레임워크 서비스 초기화 후 앱 흐름 상태 머신을 시작한다.
        /// Loading 단계에서 Config 에셋 로드와 게임 서비스 초기화가 수행된다.
        /// </summary>
        private void Start()
        {
            StartAppFlow();
        }

        /// <summary>
        /// 앱 흐름 상태 머신을 구성하고 실행한다.
        /// Bootstrap -> Loading -> PostLoadChoice -> InGame 순서로 전이한다.
        /// </summary>
        private void StartAppFlow()
        {
            var messageBroker = MainInstaller.Resolve<IMessageBrokerService>();
            var uiService = MainInstaller.Resolve<IUiService>();

            var loadingSteps = CreateLoadingSteps();
            var loadingTaskRunner = new LoadingTaskRunner(messageBroker, loadingSteps);

            var callbacks = new AppFlowCallbacks
            {
                OnBootstrapEnter = () => DevLog.Log("[AppFlow] Bootstrap 시작"),
                OnLoadingEnter = () => DevLog.Log("[AppFlow] Loading 시작"),
                LoadingTask = async () =>
                {
                    await uiService.OpenUiAsync<LoadingPresenter>();

                    var loadingUi = uiService.GetUi<LoadingPresenter>();
                    if (loadingUi != null)
                    {
                        await loadingUi.OpenTransitionTask;
                    }

                    await loadingTaskRunner.RunAsync();
                },
                OnLoadingExit = () =>
                {
                    DevLog.Log("[AppFlow] Loading 완료");
                    uiService.CloseUi<LoadingPresenter>();
                },
                HasOfflineReward = () =>
                {
                    var offlineService = MainInstaller.Resolve<IOfflineRewardService>();
                    return offlineService.HasOfflineReward(_lastSaveTimestamp);
                },
                IsFirstPlay = () => false,
                OfflineRewardTask = async () =>
                {
                    var offlineService = MainInstaller.Resolve<IOfflineRewardService>();
                    var result = offlineService.CalculateReward(_lastSaveTimestamp);

                    var tcs = new UniTaskCompletionSource();

                    var popupData = new OfflineRewardPopupData
                    {
                        Result = result,
                        OnClaimed = (watchedAd) =>
                        {
                            var finalResult = watchedAd
                                ? result.WithMultiplier(offlineService.Config.AdMultiplier)
                                : result;
                            offlineService.ClaimReward(finalResult);
                            tcs.TrySetResult();
                        }
                    };

                    await uiService.OpenUiAsync<OfflineRewardPopupPresenter, OfflineRewardPopupData>(popupData);
                    await tcs.Task;
                },
                OnOfflineRewardExit = () =>
                {
                    DevLog.Log("[AppFlow] OfflineReward 완료");
                },
                OnInGameEnter = () =>
                {
                    DevLog.Log("[AppFlow] InGame 진입");
                    OpenInitialUiAndPublishReadyAsync(messageBroker).Forget();
                },
                OnInGameExit = () => DevLog.Log("[AppFlow] InGame 퇴장")
            };

            _appFlowStatechart = new AppFlowStatechart(callbacks);
            _appFlowStatechart.Run();
        }

        /// <summary>
        /// 로딩 단계 목록을 생성한다.
        /// Config 에셋 로드 → 게임 서비스 초기화 → 에셋 프리로드 순서로 실행된다.
        /// </summary>
        /// <returns>순차 실행할 로딩 단계 목록</returns>
        private List<LoadingStep> CreateLoadingSteps()
        {
            return new List<LoadingStep>
            {
                new LoadingStep("설정 데이터 로드", async () =>
                {
                    await LoadConfigAssetsAsync();
                    DevLog.Log("[Loading] Config 에셋 로드 완료");
                }),
                new LoadingStep("게임 초기화", () =>
                {
                    var configsProvider = InitializeConfigsProvider();
                    InitializeGameServices(configsProvider);
                    DevLog.Log("[Loading] 게임 서비스 초기화 완료");
                    return UniTask.CompletedTask;
                }),
                new LoadingStep("에셋 프리로드", async () =>
                {
                    await UniTask.Delay(100);
                    DevLog.Log("[Loading] 에셋 프리로드 완료");
                })
            };
        }

        /// <summary>
        /// Addressables를 통해 5개 Config ScriptableObject 에셋을 병렬 로딩한다.
        /// 로딩된 핸들은 <see cref="OnDestroy"/>에서 해제된다.
        /// </summary>
        private async UniTask LoadConfigAssetsAsync()
        {
            var heroOp = Addressables.LoadAssetAsync<HeroConfigAsset>("HeroConfigAsset");
            var enemyOp = Addressables.LoadAssetAsync<EnemyConfigsAsset>("EnemyConfigsAsset");
            var stageOp = Addressables.LoadAssetAsync<StageConfigAsset>("StageConfigAsset");
            var growthOp = Addressables.LoadAssetAsync<GrowthConfigAsset>("GrowthConfigAsset");
            var rewardOp = Addressables.LoadAssetAsync<RewardConfigAsset>("RewardConfigAsset");
            var offlineRewardOp = Addressables.LoadAssetAsync<OfflineRewardConfigAsset>("OfflineRewardConfigAsset");
            var equipmentOp = Addressables.LoadAssetAsync<EquipmentConfigAsset>("EquipmentConfigAsset");
            var skillOp = Addressables.LoadAssetAsync<SkillConfigAsset>("SkillConfigAsset");

            _configHandles.Add(heroOp);
            _configHandles.Add(enemyOp);
            _configHandles.Add(stageOp);
            _configHandles.Add(growthOp);
            _configHandles.Add(rewardOp);
            _configHandles.Add(offlineRewardOp);
            _configHandles.Add(equipmentOp);
            _configHandles.Add(skillOp);

            _heroConfigAsset = await heroOp.Task;
            _enemyConfigsAsset = await enemyOp.Task;
            _stageConfigAsset = await stageOp.Task;
            _growthConfigAsset = await growthOp.Task;
            _rewardConfigAsset = await rewardOp.Task;
            _offlineRewardConfigAsset = await offlineRewardOp.Task;
            _equipmentConfigAsset = await equipmentOp.Task;
            _skillConfigAsset = await skillOp.Task;
        }

        /// <summary>
        /// 로드된 Config 에셋에서 POCO Config를 꺼내 <see cref="ConfigsProvider"/>에 등록한다.
        /// </summary>
        /// <returns>Config가 등록된 ConfigsProvider</returns>
        private ConfigsProvider InitializeConfigsProvider()
        {
            var provider = new ConfigsProvider();

            provider.AddSingletonConfig(_heroConfigAsset.Config);
            provider.AddSingletonConfig(_stageConfigAsset.Config);
            provider.AddSingletonConfig(_growthConfigAsset.Config);
            provider.AddSingletonConfig(_rewardConfigAsset.Config);
            provider.AddSingletonConfig(_offlineRewardConfigAsset.Config);
            provider.AddSingletonConfig(_equipmentConfigAsset.Config);
            provider.AddSingletonConfig(_skillConfigAsset.Config);
            provider.AddConfigs(config => config.Id, _enemyConfigsAsset.Configs);

            return provider;
        }

        /// <summary>
        /// 초기 UI를 연 뒤 <see cref="AppFlowReadyMessage"/>를 발행한다.
        /// UI 오픈 완료 후 메시지를 발행하여 구독자가 준비된 UI에 접근할 수 있도록 보장한다.
        /// </summary>
        /// <param name="messageBroker">메시지 발행에 사용할 브로커</param>
        private async UniTaskVoid OpenInitialUiAndPublishReadyAsync(
            IMessageBrokerService messageBroker)
        {
            if (_initialUiOpened) return;
            _initialUiOpened = true;

            try
            {
                await OpenInitialUiAsync();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[AppFlow] 초기 UI 오픈 실패: {ex}");
            }

            messageBroker.Publish(new AppFlowReadyMessage());
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
        /// 게임 고유 서비스는 Loading 단계에서 초기화된다.
        /// </summary>
        private void InitializeFrameworkServices()
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
        }

        /// <summary>
        /// <see cref="IUiService"/>를 생성하고 <see cref="MainInstaller"/>에 바인딩한다.
        /// </summary>
        private void InitializeUiService()
        {
            var loader = new AddressablesUiAssetLoader();
            _uiService = new Geuneda.UiService.UiService(loader);
            _uiService.Init(_uiConfigs);
            MainInstaller.Bind<IUiService>(_uiService);
        }

        /// <summary>
        /// 게임 고유 서비스를 생성하고 <see cref="MainInstaller"/>에 바인딩한다.
        /// <see cref="ConfigsProvider"/>에서 Config를 조회하여 서비스에 주입한다.
        /// </summary>
        /// <param name="configsProvider">Config가 등록된 프로바이더</param>
        private void InitializeGameServices(ConfigsProvider configsProvider)
        {
            MainInstaller.Bind<IConfigsProvider>(configsProvider);

            var messageBroker = MainInstaller.Resolve<IMessageBrokerService>();
            var tickService = MainInstaller.Resolve<ITickService>();
            var coroutineService = MainInstaller.Resolve<ICoroutineService>();
            var dataService = MainInstaller.Resolve<IDataService>();
            var timeService = MainInstaller.Resolve<ITimeService>();

            var currencyModel = new CurrencyModel();
            var stageModel = new StageModel();
            var growthModel = new HeroGrowthModel();
            var equipmentModel = new EquipmentModel();
            var equipmentConfig = configsProvider.GetConfig<EquipmentConfigCollection>();
            var skillModel = new SkillModel();

            _saveService = new SaveService(
                stageModel, currencyModel, growthModel,
                equipmentModel, equipmentConfig, skillModel,
                dataService, tickService, coroutineService, messageBroker, timeService);

            var saveData = _saveService.Load();
            _lastSaveTimestamp = saveData.LastSaveTimestamp;
            _saveService.ApplyLoadedData(saveData);

            MainInstaller.Bind<ISaveService>(_saveService);

            var currencyService = new CurrencyService(currencyModel, messageBroker);
            MainInstaller.Bind<ICurrencyService>(currencyService);

            var stageConfig = configsProvider.GetConfig<StageConfig>();
            var stageService = new StageService(stageConfig, stageModel, messageBroker);
            MainInstaller.Bind<IStageService>(stageService);

            var heroConfig = configsProvider.GetConfig<HeroConfig>();
            var heroModel = new HeroModel(heroConfig);

            var growthConfig = configsProvider.GetConfig<GrowthConfig>();
            var growthService = new HeroGrowthService(
                growthConfig, growthModel, heroModel, currencyService, messageBroker);
            MainInstaller.Bind<IHeroGrowthService>(growthService);

            var equipmentService = new EquipmentService(
                equipmentConfig, equipmentModel, heroModel,
                growthService, messageBroker);
            MainInstaller.Bind<IEquipmentService>(equipmentService);

            var skillConfig = configsProvider.GetConfig<SkillConfigCollection>();
            var skillService = new SkillService(
                skillConfig, skillModel, stageService, messageBroker);
            MainInstaller.Bind<ISkillService>(skillService);

            equipmentService.SetSkillBonusProvider(skillService);

            var skillExecutionService = new SkillExecutionService(skillService, messageBroker);
            MainInstaller.Bind<ISkillExecutionService>(skillExecutionService);

            var normalEnemy = configsProvider.GetConfig<EnemyConfig>(1);
            var bossEnemy = configsProvider.GetConfig<EnemyConfig>(100);

            var battleService = new BattleService(
                stageService, stageConfig, heroModel,
                normalEnemy, bossEnemy, messageBroker);
            MainInstaller.Bind<IBattleService>(battleService);

            var rewardConfig = configsProvider.GetConfig<RewardConfig>();
            var rewardService = new RewardService(
                rewardConfig, currencyService, stageService, stageConfig, messageBroker);
            MainInstaller.Bind<IRewardService>(rewardService);

            var offlineRewardConfig = configsProvider.GetConfig<OfflineRewardConfig>();
            var offlineRewardService = new OfflineRewardService(
                offlineRewardConfig, rewardConfig, stageConfig,
                stageService, currencyService, timeService, messageBroker);
            MainInstaller.Bind<IOfflineRewardService>(offlineRewardService);

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
            foreach (var handle in _configHandles)
            {
                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                }
            }

            _configHandles.Clear();
            _uiService?.Dispose();
            _saveService?.Dispose();
            MainInstaller.Clean();
        }
    }
}
