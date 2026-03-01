namespace IdleRPG.OfflineReward
{
    /// <summary>
    /// 오프라인 보상 계산 및 지급을 관리하는 서비스 인터페이스.
    /// </summary>
    public interface IOfflineRewardService
    {
        /// <summary>오프라인 보상 설정 데이터</summary>
        OfflineRewardConfig Config { get; }

        /// <summary>
        /// 오프라인 보상이 있는지 확인한다. 최소 경과 시간 이상이어야 한다.
        /// </summary>
        /// <param name="lastSaveTimestamp">마지막 저장 시각 (Unix 밀리초)</param>
        /// <returns>오프라인 보상이 있으면 true</returns>
        bool HasOfflineReward(long lastSaveTimestamp);

        /// <summary>
        /// 오프라인 보상을 계산한다.
        /// </summary>
        /// <param name="lastSaveTimestamp">마지막 저장 시각 (Unix 밀리초)</param>
        /// <returns>계산된 오프라인 보상 결과</returns>
        OfflineRewardResult CalculateReward(long lastSaveTimestamp);

        /// <summary>
        /// 계산된 보상을 실제로 지급한다.
        /// 골드는 <see cref="IdleRPG.Economy.ICurrencyService"/>를 통해,
        /// 아이템은 <see cref="OfflineRewardClaimedMessage"/>로 발행한다.
        /// </summary>
        /// <param name="result">지급할 보상 결과</param>
        void ClaimReward(OfflineRewardResult result);
    }
}
