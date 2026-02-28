using System;
using IdleRPG.Core;

namespace IdleRPG.Reward
{
    /// <summary>
    /// 적 처치 보상의 기본 설정 데이터.
    /// 보상 계산 로직을 순수 함수로 제공하여 테스트 용이성과 오프라인 보상 재사용을 지원한다.
    /// </summary>
    [Serializable]
    public class RewardConfig
    {
        /// <summary>일반 적 처치 시 기본 골드 보상량</summary>
        public BigNumber BaseGoldPerKill = 10;

        /// <summary>보스 처치 시 골드 배율</summary>
        public float BossGoldMultiplier = 5f;

        /// <summary>스테이지 진행에 따른 골드 증가율. 글로벌 스테이지 인덱스에 곱해진다.</summary>
        public float GoldScalePerStage = 0.12f;

        /// <summary>
        /// 적 처치 시 획득할 골드를 계산한다.
        /// </summary>
        /// <param name="chapter">현재 챕터 번호 (1-based)</param>
        /// <param name="stage">현재 스테이지 번호 (1-based)</param>
        /// <param name="stagesPerChapter">챕터당 스테이지 수</param>
        /// <param name="isBoss">보스 여부</param>
        /// <returns>계산된 골드 보상량</returns>
        public BigNumber CalculateGoldReward(int chapter, int stage, int stagesPerChapter, bool isBoss)
        {
            int globalIndex = (chapter - 1) * stagesPerChapter + (stage - 1);
            BigNumber stageMultiplier = 1.0 + globalIndex * GoldScalePerStage;
            BigNumber gold = BaseGoldPerKill * stageMultiplier;

            if (isBoss)
            {
                gold *= BossGoldMultiplier;
            }

            return BigNumber.Floor(gold);
        }
    }
}
