namespace IdleRPG.Stage
{
    /// <summary>
    /// 스테이지 진행 로직을 제공하는 서비스 인터페이스.
    /// 웨이브 시작/클리어/실패, 보스 자동 도전 제어, 스테이지 표시명 조회를 담당한다.
    /// </summary>
    /// <remarks>
    /// <see cref="Geuneda.Services.MainInstaller"/>에 바인딩하여 사용한다.
    /// </remarks>
    public interface IStageService
    {
        /// <summary>현재 스테이지 진행 상태 모델</summary>
        StageModel Model { get; }

        /// <summary>
        /// 현재 웨이브를 시작하고 <see cref="IdleRPG.Core.WaveStartedMessage"/>를 발행한다.
        /// </summary>
        void StartWave();

        /// <summary>
        /// 현재 웨이브를 클리어하고 다음 웨이브/스테이지/챕터로 진행한다.
        /// </summary>
        /// <remarks>
        /// <para>보스 웨이브 클리어 시 다음 스테이지로, 마지막 스테이지 클리어 시 다음 챕터로 진행한다.</para>
        /// <para>보스 자동 도전이 꺼진 상태에서 마지막 일반 웨이브를 클리어하면 첫 웨이브로 복귀한다.</para>
        /// </remarks>
        void CompleteWave();

        /// <summary>
        /// 현재 웨이브에서 실패 처리한다. 보스 웨이브에서만 동작한다.
        /// </summary>
        /// <remarks>
        /// 보스 웨이브 실패 시: 웨이브를 0으로 리셋하고 보스 자동 도전을 해제한다.
        /// 이후 일반 웨이브(0~3)를 무한 반복하며, 수동으로 보스 도전을 재활성화해야 한다.
        /// </remarks>
        void FailWave();

        /// <summary>
        /// 보스 자동 도전 활성화/비활성화를 전환한다.
        /// </summary>
        /// <param name="enabled">true이면 보스 자동 도전 활성화, false이면 일반 웨이브 무한 반복</param>
        void ToggleBossAutoChallenge(bool enabled);

        /// <summary>
        /// 현재 웨이브가 보스 웨이브인지 확인한다.
        /// </summary>
        /// <returns>보스 웨이브이면 true</returns>
        bool IsBossWave();

        /// <summary>
        /// 지정된 웨이브 인덱스가 보스 웨이브인지 확인한다.
        /// </summary>
        /// <param name="waveIndex">확인할 웨이브 인덱스 (0-based)</param>
        /// <returns>보스 웨이브이면 true</returns>
        bool IsBossWave(int waveIndex);

        /// <summary>
        /// 현재 스테이지의 표시용 이름을 반환한다. (예: "1-3")
        /// </summary>
        /// <returns>"챕터-스테이지" 형식의 문자열</returns>
        string GetStageDisplayName();
    }
}
