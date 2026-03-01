using System;
using System.Collections.Generic;

namespace IdleRPG.Equipment
{
    /// <summary>
    /// 장비 강화에 필요한 소재 개수 설정. 모든 장비가 공유한다.
    /// </summary>
    [Serializable]
    public class EquipmentUpgradeConfig
    {
        /// <summary>
        /// 레벨별 필요 소재 개수 목록. 인덱스 0 = 레벨 1에서 2로 강화 시 필요 개수.
        /// </summary>
        public List<int> RequiredCountPerLevel = new()
        {
            2, 3, 4, 6, 7, 9, 11, 14, 17, 20,
            24, 28, 33, 38, 44, 50, 57, 65, 73, 82,
            92, 103, 115, 128, 142, 157, 173, 190, 208
        };

        /// <summary>목록 초과 시 사용할 기준 비용</summary>
        public double OverflowBase = 208;

        /// <summary>목록 초과 시 레벨당 증가율</summary>
        public double OverflowGrowthRate = 0.12;
    }
}
