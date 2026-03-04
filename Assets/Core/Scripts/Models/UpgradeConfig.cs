using System;
using System.Collections.Generic;

namespace IdleRPG.Core
{
    /// <summary>
    /// 수집형 아이템 강화 비용 설정. 레벨별 필요 소재 수를 정의한다.
    /// 장비, 스킬, 펫이 공유하는 공통 설정이다.
    /// </summary>
    [Serializable]
    public class UpgradeConfig
    {
        /// <summary>레벨별 필요 소재 수. 인덱스 0은 레벨 1->2 강화 비용</summary>
        public List<int> RequiredCountPerLevel = new()
        {
            2, 3, 4, 6, 7, 9, 11, 14, 17, 20,
            24, 28, 33, 38, 44, 50, 57, 65, 73, 82,
            92, 103, 115, 128, 142, 157, 173, 190, 208
        };

        /// <summary>RequiredCountPerLevel 범위 초과 시 기준값</summary>
        public double OverflowBase = 208;

        /// <summary>초과 레벨당 증가율</summary>
        public double OverflowGrowthRate = 0.12;
    }
}
