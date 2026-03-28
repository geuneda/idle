using Cysharp.Threading.Tasks;

namespace IdleRPG.Core
{
    /// <summary>
    /// Nakama 서버 통신 서비스 인터페이스.
    /// 디바이스 인증, 세션 관리, 게임 데이터 저장/로드를 담당한다.
    /// </summary>
    public interface INakamaService
    {
        /// <summary>디바이스 고유 ID로 Nakama 서버에 인증한다.</summary>
        UniTask AuthenticateAsync();

        /// <summary>현재 세션이 유효한지 확인한다.</summary>
        bool IsSessionValid { get; }

        /// <summary>만료된 세션을 갱신한다.</summary>
        UniTask RefreshSessionAsync();

        /// <summary>서버에서 게임 데이터를 로드한다.</summary>
        /// <returns>로드 결과</returns>
        UniTask<NakamaLoadResult> LoadGameDataAsync();

        /// <summary>서버에 게임 데이터를 저장한다 (RPC 경유).</summary>
        /// <param name="jsonData">JSON 직렬화된 GameSaveData</param>
        UniTask SaveGameDataAsync(string jsonData);

        /// <summary>서버 시간 (마지막 동기화 기준, Unix 밀리초)</summary>
        long ServerTimeMillis { get; }
    }
}
