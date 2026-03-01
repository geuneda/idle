using Geuneda.StatechartMachine;

namespace IdleRPG.Core
{
    /// <summary>
    /// 앱 생명주기 흐름을 관리하는 Statechart.
    /// Bootstrap -> Loading -> PostLoadChoice -> InGame 상태를 순차 전이한다.
    /// </summary>
    /// <remarks>
    /// <para>POCO 클래스로, <see cref="GameInstaller"/>에서 생성하여 <see cref="Run"/>으로 실행한다.</para>
    /// <para>각 상태의 구체적 로직은 <see cref="AppFlowCallbacks"/>를 통해 외부에서 주입된다.</para>
    /// </remarks>
    public class AppFlowStatechart
    {
        private readonly IStatechart _statechart;

        /// <summary>
        /// <see cref="AppFlowStatechart"/>를 생성하고 상태를 구성한다.
        /// </summary>
        /// <param name="callbacks">각 상태에서 실행할 콜백</param>
        public AppFlowStatechart(AppFlowCallbacks callbacks)
        {
            _statechart = new Statechart(factory => ConfigureStates(factory, callbacks));
        }

        /// <summary>
        /// Statechart 실행을 시작한다. Initial 상태부터 자동 전이를 진행한다.
        /// </summary>
        public void Run()
        {
            _statechart.Run();
        }

        /// <summary>
        /// 이벤트를 트리거하여 상태 전이를 유도한다.
        /// </summary>
        /// <param name="statechartEvent">트리거할 이벤트</param>
        public void Trigger(IStatechartEvent statechartEvent)
        {
            _statechart.Trigger(statechartEvent);
        }

        private static void ConfigureStates(IStateFactory factory, AppFlowCallbacks callbacks)
        {
            var initial = factory.Initial("Initial");
            var bootstrap = factory.Transition("Bootstrap");
            var loading = factory.TaskWait("Loading");
            var postLoadChoice = factory.Choice("PostLoadChoice");
            var offlineReward = factory.TaskWait("OfflineReward");
            var inGame = factory.State("InGame");

            // Initial -> Bootstrap (자동 전이)
            initial.Transition().Target(bootstrap);

            // Bootstrap: 비차단 통과 상태
            bootstrap.OnEnter(() => callbacks.OnBootstrapEnter?.Invoke());
            bootstrap.Transition().Target(loading);

            // Loading: 비동기 작업 대기
            loading.OnEnter(() => callbacks.OnLoadingEnter?.Invoke());
            loading.WaitingFor(callbacks.LoadingTask).Target(postLoadChoice);
            loading.OnExit(() => callbacks.OnLoadingExit?.Invoke());

            // PostLoadChoice: 조건 분기 (오프라인 보상 / 튜토리얼 / 기본)
            postLoadChoice.Transition()
                .Condition(callbacks.HasOfflineReward)
                .Target(offlineReward);
            postLoadChoice.Transition()
                .Condition(callbacks.IsFirstPlay)
                .Target(inGame); // 추후 Tutorial 상태로 변경
            postLoadChoice.Transition()
                .Target(inGame); // 기본 경로

            // OfflineReward: 팝업 표시 후 수령 대기
            offlineReward.WaitingFor(callbacks.OfflineRewardTask).Target(inGame);
            offlineReward.OnExit(() => callbacks.OnOfflineRewardExit?.Invoke());

            // InGame: 게임 루프 진행 (이벤트 대기 상태)
            inGame.OnEnter(() => callbacks.OnInGameEnter?.Invoke());
            inGame.OnExit(() => callbacks.OnInGameExit?.Invoke());
        }
    }
}
