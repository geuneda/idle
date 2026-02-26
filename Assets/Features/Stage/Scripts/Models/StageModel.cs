using System;
using Geuneda.DataExtensions;

namespace IdleRPG.Stage
{
    [Serializable]
    public class StageModel
    {
        public ObservableField<int> CurrentChapter;
        public ObservableField<int> CurrentStage;
        public ObservableField<int> CurrentWave;
        public ObservableField<bool> IsBossAutoChallenge;
        public int HighestChapter;
        public int HighestStage;

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
