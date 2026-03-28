using System;

namespace IdleRPG.Core
{
    /// <summary>
    /// Nakama 서버 접속 설정.
    /// </summary>
    [Serializable]
    public class NakamaConfig
    {
        /// <summary>서버 호스트</summary>
        public string Host = "127.0.0.1";

        /// <summary>서버 포트</summary>
        public int Port = 7350;

        /// <summary>서버 인증 키</summary>
        public string ServerKey = "idle-rpg-dev";

        /// <summary>SSL 사용 여부</summary>
        public bool UseSSL;

        /// <summary>RPC 호출 재시도 최대 횟수</summary>
        public int MaxRetryCount = 3;

        /// <summary>재시도 간 기본 대기 시간 (초)</summary>
        public float RetryBaseDelaySec = 1f;
    }
}
