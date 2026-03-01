using System;
using System.Collections.Generic;
using IdleRPG.Core;
using IdleRPG.Reward;

namespace IdleRPG.OfflineReward
{
    /// <summary>
    /// 오프라인 보상 설정 데이터. 골드 수익률, 최대 시간, 드롭 테이블을 정의한다.
    /// </summary>
    [Serializable]
    public class OfflineRewardConfig
    {
        /// <summary>분당 기본 골드 수익 배율. 스테이지 킬 골드에 곱해진다.</summary>
        public float GoldPerMinuteMultiplier = 10f;

        /// <summary>기본 최대 오프라인 시간 (시간 단위)</summary>
        public float DefaultMaxOfflineHours = 12f;

        /// <summary>오프라인 보상 표시 최소 경과 시간 (분 단위). 이하면 무시한다.</summary>
        public float MinOfflineMinutes = 1f;

        /// <summary>광고 시청 시 보상 배율</summary>
        public float AdMultiplier = 2f;

        /// <summary>스테이지 범위별 드롭 테이블 목록</summary>
        public List<OfflineDropTableEntry> DropTables = new List<OfflineDropTableEntry>();

        /// <summary>
        /// 오프라인 동안 획득할 골드를 계산한다.
        /// </summary>
        /// <param name="elapsedMinutes">경과 시간 (분)</param>
        /// <param name="rewardConfig">적 처치 보상 설정</param>
        /// <param name="chapter">현재 챕터 (1-based)</param>
        /// <param name="stage">현재 스테이지 (1-based)</param>
        /// <param name="stagesPerChapter">챕터당 스테이지 수</param>
        /// <returns>오프라인 골드 보상량</returns>
        public BigNumber CalculateOfflineGold(
            double elapsedMinutes,
            RewardConfig rewardConfig,
            int chapter,
            int stage,
            int stagesPerChapter)
        {
            BigNumber baseGoldPerKill = rewardConfig.CalculateGoldReward(
                chapter, stage, stagesPerChapter, false);
            BigNumber totalGold = baseGoldPerKill * GoldPerMinuteMultiplier * elapsedMinutes;
            return BigNumber.Floor(totalGold);
        }

        /// <summary>
        /// 오프라인 동안 드롭된 아이템 목록을 계산한다.
        /// 기대값 기반으로 계산하며, 난수 생성기로 변동을 추가한다.
        /// </summary>
        /// <param name="elapsedMinutes">경과 시간 (분)</param>
        /// <param name="chapter">현재 챕터 (1-based)</param>
        /// <param name="stage">현재 스테이지 (1-based)</param>
        /// <param name="stagesPerChapter">챕터당 스테이지 수</param>
        /// <param name="random">난수 생성기 (테스트용 시드 고정 가능)</param>
        /// <returns>드롭된 아이템 목록</returns>
        public List<OfflineDropResult> CalculateDrops(
            double elapsedMinutes,
            int chapter,
            int stage,
            int stagesPerChapter,
            Random random)
        {
            var results = new List<OfflineDropResult>();

            int globalStageIndex = (chapter - 1) * stagesPerChapter + (stage - 1);
            var dropTable = FindDropTable(globalStageIndex);
            if (dropTable == null)
            {
                return results;
            }

            foreach (var entry in dropTable.DropEntries)
            {
                double expectedCount = elapsedMinutes * entry.DropChancePerMinute;
                if (expectedCount < 0.01)
                {
                    continue;
                }

                double variance = 0.2;
                double minCount = expectedCount * (1.0 - variance);
                double maxCount = expectedCount * (1.0 + variance);
                double actualCount = minCount + random.NextDouble() * (maxCount - minCount);
                int count = Math.Max(0, (int)Math.Round(actualCount));

                if (count > 0)
                {
                    results.Add(new OfflineDropResult
                    {
                        DropType = entry.DropType,
                        Grade = entry.Grade,
                        Count = count
                    });
                }
            }

            return results;
        }

        /// <summary>
        /// 주어진 글로벌 스테이지 인덱스에 해당하는 드롭 테이블을 찾는다.
        /// </summary>
        /// <param name="globalStageIndex">글로벌 스테이지 인덱스</param>
        /// <returns>해당 드롭 테이블. 없으면 null.</returns>
        public OfflineDropTableEntry FindDropTable(int globalStageIndex)
        {
            for (int i = 0; i < DropTables.Count; i++)
            {
                var table = DropTables[i];
                if (globalStageIndex >= table.MinGlobalStageIndex &&
                    globalStageIndex <= table.MaxGlobalStageIndex)
                {
                    return table;
                }
            }

            return null;
        }
    }
}
