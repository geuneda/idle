using System;

namespace IdleRPG.Stage
{
    /// <summary>
    /// 스테이지 시스템의 구조적 설정 데이터.
    /// 챕터당 스테이지 수, 스테이지당 웨이브 수, 보스 웨이브 인덱스를 정의한다.
    /// </summary>
    /// <remarks>
    /// 최소 구현이므로 공식 기반 스케일링 파라미터만 포함한다.
    /// 추후 Google Sheets + <c>ConfigsProvider</c>로 개별 스테이지 데이터 확장 가능.
    /// </remarks>
    [Serializable]
    public class StageConfig
    {
        /// <summary>챕터당 스테이지 수 (기본값: 10)</summary>
        public int StagesPerChapter = 10;

        /// <summary>스테이지당 웨이브 수 (기본값: 5)</summary>
        public int WavesPerStage = 5;

        /// <summary>보스 웨이브의 인덱스 (0-based, 기본값: 4 = 마지막 웨이브)</summary>
        public int BossWaveIndex = 4;
    }
}
