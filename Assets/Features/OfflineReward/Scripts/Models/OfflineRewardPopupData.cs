using System;

namespace IdleRPG.OfflineReward
{
    /// <summary>
    /// 오프라인 보상 팝업에 전달되는 데이터.
    /// </summary>
    public struct OfflineRewardPopupData
    {
        /// <summary>계산된 보상 결과</summary>
        public OfflineRewardResult Result;

        /// <summary>보상 수령 완료 콜백. true면 광고 시청으로 2배 적용.</summary>
        public Action<bool> OnClaimed;
    }
}
