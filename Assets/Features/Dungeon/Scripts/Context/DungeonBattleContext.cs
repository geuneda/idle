using Geuneda.Services;
using IdleRPG.Core;

namespace IdleRPG.Dungeon
{
    /// <summary>
    /// 던전 전투용 <see cref="IBattleContext"/> 구현.
    /// 웨이브 진행을 내부에서 추적하고 던전 전용 메시지를 발행한다.
    /// </summary>
    public class DungeonBattleContext : IBattleContext
    {
        private readonly DungeonLevelConfig _levelConfig;
        private readonly DungeonType _dungeonType;
        private readonly int _level;
        private readonly IMessageBrokerService _messageBroker;

        private int _currentWave;
        private float _remainingTime;
        private bool _timerActive;
        private int _lastPublishedSecond;

        /// <summary>던전 종류</summary>
        public DungeonType DungeonType => _dungeonType;

        /// <summary>던전 레벨</summary>
        public int Level => _level;

        /// <summary>현재 웨이브 인덱스 (0-based)</summary>
        public int CurrentWave => _currentWave;

        /// <summary>총 웨이브 수</summary>
        public int TotalWaves => _levelConfig.WaveCount;

        /// <summary>남은 시간 (초)</summary>
        public float RemainingTime => _remainingTime;

        /// <summary>
        /// <see cref="DungeonBattleContext"/>를 생성한다.
        /// </summary>
        /// <param name="levelConfig">던전 레벨 설정</param>
        /// <param name="dungeonType">던전 종류</param>
        /// <param name="level">던전 레벨</param>
        /// <param name="messageBroker">이벤트 발행용 메시지 브로커</param>
        public DungeonBattleContext(
            DungeonLevelConfig levelConfig,
            DungeonType dungeonType,
            int level,
            IMessageBrokerService messageBroker)
        {
            _levelConfig = levelConfig;
            _dungeonType = dungeonType;
            _level = level;
            _messageBroker = messageBroker;
            _currentWave = 0;
            _remainingTime = levelConfig.TimeLimitSeconds;
            _timerActive = true;
            _lastPublishedSecond = UnityEngine.Mathf.CeilToInt(levelConfig.TimeLimitSeconds);
        }

        /// <inheritdoc />
        public bool ShouldAutoRestart => false;

        /// <inheritdoc />
        public int GetWaveEnemyCount()
        {
            return _levelConfig.EnemiesPerWave;
        }

        /// <inheritdoc />
        public BigNumber GetHpMultiplier()
        {
            return _levelConfig.GetWaveHpMultiplier(_currentWave);
        }

        /// <inheritdoc />
        public BigNumber GetAttackMultiplier()
        {
            return _levelConfig.GetWaveAttackMultiplier(_currentWave);
        }

        /// <inheritdoc />
        public bool IsBossWave()
        {
            return false;
        }

        /// <inheritdoc />
        public void OnWaveStarted()
        {
            _messageBroker.Publish(new DungeonWaveStartedMessage
            {
                Type = _dungeonType,
                WaveIndex = _currentWave,
                TotalWaves = TotalWaves
            });
        }

        /// <inheritdoc />
        public bool OnAllEnemiesCleared()
        {
            _messageBroker.Publish(new DungeonWaveClearedMessage
            {
                Type = _dungeonType,
                WaveIndex = _currentWave,
                TotalWaves = TotalWaves
            });

            _currentWave++;

            if (_currentWave >= TotalWaves)
            {
                _messageBroker.Publish(new DungeonAllWavesClearedMessage
                {
                    Type = _dungeonType,
                    Level = _level
                });
                return false;
            }

            return true;
        }

        /// <inheritdoc />
        public void OnHeroDied()
        {
            _timerActive = false;
            _messageBroker.Publish(new DungeonHeroDiedMessage
            {
                Type = _dungeonType,
                Level = _level
            });
        }

        /// <inheritdoc />
        public void Tick(float deltaTime)
        {
            if (!_timerActive) return;

            _remainingTime -= deltaTime;

            int currentSecond = UnityEngine.Mathf.CeilToInt(UnityEngine.Mathf.Max(0f, _remainingTime));
            if (currentSecond != _lastPublishedSecond)
            {
                _lastPublishedSecond = currentSecond;
                _messageBroker.Publish(new DungeonTimerTickMessage
                {
                    RemainingTime = _remainingTime
                });
            }

            if (_remainingTime <= 0f)
            {
                _remainingTime = 0f;
                _timerActive = false;

                _messageBroker.Publish(new DungeonTimeExpiredMessage
                {
                    Type = _dungeonType,
                    Level = _level
                });
            }
        }

        /// <summary>
        /// 타이머를 정지한다.
        /// </summary>
        public void StopTimer()
        {
            _timerActive = false;
        }
    }
}
