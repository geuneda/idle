using System;
using Cysharp.Threading.Tasks;
using Nakama;
using Newtonsoft.Json.Linq;
using UnityEngine;
using static IdleRPG.Core.DevLog;

namespace IdleRPG.Core
{
    /// <summary>
    /// Nakama 서버 통신 서비스 구현체.
    /// 디바이스 인증, 세션 토큰 캐시, RPC를 통한 게임 데이터 저장/로드를 수행한다.
    /// </summary>
    public class NakamaService : INakamaService
    {
        private const string AuthTokenKey = "nakama_auth_token";
        private const string RefreshTokenKey = "nakama_refresh_token";
        private const string RpcSaveGame = "save_game";
        private const string RpcLoadGame = "load_game";
        private const string ResponseFieldServerTime = "serverTime";
        private const string ResponseFieldData = "data";
        private const int HttpStatusClientErrorMin = 400;
        private const int HttpStatusClientErrorMax = 499;

        private readonly IClient _client;
        private readonly NakamaConfig _config;
        private ISession _session;
        private long _serverTimeMillis;

        /// <summary>서버 시간 (마지막 동기화 기준, Unix 밀리초)</summary>
        public long ServerTimeMillis => _serverTimeMillis;

        /// <summary>현재 세션이 유효한지 확인한다.</summary>
        public bool IsSessionValid => _session != null && !_session.IsExpired;

        /// <summary>
        /// NakamaService를 생성한다.
        /// </summary>
        /// <param name="config">서버 접속 설정</param>
        public NakamaService(NakamaConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));

            var scheme = config.UseSSL ? "https" : "http";
            _client = new Client(scheme, config.Host, config.Port, config.ServerKey);

