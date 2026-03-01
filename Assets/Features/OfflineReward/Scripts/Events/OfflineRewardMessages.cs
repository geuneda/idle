using System.Collections.Generic;
using Geuneda.Services;
using IdleRPG.Core;

namespace IdleRPG.OfflineReward
{
    /// <summary>
    /// 오프라인 보상이 지급되었을 때 발행되는 메시지.
    /// </summary>
    public struct OfflineRewardClaimedMessage : IMessage
    {
        /// <summary>지급된 골드량</summary>
        public BigNumber Gold;

        /// <summary>드롭된 아이템 목록</summary>
        public IReadOnlyList<OfflineDropResult> Drops;

        /// <summary>광고 2배 적용 여부</summary>
        public bool IsDoubled;

        /// <summary>클램프된 오프라인 경과 시간 (분)</summary>
        public double ElapsedMinutes;
    }
}
