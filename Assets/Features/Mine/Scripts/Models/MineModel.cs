using System;
using System.Collections.Generic;

namespace IdleRPG.Mine
{
    /// <summary>
    /// 광산 컨텐츠 진행 상태를 관리하는 POCO 모델.
    /// </summary>
    [Serializable]
    public class MineModel
    {
        private int _currentFloor = 1;
        private MineBlock[] _board;
        private int _chestIndex = -1;
        private int _chestGrade = 1;
        private int _chestUpgradesDone;
        private bool[] _chestUpgradeResults;
        private bool _chestCompleted;
        private bool _chestRewardClaimed;
        private HashSet<int> _claimedProgressFloors = new HashSet<int>();
        private long _lastPickaxeRechargeTimestamp;

        /// <summary>현재 층 번호 (1-based)</summary>
        public int CurrentFloor => _currentFloor;

        /// <summary>6x6 보드판 블록 배열 (row-major, 길이 36). 읽기 전용.</summary>
        public IReadOnlyList<MineBlock> Board => _board;

        /// <summary>보물상자 위치 인덱스 (-1이면 없음)</summary>
        public int ChestIndex => _chestIndex;

        /// <summary>보물상자 현재 등급 (ItemGrade as int, Advanced=1 시작)</summary>
        public int ChestGrade => _chestGrade;

        /// <summary>보물상자 강화 완료 횟수 (0~4)</summary>
        public int ChestUpgradesDone => _chestUpgradesDone;

        /// <summary>보물상자 강화 결과 배열 (길이 4, true=성공). 읽기 전용.</summary>
        public IReadOnlyList<bool> ChestUpgradeResults => _chestUpgradeResults;

        /// <summary>보물상자 강화 완료 여부</summary>
        public bool ChestCompleted => _chestCompleted;

        /// <summary>보물상자 보상 수령 여부</summary>
        public bool ChestRewardClaimed => _chestRewardClaimed;

        /// <summary>수령 완료한 진행도 보상 층 번호 집합. 읽기 전용.</summary>
        public IReadOnlyCollection<int> ClaimedProgressFloors => _claimedProgressFloors;

        /// <summary>마지막 곡괭이 충전 시각 (UTC 밀리초)</summary>
        public long LastPickaxeRechargeTimestamp => _lastPickaxeRechargeTimestamp;

        public const int BoardWidth = 6;
        public const int BoardHeight = 6;
        public const int BoardSize = BoardWidth * BoardHeight;

        /// <summary>
        /// (x, y) 좌표로 블록 인덱스를 구한다.
        /// </summary>
        public static int GetBlockIndex(int x, int y) => y * BoardWidth + x;

        /// <summary>
        /// 블록 인덱스로 (x, y) 좌표를 구한다.
        /// </summary>
        public static (int x, int y) GetBlockPosition(int index) => (index % BoardWidth, index / BoardWidth);

        /// <summary>
        /// 지정한 좌표의 블록을 반환한다.
        /// </summary>
        public MineBlock GetBlock(int x, int y) => _board[GetBlockIndex(x, y)];

        /// <summary>보드 블록을 설정한다.</summary>
        internal void SetBoardBlock(int index, MineBlock block) => _board[index] = block;

        /// <summary>현재 층을 설정한다.</summary>
        internal void SetCurrentFloor(int floor) => _currentFloor = floor;

        /// <summary>보물상자 인덱스를 설정한다.</summary>
        internal void SetChestIndex(int index) => _chestIndex = index;

        /// <summary>보물상자 등급을 설정한다.</summary>
        internal void SetChestGrade(int grade) => _chestGrade = grade;

        /// <summary>보물상자 강화 완료 횟수를 설정한다.</summary>
        internal void SetChestUpgradesDone(int count) => _chestUpgradesDone = count;

        /// <summary>보물상자 강화 결과를 기록한다.</summary>
        internal void SetChestUpgradeResult(int index, bool success) => _chestUpgradeResults[index] = success;

        /// <summary>보물상자 강화 완료 여부를 설정한다.</summary>
        internal void SetChestCompleted(bool completed) => _chestCompleted = completed;

        /// <summary>보물상자 보상 수령 여부를 설정한다.</summary>
        internal void SetChestRewardClaimed(bool claimed) => _chestRewardClaimed = claimed;

        /// <summary>진행도 보상 수령 층을 추가한다.</summary>
        internal void AddClaimedProgressFloor(int floor) => _claimedProgressFloors.Add(floor);

        /// <summary>진행도 보상 수령 층 포함 여부를 확인한다.</summary>
        internal bool HasClaimedProgressFloor(int floor) => _claimedProgressFloors.Contains(floor);

        /// <summary>마지막 곡괭이 충전 시각을 설정한다.</summary>
        internal void SetLastPickaxeRechargeTimestamp(long timestamp) => _lastPickaxeRechargeTimestamp = timestamp;

        /// <summary>
        /// 보드를 초기 상태로 리셋한다. 모든 블록을 Hidden으로 설정한다.
        /// </summary>
        internal void ResetBoard()
        {
            _board = new MineBlock[BoardSize];
            _chestIndex = -1;
            _chestGrade = 1;
            _chestUpgradesDone = 0;
            _chestUpgradeResults = new bool[4];
            _chestCompleted = false;
            _chestRewardClaimed = false;
        }

        /// <summary>
        /// 저장 데이터에서 상태를 복원한다. 저장 시스템에서만 호출해야 한다.
        /// </summary>
        public void RestoreFromSave(
            int currentFloor,
            MineBlock[] board,
            int chestIndex,
            int chestGrade,
            int chestUpgradesDone,
            bool[] chestUpgradeResults,
            bool chestCompleted,
            bool chestRewardClaimed,
            HashSet<int> claimedProgressFloors,
            long lastPickaxeRechargeTimestamp)
        {
            _currentFloor = currentFloor;
            _board = board;
            _chestIndex = chestIndex;
            _chestGrade = chestGrade;
            _chestUpgradesDone = chestUpgradesDone;
            _chestUpgradeResults = chestUpgradeResults ?? new bool[4];
            _chestCompleted = chestCompleted;
            _chestRewardClaimed = chestRewardClaimed;
            _claimedProgressFloors = claimedProgressFloors ?? new HashSet<int>();
            _lastPickaxeRechargeTimestamp = lastPickaxeRechargeTimestamp;
        }

        /// <summary>
        /// Hidden 상태인 블록 수를 반환한다.
        /// </summary>
        public int CountHiddenBlocks()
        {
            int count = 0;
            if (_board == null) return count;
            for (int i = 0; i < _board.Length; i++)
            {
                if (_board[i].State == MineBlockState.Hidden) count++;
            }
            return count;
        }

        /// <summary>
        /// 파괴되었지만 아직 수집하지 않은 보상 목록을 반환한다.
        /// </summary>
        public List<(int index, MineBlockRewardType type, int amount)> GetUncollectedRewards()
        {
            var results = new List<(int, MineBlockRewardType, int)>();
            if (_board == null) return results;
            for (int i = 0; i < _board.Length; i++)
            {
                if (_board[i].State == MineBlockState.Revealed &&
                    _board[i].RewardType != MineBlockRewardType.Empty &&
                    _board[i].RewardType != MineBlockRewardType.TreasureChest)
                {
                    results.Add((i, _board[i].RewardType, _board[i].RewardAmount));
                }
            }
            return results;
        }
    }
}
