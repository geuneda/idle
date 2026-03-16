using System;
using System.Collections.Generic;
using IdleRPG.Core;

namespace IdleRPG.Mine
{
    /// <summary>
    /// 광산 설정 데이터의 런타임 컨테이너.
    /// <see cref="BuildLookup"/>을 호출하여 O(1) 조회용 인덱스를 구축한다.
    /// </summary>
    [Serializable]
    public class MineConfigCollection
    {
        /// <summary>층 구간별 블록 가중치 목록</summary>
        public List<MineBlockWeightConfig> BlockWeightEntries = new List<MineBlockWeightConfig>();

        /// <summary>등급별 상자 보상 목록</summary>
        public List<MineChestRewardConfig> ChestRewardEntries = new List<MineChestRewardConfig>();

        /// <summary>층별 진행도 보상 목록</summary>
        public List<MineProgressRewardConfig> ProgressRewardEntries = new List<MineProgressRewardConfig>();

        /// <summary>글로벌 설정</summary>
        public MineSettingsConfig Settings = new MineSettingsConfig();

        private List<int> _sortedFloorStarts;
        private Dictionary<int, MineBlockWeightConfig> _blockWeightLookup;
        private Dictionary<ItemGrade, MineChestRewardConfig> _chestRewardLookup;
        private Dictionary<int, MineProgressRewardConfig> _progressRewardLookup;
        private List<int> _sortedProgressFloors;

        /// <summary>
        /// 룩업 테이블을 구축한다. 역직렬화 후 반드시 호출해야 한다.
        /// </summary>
        public void BuildLookup()
        {
            _blockWeightLookup = new Dictionary<int, MineBlockWeightConfig>();
            _sortedFloorStarts = new List<int>();
            foreach (var entry in BlockWeightEntries)
            {
                _blockWeightLookup[entry.FloorStart] = entry;
                _sortedFloorStarts.Add(entry.FloorStart);
            }
            _sortedFloorStarts.Sort();

            _chestRewardLookup = new Dictionary<ItemGrade, MineChestRewardConfig>();
            foreach (var entry in ChestRewardEntries)
            {
                _chestRewardLookup[entry.Grade] = entry;
            }

            _progressRewardLookup = new Dictionary<int, MineProgressRewardConfig>();
            _sortedProgressFloors = new List<int>();
            foreach (var entry in ProgressRewardEntries)
            {
                _progressRewardLookup[entry.Floor] = entry;
                _sortedProgressFloors.Add(entry.Floor);
            }
            _sortedProgressFloors.Sort();
        }

        /// <summary>
        /// 해당 층에 적용되는 블록 가중치 설정을 반환한다.
        /// 구간 방식: 해당 층 이하의 가장 큰 FloorStart 설정을 적용한다.
        /// </summary>
        public MineBlockWeightConfig GetBlockWeightConfig(int floor)
        {
            if (_sortedFloorStarts == null || _sortedFloorStarts.Count == 0)
                return BlockWeightEntries.Count > 0 ? BlockWeightEntries[0] : null;

            int applicableFloor = _sortedFloorStarts[0];
            for (int i = _sortedFloorStarts.Count - 1; i >= 0; i--)
            {
                if (_sortedFloorStarts[i] <= floor)
                {
                    applicableFloor = _sortedFloorStarts[i];
                    break;
                }
            }

            return _blockWeightLookup.TryGetValue(applicableFloor, out var config) ? config : null;
        }

        /// <summary>
        /// 등급별 상자 보상 설정을 반환한다.
        /// </summary>
        public MineChestRewardConfig GetChestRewardConfig(ItemGrade grade)
        {
            return _chestRewardLookup != null && _chestRewardLookup.TryGetValue(grade, out var config)
                ? config : null;
        }

        /// <summary>
        /// 해당 층의 진행도 보상 설정을 반환한다. 해당 층에 보상이 없으면 null.
        /// </summary>
        public MineProgressRewardConfig GetProgressRewardConfig(int floor)
        {
            return _progressRewardLookup != null && _progressRewardLookup.TryGetValue(floor, out var config)
                ? config : null;
        }

        /// <summary>
        /// 현재 층 이후의 다음 진행도 보상 층을 반환한다. 없으면 -1.
        /// </summary>
        public int GetNextProgressFloor(int currentFloor)
        {
            if (_sortedProgressFloors == null) return -1;
            foreach (var floor in _sortedProgressFloors)
            {
                if (floor > currentFloor) return floor;
            }
            return -1;
        }

        /// <summary>
        /// 현재 층 이하의 이전 진행도 보상 층을 반환한다. 없으면 -1.
        /// </summary>
        public int GetPrevProgressFloor(int currentFloor)
        {
            if (_sortedProgressFloors == null) return -1;
            for (int i = _sortedProgressFloors.Count - 1; i >= 0; i--)
            {
                if (_sortedProgressFloors[i] <= currentFloor) return _sortedProgressFloors[i];
            }
            return -1;
        }

        private static readonly IReadOnlyList<int> EmptyFloors = new List<int>();

        /// <summary>
        /// 전체 진행도 보상 층 목록을 반환한다.
        /// </summary>
        public IReadOnlyList<int> GetAllProgressFloors()
        {
            return _sortedProgressFloors ?? EmptyFloors;
        }
    }
}
