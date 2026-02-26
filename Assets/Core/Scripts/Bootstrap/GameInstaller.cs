using Geuneda.Services;
using IdleRPG.Stage;
using UnityEngine;

namespace IdleRPG.Core
{
    /// <summary>
    /// 게임 진입점. 프레임워크 서비스와 게임 고유 서비스를 <see cref="MainInstaller"/>에 등록한다.
    /// </summary>
    /// <remarks>
    /// <para>씬의 루트 오브젝트에 부착하여 사용한다. Awake에서 DontDestroyOnLoad를 설정한다.</para>
    /// <para>서비스 등록 순서는 의존성 방향을 따른다:
    /// MessageBroker → Tick → Data → Time → 게임 서비스</para>
    /// </remarks>
    public class GameInstaller : MonoBehaviour
    {
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            InitializeServices();
        }

        /// <summary>
        /// 프레임워크 서비스를 생성하고 <see cref="MainInstaller"/>에 바인딩한다.
        /// </summary>
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

        /// <summary>
        /// 게임 고유 서비스를 생성하고 <see cref="MainInstaller"/>에 바인딩한다.
        /// </summary>
        /// <param name="messageBroker">이벤트 발행에 사용할 메시지 브로커 인스턴스</param>
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
