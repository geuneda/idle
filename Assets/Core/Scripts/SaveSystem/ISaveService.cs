namespace IdleRPG.Core
{
    /// <summary>
    /// 게임 데이터의 저장/로드를 관리하는 서비스 인터페이스.
    /// 2-Tier 저장 전략을 지원한다.
    /// </summary>
    /// <remarks>
    /// <para><b>Tier 1 - 즉시 저장</b>: <see cref="Save"/>를 직접 호출한다. 결제/가챠 등 데이터 손실이 치명적인 경우에 사용.</para>
    /// <para><b>Tier 2 - Dirty + Debounce</b>: <see cref="MarkDirty"/>를 호출한다. 스탯 레벨업/골드 획득 등 연속 발생 가능한 경우에 사용.</para>
    /// </remarks>
    public interface ISaveService
    {
        /// <summary>
        /// 현재 모델 상태를 즉시 저장한다 (Tier 1).
        /// dirty 플래그를 초기화하고 대기 중인 debounce 저장을 취소한다.
        /// </summary>
        void Save();

        /// <summary>
        /// dirty 상태면 즉시 저장한다. 앱 Pause/Quit 시 호출하여 미저장 데이터 손실을 방지한다.
        /// </summary>
        void SaveIfDirty();

        /// <summary>
        /// dirty 플래그를 설정하고 debounce 저장을 예약한다 (Tier 2).
        /// 이미 대기 중인 debounce가 있으면 무시한다.
        /// </summary>
        void MarkDirty();

        /// <summary>
        /// 저장된 데이터를 로드하여 반환한다. 저장 데이터가 없으면 기본값을 반환한다.
        /// </summary>
        /// <returns>로드된 저장 데이터</returns>
        GameSaveData Load();

        /// <summary>
        /// 저장 데이터가 존재하는지 확인한다.
        /// </summary>
        /// <returns>저장 데이터가 있으면 true</returns>
        bool HasSaveData();

        /// <summary>
        /// 저장 데이터를 삭제한다. 디버그 및 데이터 리셋 용도.
        /// </summary>
        void DeleteSaveData();

        /// <summary>
        /// 주기적 자동 저장과 이벤트 기반 dirty 마킹을 시작한다.
        /// </summary>
        void StartAutoSave();

        /// <summary>
        /// 자동 저장과 이벤트 구독을 중지한다.
        /// </summary>
        void StopAutoSave();
    }
}
