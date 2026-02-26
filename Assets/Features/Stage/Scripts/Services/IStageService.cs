namespace IdleRPG.Stage
{
    public interface IStageService
    {
        StageModel Model { get; }
        void StartWave();
        void CompleteWave();
        void FailWave();
        void ToggleBossAutoChallenge(bool enabled);
        bool IsBossWave();
        bool IsBossWave(int waveIndex);
        string GetStageDisplayName();
    }
}
