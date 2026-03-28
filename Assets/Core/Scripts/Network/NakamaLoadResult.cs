namespace IdleRPG.Core
{
    /// <summary>
    /// 서버 게임 데이터 로드 결과.
    /// </summary>
    public class NakamaLoadResult
    {
        /// <summary>서버에 저장 데이터가 존재하는지 여부</summary>
        public bool HasData { get; set; }

        /// <summary>JSON 직렬화된 GameSaveData. HasData가 false이면 null.</summary>
        public string JsonData { get; set; }

        /// <summary>서버 시간 (Unix 밀리초)</summary>
        public long ServerTimeMillis { get; set; }
    }
}
