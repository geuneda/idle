using Geuneda.Services;
using IdleRPG.Core;

namespace IdleRPG.Stage
{
    public class StageService : IStageService
    {
        private readonly StageConfig _config;
        private readonly IMessageBrokerService _messageBroker;

        public StageModel Model { get; }

        public StageService(StageConfig config, StageModel model, IMessageBrokerService messageBroker)
        {
            _config = config;
            _messageBroker = messageBroker;
            Model = model;
        }

        public void StartWave()
        {
            _messageBroker.Publish(new WaveStartedMessage
            {
                Chapter = Model.CurrentChapter.Value,
                Stage = Model.CurrentStage.Value,
                Wave = Model.CurrentWave.Value,
                IsBossWave = IsBossWave()
            });
        }

        public void CompleteWave()
        {
            int chapter = Model.CurrentChapter.Value;
            int stage = Model.CurrentStage.Value;
            int wave = Model.CurrentWave.Value;

            _messageBroker.Publish(new WaveClearedMessage
            {
                Chapter = chapter,
                Stage = stage,
                Wave = wave
            });

            if (IsBossWave(wave))
            {
                AdvanceStage(chapter, stage);
                return;
            }

            int nextWave = wave + 1;

            if (nextWave >= _config.BossWaveIndex && !Model.IsBossAutoChallenge.Value)
            {
                Model.CurrentWave.Value = 0;
            }
            else
            {
                Model.CurrentWave.Value = nextWave;
            }
        }

        public void FailWave()
        {
            if (!IsBossWave()) return;

            int chapter = Model.CurrentChapter.Value;
            int stage = Model.CurrentStage.Value;

            Model.CurrentWave.Value = 0;
            Model.IsBossAutoChallenge.Value = false;

            _messageBroker.Publish(new BossChallengeFailedMessage
            {
                Chapter = chapter,
                Stage = stage
            });

            _messageBroker.Publish(new BossAutoChallengeChangedMessage
            {
                IsEnabled = false
            });
        }

        public void ToggleBossAutoChallenge(bool enabled)
        {
            Model.IsBossAutoChallenge.Value = enabled;

            _messageBroker.Publish(new BossAutoChallengeChangedMessage
            {
                IsEnabled = enabled
            });
        }

        public bool IsBossWave()
        {
            return IsBossWave(Model.CurrentWave.Value);
        }

        public bool IsBossWave(int waveIndex)
        {
            return waveIndex == _config.BossWaveIndex;
        }

        public string GetStageDisplayName()
        {
            return $"{Model.CurrentChapter.Value}-{Model.CurrentStage.Value}";
        }

        private void AdvanceStage(int chapter, int stage)
        {
            _messageBroker.Publish(new StageClearedMessage
            {
                Chapter = chapter,
                Stage = stage
            });

            if (stage >= _config.StagesPerChapter)
            {
                _messageBroker.Publish(new ChapterClearedMessage
                {
                    Chapter = chapter
                });

                Model.CurrentChapter.Value = chapter + 1;
                Model.CurrentStage.Value = 1;
            }
            else
            {
                Model.CurrentStage.Value = stage + 1;
            }

            Model.CurrentWave.Value = 0;
            Model.IsBossAutoChallenge.Value = true;

            UpdateHighestProgress();
        }

        private void UpdateHighestProgress()
        {
            int chapter = Model.CurrentChapter.Value;
            int stage = Model.CurrentStage.Value;

            if (chapter > Model.HighestChapter ||
                (chapter == Model.HighestChapter && stage > Model.HighestStage))
            {
                Model.HighestChapter = chapter;
                Model.HighestStage = stage;
            }
        }
    }
}
