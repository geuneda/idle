using System;
using System.Collections.Generic;

namespace IdleRPG.Dungeon
{
    /// <summary>
    /// 던전 진행 상태를 관리하는 POCO 데이터 모델.
    /// 타입별 클리어 레벨과 일일 입장 횟수를 추적한다.
    /// </summary>
    [Serializable]
    public class DungeonModel
    {
        /// <summary>던전 타입별 최고 클리어 레벨. key: DungeonType(int), value: 레벨</summary>
        public Dictionary<int, int> ClearedLevels = new Dictionary<int, int>();

        /// <summary>던전 타입별 오늘 사용한 입장 횟수. key: DungeonType(int), value: 횟수</summary>
        public Dictionary<int, int> DailyUsedEntries = new Dictionary<int, int>();

        /// <summary>마지막 일일 리셋 시각 (UTC 밀리초)</summary>
        public long LastDailyResetTimestamp;

        /// <summary>
        /// 지정 던전 타입의 최고 클리어 레벨을 반환한다.
        /// </summary>
        /// <param name="type">던전 종류</param>
        /// <returns>클리어 레벨. 기록이 없으면 0</returns>
        public int GetClearedLevel(DungeonType type)
        {
            return ClearedLevels.TryGetValue((int)type, out var level) ? level : 0;
        }

        /// <summary>
        /// 지정 던전 타입의 최고 클리어 레벨을 설정한다.
        /// </summary>
        /// <param name="type">던전 종류</param>
        /// <param name="level">설정할 레벨</param>
        public void SetClearedLevel(DungeonType type, int level)
        {
            ClearedLevels[(int)type] = level;
        }

        /// <summary>
        /// 지정 던전 타입의 오늘 사용한 입장 횟수를 반환한다.
        /// </summary>
        /// <param name="type">던전 종류</param>
        /// <returns>사용한 입장 횟수</returns>
        public int GetUsedEntries(DungeonType type)
        {
            return DailyUsedEntries.TryGetValue((int)type, out var count) ? count : 0;
        }

        /// <summary>
        /// 지정 던전 타입의 사용 입장 횟수를 1 증가시킨다.
        /// </summary>
        /// <param name="type">던전 종류</param>
        public void IncrementUsedEntries(DungeonType type)
        {
            int key = (int)type;
            DailyUsedEntries.TryGetValue(key, out var current);
            DailyUsedEntries[key] = current + 1;
        }

        /// <summary>
        /// 일일 입장 횟수를 초기화한다.
        /// </summary>
        /// <param name="resetTimestamp">리셋 시각 (UTC 밀리초)</param>
        public void ResetDailyEntries(long resetTimestamp)
        {
            DailyUsedEntries.Clear();
            LastDailyResetTimestamp = resetTimestamp;
        }
    }
}
