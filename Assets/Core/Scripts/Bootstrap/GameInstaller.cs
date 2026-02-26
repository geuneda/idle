using Geuneda.Services;
using IdleRPG.Stage;
using UnityEngine;

namespace IdleRPG.Core
{
    public class GameInstaller : MonoBehaviour
    {
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            InitializeServices();
        }

        private void InitializeServices()
        {
            var messageBroker = new MessageBrokerService();
            MainInstaller.Bind<IMessageBrokerService>(messageBroker);

            MainInstaller.Bind<ITickService>(new TickService());
            MainInstaller.Bind<IDataService>(new DataService());

            var timeService = new TimeService();
            MainInstaller.Bind<ITimeService>(timeService);
            MainInstaller.Bind<ITimeManipulator>(timeService);

            InitializeGameServices(messageBroker);
        }

        private void InitializeGameServices(IMessageBrokerService messageBroker)
        {
            var stageConfig = new StageConfig();
            var stageModel = new StageModel();
            var stageService = new StageService(stageConfig, stageModel, messageBroker);
            MainInstaller.Bind<IStageService>(stageService);
        }

        private void OnDestroy()
        {
            MainInstaller.Clean();
        }
    }
}
