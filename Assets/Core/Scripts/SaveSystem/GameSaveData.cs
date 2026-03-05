using System;
using System.Collections.Generic;

namespace IdleRPG.Core
{
    /// <summary>
    /// 장비 보유/장착 상태 DTO.
    /// </summary>
    [Serializable]
    public class EquipmentSaveData
    {
        /// <summary>장비ID(int) -> 보유 수량 및 레벨 매핑</summary>
        public Dictionary<int, EquipmentItemSaveEntry> Items = new Dictionary<int, EquipmentItemSaveEntry>();

        /// <summary>슬롯 타입(int) -> 장착된 장비 ID 매핑. 미장착 슬롯은 키가 없다</summary>
        public Dictionary<int, int> Equipped = new Dictionary<int, int>();
    }

    /// <summary>
    /// 개별 장비 아이템의 저장 항목.
    /// </summary>
    [Serializable]
    public class EquipmentItemSaveEntry
    {
        /// <summary>보유 수량</summary>
        public int OwnedCount;

        /// <summary>강화 레벨</summary>
        public int Level;
    }

    /// <summary>
    /// 펫 보유/장착 상태 DTO.
    /// </summary>
    [Serializable]
    public class PetSaveData
    {
        /// <summary>펫ID(int) -> 보유 수량 및 레벨 매핑</summary>
        public Dictionary<int, PetItemSaveEntry> Items = new Dictionary<int, PetItemSaveEntry>();

        /// <summary>슬롯 인덱스(int) -> 장착된 펫 ID 매핑. 미장착 슬롯은 키가 없다</summary>
        public Dictionary<int, int> Equipped = new Dictionary<int, int>();
    }

    /// <summary>
    /// 개별 펫 아이템의 저장 항목.
    /// </summary>
    [Serializable]
    public class PetItemSaveEntry
    {
        /// <summary>보유 수량</summary>
        public int OwnedCount;

        /// <summary>강화 레벨</summary>
        public int Level;
    }

    /// <summary>
    /// 스킬 보유/장착 상태 DTO.
    /// </summary>
    [Serializable]
    public class SkillSaveData
    {
        /// <summary>스킬ID(int) -> 보유 수량 및 레벨 매핑</summary>
        public Dictionary<int, SkillItemSaveEntry> Items = new Dictionary<int, SkillItemSaveEntry>();

        /// <summary>슬롯 인덱스(int) -> 장착된 스킬 ID 매핑. 미장착 슬롯은 키가 없다</summary>
        public Dictionary<int, int> Equipped = new Dictionary<int, int>();
    }

    /// <summary>
    /// 개별 스킬 아이템의 저장 항목.
    /// </summary>
    [Serializable]
    public class SkillItemSaveEntry
    {
        /// <summary>보유 수량</summary>
        public int OwnedCount;

        /// <summary>강화 레벨</summary>
        public int Level;
    }

    /// <summary>
    /// 게임 전체 저장 데이터의 최상위 DTO.
    /// 단일 객체로 원자적 저장을 보장하여 데이터 일관성을 유지한다.
    /// </summary>
    [Serializable]
    public class GameSaveData
    {
        /// <summary>저장 데이터 포맷 버전. 향후 마이그레이션에 사용한다.</summary>
        public int SaveVersion = 1;

        /// <summary>마지막 저장 시각 (Unix 밀리초). 오프라인 보상 계산에 사용한다.</summary>
        public long LastSaveTimestamp;

        /// <summary>스테이지 진행 상태</summary>
        public StageSaveData Stage;

        /// <summary>재화 보유량</summary>
        public CurrencySaveData Currency;

        /// <summary>영웅 성장 레벨</summary>
        public GrowthSaveData Growth;

        /// <summary>장비 보유 및 장착 상태</summary>
        public EquipmentSaveData Equipment;

        /// <summary>스킬 보유 및 장착 상태</summary>
        public SkillSaveData Skill;

        /// <summary>펫 보유 및 장착 상태</summary>
        public PetSaveData Pet;

        /// <summary>던전 진행 상태</summary>
        public DungeonSaveData Dungeon;
    }

    /// <summary>
    /// 스테이지 진행 상태 DTO.
    /// </summary>
    [Serializable]
    public class StageSaveData
    {
        /// <summary>현재 챕터 번호 (1-based)</summary>
        public int CurrentChapter = 1;

        /// <summary>현재 스테이지 번호 (1-based)</summary>
        public int CurrentStage = 1;

        /// <summary>현재 웨이브 인덱스 (0-based)</summary>
        public int CurrentWave;

        /// <summary>보스 자동 도전 활성화 여부</summary>
        public bool IsBossAutoChallenge = true;

        /// <summary>도달한 최고 챕터 번호</summary>
        public int HighestChapter = 1;

        /// <summary>도달한 최고 스테이지 번호</summary>
        public int HighestStage = 1;
    }

    /// <summary>
    /// 재화 보유량 DTO. 키는 <c>CurrencyType</c>의 int 캐스트 값,
    /// 값은 <see cref="BigNumber.ToString()"/> 형식 문자열이다.
    /// </summary>
    [Serializable]
    public class CurrencySaveData
    {
        /// <summary>재화 종류(int) → 보유량(BigNumber 문자열) 매핑</summary>
        public Dictionary<int, string> Currencies = new Dictionary<int, string>();
    }

    /// <summary>
    /// 던전 진행 상태 DTO.
    /// </summary>
    [Serializable]
    public class DungeonSaveData
    {
        /// <summary>던전 타입(int) -> 최고 클리어 레벨 매핑</summary>
        public Dictionary<int, int> ClearedLevels = new Dictionary<int, int>();

        /// <summary>던전 타입(int) -> 오늘 사용한 입장 횟수 매핑</summary>
        public Dictionary<int, int> DailyUsedEntries = new Dictionary<int, int>();

        /// <summary>마지막 일일 리셋 시각 (UTC 밀리초)</summary>
        public long LastDailyResetTimestamp;
    }

    /// <summary>
    /// 영웅 성장 레벨 DTO. 키는 <c>HeroStatType</c>의 int 캐스트 값이다.
    /// </summary>
    [Serializable]
    public class GrowthSaveData
    {
        /// <summary>스탯 유형(int) → 레벨 매핑</summary>
        public Dictionary<int, int> StatLevels = new Dictionary<int, int>();
    }
}
