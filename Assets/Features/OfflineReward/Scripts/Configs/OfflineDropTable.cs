using System;
using System.Collections.Generic;

namespace IdleRPG.OfflineReward
{
    /// <summary>
    /// 오프라인 보상으로 획득 가능한 아이템 종류.
    /// </summary>
    public enum OfflineDropType
    {
        /// <summary>펫</summary>
        Pet = 0,

        /// <summary>장비</summary>
        Equipment = 1,

        /// <summary>스킬</summary>
        Skill = 2
    }

    /// <summary>
    /// 오프라인 드롭 아이템 등급.
    /// </summary>
    public enum OfflineDropGrade
    {
        /// <summary>일반</summary>
        Common = 0,

        /// <summary>고급</summary>
        Uncommon = 1,

        /// <summary>희귀</summary>
        Rare = 2,

        /// <summary>영웅</summary>
        Epic = 3,

        /// <summary>전설</summary>
        Legendary = 4
    }

    /// <summary>
    /// 단일 드롭 확률 엔트리. 특정 아이템 종류와 등급의 분당 드롭 확률을 정의한다.
    /// </summary>
    [Serializable]
    public class OfflineDropEntry
    {
        /// <summary>아이템 종류</summary>
        public OfflineDropType DropType;

        /// <summary>아이템 등급</summary>
        public OfflineDropGrade Grade;

        /// <summary>분당 드롭 확률 (0.0~1.0). 매 분마다 이 확률로 드롭 판정한다.</summary>
        public float DropChancePerMinute;
    }

    /// <summary>
    /// 스테이지 범위별 드롭 테이블. 특정 글로벌 스테이지 범위에서 적용되는 드롭 확률 목록이다.
    /// </summary>
    [Serializable]
    public class OfflineDropTableEntry
    {
        /// <summary>적용 시작 글로벌 스테이지 인덱스 (inclusive)</summary>
        public int MinGlobalStageIndex;

        /// <summary>적용 종료 글로벌 스테이지 인덱스 (inclusive)</summary>
        public int MaxGlobalStageIndex;

        /// <summary>이 스테이지 범위에서 적용되는 드롭 확률 목록</summary>
        public List<OfflineDropEntry> DropEntries = new List<OfflineDropEntry>();
    }
}
