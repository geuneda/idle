using System;
using Geuneda.DataExtensions;

namespace IdleRPG.Stage
{
    /// <summary>
    /// 스테이지 진행 상태를 관리하는 런타임 모델.
    /// <see cref="ObservableField{T}"/>를 사용하여 UI 자동 바인딩을 지원한다.
    /// </summary>
    /// <remarks>
    /// <see cref="Geuneda.Services.IDataService"/>를 통해 PlayerPrefs + JSON으로 저장/로드된다.
    /// </remarks>
    [Serializable]
    public class StageModel
    {
        /// <summary>현재 챕터 번호 (1-based)</summary>
        public ObservableField<int> CurrentChapter;

        /// <summary>현재 스테이지 번호 (1-based, 챕터 내 1~10)</summary>
        public ObservableField<int> CurrentStage;

        /// <summary>현재 웨이브 인덱스 (0-based, 0~4)</summary>
        public ObservableField<int> CurrentWave;

        /// <summary>보스 자동 도전 활성화 여부. false이면 일반 웨이브만 무한 반복한다.</summary>
        public ObservableField<bool> IsBossAutoChallenge;

        /// <summary>도달한 최고 챕터 번호</summary>
        public int HighestChapter;

        /// <summary>도달한 최고 스테이지 번호 (해당 챕터 내)</summary>
        public int HighestStage;

        /// <summary>
        /// 기본값으로 초기화된 <see cref="StageModel"/>을 생성한다.
        /// 챕터 1, 스테이지 1, 웨이브 0, 보스 자동 도전 활성화 상태로 시작한다.
        /// </summary>
        public StageModel()
        {
            CurrentChapter = new ObservableField<int>(1);
            CurrentStage = new ObservableField<int>(1);
            CurrentWave = new ObservableField<int>(0);
            IsBossAutoChallenge = new ObservableField<bool>(true);
            HighestChapter = 1;
            HighestStage = 1;
        }
    }
}
