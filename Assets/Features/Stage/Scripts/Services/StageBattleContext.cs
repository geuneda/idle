using Geuneda.Services;
using IdleRPG.Core;

namespace IdleRPG.Stage
{
    /// <summary>
    /// 스테이지 전투용 <see cref="IBattleContext"/> 구현.
    /// 기존 <see cref="IStageService"/>의 웨이브 진행 로직을 래핑한다.
    /// </summary>
    public class StageBattleContext : IBattleContext
    {
        private readonly IStageService _stageService;
        private readonly StageConfig _stageConfig;
        private readonly IMessageBrokerService _messageBroker;

        /// <summary>
        /// <see cref="StageBattleContext"/>를 생성한다.
        /// </summary>
        /// <param name="stageService">스테이지 진행 서비스</param>
        /// <param name="stageConfig">스테이지 구조 설정</param>
        /// <param name="messageBroker">이벤트 발행용 메시지 브로커</param>
        public StageBattleContext(
            IStageService stageService,
            StageConfig stageConfig,
            IMessageBrokerService messageBroker)
        {
            _stageService = stageService;
            _stageConfig = stageConfig;
            _messageBroker = messageBroker;
        }

        /// <inheritdoc />
        public bool ShouldAutoRestart => true;

        /// <inheritdoc />
        public int GetWaveEnemyCount()
        {
            return _stageConfig.GetEnemyCount(_stageService.Model.CurrentWave.Value);
        }

        /// <inheritdoc />
        public BigNumber GetHpMultiplier()
        {
            var m = _stageService.Model;
            return _stageConfig.GetHpMultiplier(m.CurrentChapter.Value, m.CurrentStage.Value);
        }

        /// <inheritdoc />
        public BigNumber GetAttackMultiplier()
        {
            var m = _stageService.Model;
            return _stageConfig.GetAttackMultiplier(m.CurrentChapter.Value, m.CurrentStage.Value);
        }

        /// <inheritdoc />
        public bool IsBossWave()
        {
            return _stageService.IsBossWave();
        }

        /// <inheritdoc />
        public void OnWaveStarted()
        {
            _stageService.StartWave();
        }

        /// <inheritdoc />
        public bool OnAllEnemiesCleared()
        {
            var stageModel = _stageService.Model;
            _messageBroker.Publish(new AllEnemiesClearedMessage
            {
                Chapter = stageModel.CurrentChapter.Value,
                Stage = stageModel.CurrentStage.Value,
                Wave = stageModel.CurrentWave.Value
            });

            _stageService.CompleteWave();
            return true;
        }

        /// <inheritdoc />
        public void OnHeroDied()
        {
            _stageService.FailWave();
        }

        /// <inheritdoc />
        public void Tick(float deltaTime) { }
    }
}
