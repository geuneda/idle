namespace IdleRPG.Reward
{
    /// <summary>
    /// 보상 지급 서비스 인터페이스.
    /// 적 처치 등 게임 이벤트에 반응하여 재화를 자동으로 지급한다.
    /// </summary>
    /// <remarks>
    /// <see cref="Geuneda.Services.MainInstaller"/>에 바인딩하여 사용한다.
    /// </remarks>
    public interface IRewardService
    {
        /// <summary>현재 보상 설정 데이터</summary>
        RewardConfig Config { get; }
    }
}
