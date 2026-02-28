using Geuneda.Services;
using IdleRPG.Core;

namespace IdleRPG.Stage
{
    /// <summary>
    /// <see cref="IStageService"/>의 구현체. 스테이지/웨이브 진행 로직을 관리한다.
    /// </summary>
    /// <remarks>
    /// <para>스테이지 흐름: 웨이브 0~3(일반) → 웨이브 4(보스) → 다음 스테이지</para>
    /// <para>챕터 내 모든 스테이지 클리어 시 다음 챕터로 진행한다.</para>
    /// <para>보스 실패 시 자동 도전이 해제되고 일반 웨이브를 무한 반복한다.</para>
    /// </remarks>
    public class StageService : IStageService
    {
        private readonly StageConfig _config;
        private readonly IMessageBrokerService _messageBroker;

        /// <inheritdoc />
        public StageModel Model { get; }

        /// <inheritdoc />
        public int WavesPerStage => _config.WavesPerStage;

        /// <summary>
        /// <see cref="StageService"/>를 생성한다.
        /// </summary>
        /// <param name="config">스테이지 구조 설정</param>
        /// <param name="model">스테이지 진행 상태 모델</param>
        /// <param name="messageBroker">이벤트 발행용 메시지 브로커</param>
        public StageService(StageConfig config, StageModel model, IMessageBrokerService messageBroker)
        {
            _config = config;
            _messageBroker = messageBroker;
            Model = model;
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
        public void FailWave()
        {
            int chapter = Model.CurrentChapter.Value;
            int stage = Model.CurrentStage.Value;
            bool wasBossWave = IsBossWave();

            Model.CurrentWave.Value = 0;

            if (wasBossWave)
            {
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
        }

        /// <inheritdoc />
        public void ToggleBossAutoChallenge(bool enabled)
        {
            Model.IsBossAutoChallenge.Value = enabled;

            _messageBroker.Publish(new BossAutoChallengeChangedMessage
            {
                IsEnabled = enabled
            });
        }

        /// <inheritdoc />
        public bool IsBossWave()
        {
            return IsBossWave(Model.CurrentWave.Value);
        }

        /// <inheritdoc />
        public bool IsBossWave(int waveIndex)
        {
            return waveIndex == _config.BossWaveIndex;
        }

        /// <inheritdoc />
        public string GetStageDisplayName()
        {
            return $"{Model.CurrentChapter.Value}-{Model.CurrentStage.Value}";
        }

        /// <summary>
        /// 현재 스테이지를 클리어하고 다음 스테이지 또는 챕터로 진행한다.
        /// </summary>
        /// <param name="chapter">클리어한 챕터 번호</param>
        /// <param name="stage">클리어한 스테이지 번호</param>
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

        /// <summary>
        /// 현재 진행 상태가 최고 기록보다 높으면 갱신한다.
        /// </summary>
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
