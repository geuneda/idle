using System;
using System.Collections.Generic;
using Geuneda.Services;
using IdleRPG.Mine;

namespace IdleRPG.Core
{
    /// <summary>
    /// 광산 진행 상태의 저장/로드를 담당한다.
    /// </summary>
    public class MineSaveCollector : ISaveDataCollector
    {
        private readonly MineModel _mineModel;
        private Action _markDirty;

        /// <summary>
        /// <see cref="MineSaveCollector"/>를 생성한다.
        /// </summary>
        /// <param name="mineModel">광산 진행 상태 모델</param>
        public MineSaveCollector(MineModel mineModel)
        {
            _mineModel = mineModel;
        }

        /// <inheritdoc />
        public void ExtractTo(GameSaveData saveData)
        {
            var data = new MineSaveData
            {
                CurrentFloor = _mineModel.CurrentFloor,
                ChestIndex = _mineModel.ChestIndex,
                ChestGrade = _mineModel.ChestGrade,
                ChestUpgradesDone = _mineModel.ChestUpgradesDone,
                ChestCompleted = _mineModel.ChestCompleted,
                ChestRewardClaimed = _mineModel.ChestRewardClaimed,
                ClaimedProgressFloors = _mineModel.ClaimedProgressFloors != null
                    ? new List<int>(_mineModel.ClaimedProgressFloors)
                    : new List<int>(),
                LastPickaxeRechargeTimestamp = _mineModel.LastPickaxeRechargeTimestamp
            };

            if (_mineModel.Board != null)
            {
                int len = _mineModel.Board.Count;
                data.BoardStates = new int[len];
                data.BoardRewardTypes = new int[len];
                data.BoardRewardAmounts = new int[len];

                for (int i = 0; i < len; i++)
                {
                    data.BoardStates[i] = (int)_mineModel.Board[i].State;
                    data.BoardRewardTypes[i] = (int)_mineModel.Board[i].RewardType;
                    data.BoardRewardAmounts[i] = _mineModel.Board[i].RewardAmount;
                }
            }

            if (_mineModel.ChestUpgradeResults != null)
            {
                int upgradeLen = _mineModel.ChestUpgradeResults.Count;
                data.ChestUpgradeResults = new bool[upgradeLen];
                for (int i = 0; i < upgradeLen; i++)
                    data.ChestUpgradeResults[i] = _mineModel.ChestUpgradeResults[i];
            }

            saveData.Mine = data;
        }

        /// <inheritdoc />
        public void ApplyFrom(GameSaveData saveData)
        {
            var data = saveData.Mine;
            if (data == null) return;

            MineBlock[] board = null;
            if (data.BoardStates != null && data.BoardStates.Length > 0)
            {
                int len = data.BoardStates.Length;
                board = new MineBlock[len];
                for (int i = 0; i < len; i++)
                {
                    board[i] = new MineBlock(
                        (MineBlockState)data.BoardStates[i],
                        (MineBlockRewardType)data.BoardRewardTypes[i],
                        data.BoardRewardAmounts[i]);
                }
            }

            _mineModel.RestoreFromSave(
                data.CurrentFloor,
                board,
                data.ChestIndex,
                data.ChestGrade,
                data.ChestUpgradesDone,
                data.ChestUpgradeResults != null
                    ? (bool[])data.ChestUpgradeResults.Clone()
                    : new bool[4],
                data.ChestCompleted,
                data.ChestRewardClaimed,
                data.ClaimedProgressFloors != null
                    ? new HashSet<int>(data.ClaimedProgressFloors)
                    : new HashSet<int>(),
                data.LastPickaxeRechargeTimestamp);
        }

        /// <inheritdoc />
        public void SubscribeDirtyEvents(IMessageBrokerService broker, Action markDirty)
        {
            _markDirty = markDirty;
            broker.Subscribe<MineStateChangedMessage>(OnStateChanged);
        }

        /// <inheritdoc />
        public void UnsubscribeDirtyEvents(IMessageBrokerService broker)
        {
            broker.Unsubscribe<MineStateChangedMessage>(this);
            _markDirty = null;
        }

        private void OnStateChanged(MineStateChangedMessage msg) => _markDirty?.Invoke();
    }
}
