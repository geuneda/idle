using System;
using System.Collections.Generic;
using IdleRPG.Core;

namespace IdleRPG.OfflineReward
{
    /// <summary>
    /// 단일 드롭 아이템의 결과 데이터.
    /// </summary>
    [Serializable]
    public struct OfflineDropResult
    {
        /// <summary>아이템 종류</summary>
        public OfflineDropType DropType;

        /// <summary>아이템 등급</summary>
        public ItemGrade Grade;

        /// <summary>드롭 수량</summary>
        public int Count;
    }

    /// <summary>
    /// 오프라인 보상 계산 결과. UI 표시와 보상 지급에 사용되는 불변 데이터.
    /// </summary>
    public class OfflineRewardResult
    {
        /// <summary>오프라인 경과 시간 (분)</summary>
        public double ElapsedMinutes { get; }

        /// <summary>최대 오프라인 시간에 의해 클램프된 경과 시간 (분)</summary>
        public double ClampedMinutes { get; }

        /// <summary>획득 골드량</summary>
        public BigNumber Gold { get; }

        /// <summary>드롭된 아이템 목록</summary>
        public IReadOnlyList<OfflineDropResult> Drops { get; }

        /// <summary>광고 배율 적용 여부</summary>
        public bool IsDoubled { get; private set; }

        /// <summary>
        /// 오프라인 보상 결과를 생성한다.
        /// </summary>
        /// <param name="elapsedMinutes">실제 경과 시간 (분)</param>
        /// <param name="clampedMinutes">클램프된 경과 시간 (분)</param>
        /// <param name="gold">골드 보상량</param>
        /// <param name="drops">드롭 아이템 목록</param>
        public OfflineRewardResult(
            double elapsedMinutes,
            double clampedMinutes,
            BigNumber gold,
            IReadOnlyList<OfflineDropResult> drops)
        {
            ElapsedMinutes = elapsedMinutes;
            ClampedMinutes = clampedMinutes;
            Gold = gold;
            Drops = drops;
            IsDoubled = false;
        }

        /// <summary>
        /// 광고 배율을 적용한 새로운 결과를 반환한다.
        /// </summary>
        /// <param name="multiplier">배율 (기본 2.0)</param>
        /// <returns>배율 적용된 새 결과</returns>
        public OfflineRewardResult WithMultiplier(float multiplier)
        {
            var doubledDrops = new List<OfflineDropResult>();
            foreach (var drop in Drops)
            {
                doubledDrops.Add(new OfflineDropResult
                {
                    DropType = drop.DropType,
                    Grade = drop.Grade,
                    Count = (int)(drop.Count * multiplier)
                });
            }

            return new OfflineRewardResult(
                ElapsedMinutes,
                ClampedMinutes,
                BigNumber.Floor(Gold * multiplier),
                doubledDrops)
            {
                IsDoubled = true
            };
        }
    }
}