            Log($"[Nakama] Client created: {scheme}://{config.Host}:{config.Port}");
        }

        /// <summary>
        /// 디바이스 고유 ID로 Nakama 서버에 인증한다.
        /// 캐시된 세션 토큰이 있으면 복원을 시도하고, 실패 시 지수 백오프 재시도로 새로 인증한다.
        /// 4xx 클라이언트 에러는 재시도하지 않는다.
        /// </summary>
        public async UniTask AuthenticateAsync()
        {
            if (TryRestoreSession())
            {
                Log("[Nakama] Session restored from cache");
                return;
            }

            var deviceId = SystemInfo.deviceUniqueIdentifier;
            Log($"[Nakama] Authenticating with device ID: {deviceId[..Mathf.Min(8, deviceId.Length)]}...");

            var maxRetries = _config.MaxRetryCount;
            var baseDelay = _config.RetryBaseDelaySec;

            for (var attempt = 0; attempt <= maxRetries; attempt++)
            {
                try
                {
                    _session = await _client.AuthenticateDeviceAsync(deviceId);
                    CacheSessionTokens();
                    Log($"[Nakama] Authenticated. UserId: {_session.UserId}");
                    return;
                }
                catch (ApiResponseException ex) when (IsClientError(ex.StatusCode))
                {
                    Log($"[Nakama] Auth client error (HTTP {ex.StatusCode}), not retrying");
                    throw;
                }
                catch (Exception ex) when (attempt < maxRetries)
                {
                    var delay = baseDelay * (1 << attempt);
                    Log($"[Nakama] Auth failed (attempt {attempt + 1}/{maxRetries + 1}): {ex.Message}. " +
                        $"Retrying in {delay:F1}s");
                    await UniTask.Delay(TimeSpan.FromSeconds(delay));
                }
            }

            _session = await _client.AuthenticateDeviceAsync(deviceId);
            CacheSessionTokens();
            Log($"[Nakama] Authenticated. UserId: {_session.UserId}");
        }

        /// <summary>
        /// 만료된 세션을 갱신한다. 갱신 실패 시 디바이스 재인증을 시도한다.
        /// </summary>
        public async UniTask RefreshSessionAsync()
        {
            if (_session == null)
            {
                throw new InvalidOperationException("No session to refresh. Call AuthenticateAsync first.");
            }

            try
            {
                _session = await _client.SessionRefreshAsync(_session);
                CacheSessionTokens();
                Log("[Nakama] Session refreshed");
            }
            catch (ApiResponseException)
            {
                Log("[Nakama] Session refresh failed, re-authenticating");
                var deviceId = SystemInfo.deviceUniqueIdentifier;
                _session = await _client.AuthenticateDeviceAsync(deviceId);
                CacheSessionTokens();
                Log("[Nakama] Re-authenticated after refresh failure");
            }
        }

        /// <summary>
        /// 서버에서 게임 데이터를 로드한다.
        /// RPC <c>load_game</c>을 호출하여 저장 데이터와 서버 시간을 가져온다.
        /// </summary>
        /// <returns>로드 결과. 저장 데이터가 없으면 <see cref="NakamaLoadResult.HasData"/>가 false.</returns>
        public async UniTask<NakamaLoadResult> LoadGameDataAsync()
        {
            await EnsureSessionValid();

            var response = await ExecuteRpcWithRetry(RpcLoadGame, "{}");
            var json = JObject.Parse(response.Payload);

            var serverTime = json.Value<long>(ResponseFieldServerTime);
            _serverTimeMillis = serverTime;

            var dataToken = json[ResponseFieldData];
            var hasData = dataToken != null && dataToken.Type != JTokenType.Null;

            var result = new NakamaLoadResult
            {
                HasData = hasData,
                JsonData = hasData ? dataToken.ToString() : null,
                ServerTimeMillis = serverTime
            };

            Log($"[Nakama] Game data loaded. HasData: {result.HasData}, ServerTime: {serverTime}");
            return result;
        }

        /// <summary>
        /// 서버에 게임 데이터를 저장한다.
        /// RPC <c>save_game</c>을 호출하고 응답에서 서버 시간을 갱신한다.
        /// </summary>
        /// <param name="jsonData">JSON 직렬화된 GameSaveData</param>
        public async UniTask SaveGameDataAsync(string jsonData)
        {
            await EnsureSessionValid();

            var response = await ExecuteRpcWithRetry(RpcSaveGame, jsonData);
            var json = JObject.Parse(response.Payload);

            var serverTime = json.Value<long>(ResponseFieldServerTime);
            _serverTimeMillis = serverTime;

            Log($"[Nakama] Game data saved. ServerTime: {serverTime}");
        }

        /// <summary>
        /// 캐시된 세션 토큰으로 세션 복원을 시도한다.
        /// 만료되지 않은 유효한 세션만 복원한다. 만료 세션은 false를 반환하여 재인증을 유도한다.
        /// </summary>
        /// <returns>유효한 세션 복원 성공 시 true</returns>
        private bool TryRestoreSession()
        {
            var authToken = PlayerPrefs.GetString(AuthTokenKey, "");
            var refreshToken = PlayerPrefs.GetString(RefreshTokenKey, "");

            if (string.IsNullOrEmpty(authToken))
            {
                return false;
            }

            var restored = Session.Restore(authToken, refreshToken);

            if (restored.IsExpired)
            {
                return false;
            }

            _session = restored;
            return true;
        }

        /// <summary>
        /// 세션 토큰을 PlayerPrefs에 캐시한다.
        /// </summary>
        private void CacheSessionTokens()
        {
            PlayerPrefs.SetString(AuthTokenKey, _session.AuthToken);
            PlayerPrefs.SetString(RefreshTokenKey, _session.RefreshToken);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// 세션 유효성을 확인하고, 만료 시 갱신한다.
        /// </summary>
        private async UniTask EnsureSessionValid()
        {
            if (_session == null)
            {
                throw new InvalidOperationException("Not authenticated. Call AuthenticateAsync first.");
            }

            if (_session.IsExpired)
            {
                await RefreshSessionAsync();
            }
        }

        /// <summary>
        /// RPC를 지수 백오프 재시도 로직과 함께 실행한다.
        /// 4xx 클라이언트 에러는 재시도하지 않고 즉시 전파한다.
        /// </summary>
        /// <param name="rpcId">RPC 식별자</param>
        /// <param name="payload">요청 페이로드 (JSON 문자열)</param>
        /// <returns>RPC 응답</returns>
        private async UniTask<IApiRpc> ExecuteRpcWithRetry(string rpcId, string payload)
        {
            var maxRetries = _config.MaxRetryCount;
            var baseDelay = _config.RetryBaseDelaySec;

            for (var attempt = 0; attempt <= maxRetries; attempt++)
            {
                try
                {
                    return await _client.RpcAsync(_session, rpcId, payload);
                }
                catch (ApiResponseException ex) when (IsClientError(ex.StatusCode))
                {
                    Log($"[Nakama] RPC '{rpcId}' client error (HTTP {ex.StatusCode}), not retrying");
                    throw;
                }
                catch (Exception ex) when (attempt < maxRetries)
                {
                    var delay = baseDelay * Mathf.Pow(2, attempt);
                    Log($"[Nakama] RPC '{rpcId}' failed (attempt {attempt + 1}/{maxRetries + 1}): {ex.Message}. " +
                        $"Retrying in {delay:F1}s");

                    await UniTask.Delay(TimeSpan.FromSeconds(delay));
                }
            }

            // 마지막 시도는 예외를 그대로 전파한다
            return await _client.RpcAsync(_session, rpcId, payload);
        }

        /// <summary>
        /// HTTP 상태 코드가 4xx 클라이언트 에러 범위인지 확인한다.
        /// </summary>
        /// <param name="statusCode">HTTP 상태 코드</param>
        /// <returns>4xx 범위이면 true</returns>
        private static bool IsClientError(long statusCode)
        {
            return statusCode >= HttpStatusClientErrorMin && statusCode <= HttpStatusClientErrorMax;
        }
    }
}
