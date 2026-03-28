using Cysharp.Threading.Tasks;

namespace IdleRPG.Core
{
    /// <summary>
    /// 게임 데이터의 서버 저장/로드를 관리하는 서비스 인터페이스.
    /// 2-Tier 저장 전략을 지원한다.
    /// </summary>
    /// <remarks>
    /// <para><b>Tier 1 - 즉시 저장</b>: <see cref="SaveAsync"/>를 직접 호출한다. 결제/가챠 등 데이터 손실이 치명적인 경우에 사용.</para>
    /// <para><b>Tier 2 - Dirty + Debounce</b>: <see cref="MarkDirty"/>를 호출한다. 스탯 레벨업/골드 획득 등 연속 발생 가능한 경우에 사용.</para>
    /// <para>모든 저장/로드는 서버를 통해 수행된다. 로컬 폴백은 없다.</para>
    /// </remarks>
    public interface ISaveService
    {
        /// <summary>
        /// 현재 모델 상태를 서버에 즉시 저장한다 (Tier 1, 비동기).
        /// dirty 플래그를 초기화하고 대기 중인 debounce 저장을 취소한다.
        /// 실패 시 예외를 던진다.
        /// </summary>
        UniTask SaveAsync();

        /// <summary>
        /// dirty 상태면 서버에 저장을 시도한다. 앱 Pause/Quit 시 호출한다.
        /// 내부적으로 fire-and-forget으로 실행되며 실패 시 로그만 남긴다.
        /// </summary>
        void SaveIfDirty();

        /// <summary>
        /// dirty 플래그를 설정하고 debounce 저장을 예약한다 (Tier 2).
        /// 이미 대기 중인 debounce가 있으면 무시한다.
        /// </summary>
        void MarkDirty();

        /// <summary>
        /// 서버에서 저장 데이터를 로드한다.
        /// 서버에 데이터가 없으면 기본값을 반환한다.
        /// </summary>
        /// <returns>로드된 저장 데이터</returns>
        UniTask<GameSaveData> LoadAsync();

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
