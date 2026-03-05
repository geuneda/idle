using System;
using IdleRPG.Core;

namespace IdleRPG.Dungeon
{
    /// <summary>
    /// 던전 레벨별 설정 데이터. Google Sheets에서 임포트된다.
    /// </summary>
    [Serializable]
    public class DungeonLevelConfig
    {
        /// <summary>던전 종류</summary>
        public DungeonType Type = DungeonType.Gem;

        /// <summary>레벨 (1-based)</summary>
        public int Level = 1;

        /// <summary>웨이브 수 (보석/유물: 5, 골드: 1)</summary>
        public int WaveCount = 5;

        /// <summary>웨이브당 적 수</summary>
        public int EnemiesPerWave = 5;

        /// <summary>제한 시간 (초)</summary>
        public float TimeLimitSeconds = 60f;

        /// <summary>기본 HP 계수</summary>
        public double HpMultiplier = 1.0;

        /// <summary>웨이브별 HP 증가율</summary>
        public float HpScalePerWave = 0.2f;

        /// <summary>기본 ATK 계수</summary>
        public double AttackMultiplier = 1.0;

        /// <summary>웨이브별 ATK 증가율</summary>
        public float AttackScalePerWave = 0.15f;

        /// <summary>클리어 보상 수량</summary>
        public double RewardAmount = 100;

        /// <summary>일일 입장 제한 횟수</summary>
        public int DailyEntryLimit = 2;

        /// <summary>
        /// 지정 웨이브의 HP 계수를 계산한다.
        /// </summary>
        /// <param name="waveIndex">웨이브 인덱스 (0-based)</param>
        /// <returns>해당 웨이브의 HP 계수</returns>
        public BigNumber GetWaveHpMultiplier(int waveIndex)
        {
            return HpMultiplier * (1.0 + waveIndex * HpScalePerWave);
        }

        /// <summary>
        /// 지정 웨이브의 ATK 계수를 계산한다.
        /// </summary>
        /// <param name="waveIndex">웨이브 인덱스 (0-based)</param>
        /// <returns>해당 웨이브의 ATK 계수</returns>
        public BigNumber GetWaveAttackMultiplier(int waveIndex)
        {
            return AttackMultiplier * (1.0 + waveIndex * AttackScalePerWave);
        }
    }
}
