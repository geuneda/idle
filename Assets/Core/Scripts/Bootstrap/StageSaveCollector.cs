using System;
using Geuneda.Services;
using IdleRPG.Stage;

namespace IdleRPG.Core
{
    /// <summary>
    /// 스테이지 진행 상태의 저장/로드를 담당한다.
    /// </summary>
    public class StageSaveCollector : ISaveDataCollector
    {
        private readonly StageModel _stageModel;
        private Action _markDirty;

        public StageSaveCollector(StageModel stageModel)
        {
            _stageModel = stageModel;
        }

        /// <inheritdoc />
        public void ExtractTo(GameSaveData saveData)
        {
            saveData.Stage = new StageSaveData
            {
                CurrentChapter = _stageModel.CurrentChapter.Value,
                CurrentStage = _stageModel.CurrentStage.Value,
                CurrentWave = _stageModel.CurrentWave.Value,
                IsBossAutoChallenge = _stageModel.IsBossAutoChallenge.Value,
                HighestChapter = _stageModel.HighestChapter,
                HighestStage = _stageModel.HighestStage
            };
        }

        /// <inheritdoc />
        public void ApplyFrom(GameSaveData saveData)
        {
            var data = saveData.Stage;
            if (data == null) return;

            _stageModel.CurrentChapter.Value = data.CurrentChapter;
            _stageModel.CurrentStage.Value = data.CurrentStage;
            _stageModel.CurrentWave.Value = data.CurrentWave;
            _stageModel.IsBossAutoChallenge.Value = data.IsBossAutoChallenge;
            _stageModel.HighestChapter = data.HighestChapter;
            _stageModel.HighestStage = data.HighestStage;
        }

        /// <inheritdoc />
        public void SubscribeDirtyEvents(IMessageBrokerService broker, Action markDirty)
        {
            _markDirty = markDirty;
            broker.Subscribe<StageClearedMessage>(OnStageCleared);
            broker.Subscribe<ChapterClearedMessage>(OnChapterCleared);
        }

        /// <inheritdoc />
        public void UnsubscribeDirtyEvents(IMessageBrokerService broker)
        {
            broker.Unsubscribe<StageClearedMessage>(this);
            broker.Unsubscribe<ChapterClearedMessage>(this);
            _markDirty = null;
        }

        private void OnStageCleared(StageClearedMessage message) => _markDirty?.Invoke();
        private void OnChapterCleared(ChapterClearedMessage message) => _markDirty?.Invoke();
    }
}
