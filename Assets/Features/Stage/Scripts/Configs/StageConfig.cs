using System;
using IdleRPG.Core;

namespace IdleRPG.Stage
{
    /// <summary>
    /// 스테이지 구조 및 밸런스 설정 데이터.
    /// 챕터/스테이지/웨이브 구성과 적 스탯 배율을 정의한다.
    /// </summary>
    [Serializable]
    public class StageConfig
    {
        /// <summary>챕터당 스테이지 수</summary>
        public int StagesPerChapter = 10;

        /// <summary>스테이지당 웨이브 수</summary>
        public int WavesPerStage = 5;

        /// <summary>보스 웨이브 인덱스 (0-based)</summary>
        public int BossWaveIndex = 4;

        /// <summary>일반 웨이브당 적 수</summary>
        public int EnemiesPerNormalWave = 3;

        /// <summary>보스 웨이브당 적 수</summary>
        public int EnemiesPerBossWave = 1;

        /// <summary>스테이지당 적 체력 증가 비율</summary>
        public float HpScalePerStage = 0.15f;

        /// <summary>스테이지당 적 공격력 증가 비율</summary>
        public float AttackScalePerStage = 0.1f;

        /// <summary>
        /// 지정된 웨이브 인덱스의 적 출현 수를 반환한다.
        /// </summary>
        /// <param name="waveIndex">웨이브 인덱스 (0-based)</param>
        /// <returns>적 출현 수</returns>
        public int GetEnemyCount(int waveIndex)
        {
            return waveIndex == BossWaveIndex ? EnemiesPerBossWave : EnemiesPerNormalWave;
        }

        /// <summary>
        /// 지정된 챕터/스테이지의 적 체력 배율을 계산한다.
        /// </summary>
        /// <param name="chapter">챕터 번호 (1-based)</param>
        /// <param name="stage">스테이지 번호 (1-based)</param>
        /// <returns>체력 배율</returns>
        public BigNumber GetHpMultiplier(int chapter, int stage)
        {
            int globalIndex = (chapter - 1) * StagesPerChapter + (stage - 1);
            return 1.0 + globalIndex * HpScalePerStage;
        }

        /// <summary>
        /// 지정된 챕터/스테이지의 적 공격력 배율을 계산한다.
        /// </summary>
        /// <param name="chapter">챕터 번호 (1-based)</param>
        /// <param name="stage">스테이지 번호 (1-based)</param>
        /// <returns>공격력 배율</returns>
        public BigNumber GetAttackMultiplier(int chapter, int stage)
        {
            int globalIndex = (chapter - 1) * StagesPerChapter + (stage - 1);
            return 1.0 + globalIndex * AttackScalePerStage;
        }
    }
}
