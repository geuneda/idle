using System;
using System.Collections.Generic;
using Geuneda.Services;
using IdleRPG.Core;
using IdleRPG.Economy;

namespace IdleRPG.Mine
{
    /// <summary>
    /// 광산 컨텐츠 서비스 구현.
    /// </summary>
    public class MineService : IMineService, IDisposable
    {
        private readonly MineConfigCollection _config;
        private readonly MineModel _model;
        private readonly ICurrencyService _currencyService;
        private readonly IMessageBrokerService _messageBroker;
        private readonly Random _random = new Random();
        private readonly List<int> _reusableIndices = new List<int>(MineModel.BoardSize);
        private readonly List<int> _reusableAffected = new List<int>(11);
        private readonly List<int> _reusableDestroyed = new List<int>(MineModel.BoardSize);

        private readonly MineBoardGenerator _boardGenerator;
        private readonly MinePickaxeRecharger _pickaxeRecharger;
        private readonly MineChestHandler _chestHandler;

        /// <summary>광산 모델</summary>
        public MineModel Model => _model;

        /// <summary>광산 설정</summary>
        public MineConfigCollection Config => _config;

        /// <summary>현재 곡괭이 수량</summary>
        public int CurrentPickaxeCount => (int)_currencyService.GetAmount(CurrencyType.Pickaxe).ToDouble();

        /// <summary>최대 곡괭이 수량</summary>
        public int MaxPickaxeCount => _config.Settings.MaxPickaxe;

        /// <summary>곡괭이 최대치 여부</summary>
        public bool IsPickaxeMaxed => CurrentPickaxeCount >= MaxPickaxeCount;

        /// <summary>다음 충전까지 남은 시간</summary>
        public TimeSpan NextRechargeTime => _pickaxeRecharger.NextRechargeTime;

        /// <summary>보물상자가 발견(Revealed)되었는지 여부</summary>
        public bool IsChestRevealed =>
            _model.ChestIndex >= 0 &&
            _model.Board != null &&
            _model.Board[_model.ChestIndex].State == MineBlockState.Revealed;

        /// <summary>보물상자 강화가 완료되었는지 여부</summary>
        public bool IsChestCompleted => _model.ChestCompleted;

        /// <summary>다음 층으로 이동 가능한지 여부</summary>
        public bool CanAdvanceFloor => _model.ChestRewardClaimed;

        /// <summary>빠른 클리어 가능 여부</summary>
        public bool CanQuickClear => CurrentPickaxeCount > 0;

        /// <summary>
        /// MineService를 생성한다.
        /// </summary>
        public MineService(
            MineConfigCollection config,
            MineModel model,
            ICurrencyService currencyService,
            ITimeService timeService,
            ITickService tickService,
            IMessageBrokerService messageBroker)
        {
            _config = config;
            _model = model;
            _currencyService = currencyService;
            _messageBroker = messageBroker;

            _boardGenerator = new MineBoardGenerator(_random);
            _chestHandler = new MineChestHandler(_random, config.Settings);
            _pickaxeRecharger = new MinePickaxeRecharger(
                model, config.Settings, currencyService,
                timeService, tickService, messageBroker);

            if (_model.Board == null || _model.Board.Count == 0)
            {
                InitializeBoard();
            }
        }

        /// <summary>
        /// 리소스를 해제한다.
        /// </summary>
        public void Dispose()
        {
            _pickaxeRecharger.Dispose();
        }

        /// <summary>보드를 초기화한다 (현재 층 기준).</summary>
        public void InitializeBoard()
        {
            var weightConfig = _config.GetBlockWeightConfig(_model.CurrentFloor);
            _boardGenerator.GenerateBoard(_model, weightConfig);
            _messageBroker.Publish(new MineStateChangedMessage());
        }

        /// <summary>해당 도구를 사용할 수 있는지 확인한다.</summary>
        public bool CanUseTool(MineToolType tool)
        {
            var currencyType = GetToolCurrencyType(tool);
            return _currencyService.GetAmount(currencyType) >= BigNumber.One;
        }

        /// <summary>도구 사용 시 영향받는 블록 인덱스를 반환한다 (재사용 리스트).</summary>
        public List<int> GetAffectedBlocks(MineToolType tool, int centerIndex)
        {
            _reusableAffected.Clear();
            var (cx, cy) = MineModel.GetBlockPosition(centerIndex);

            switch (tool)
            {
                case MineToolType.Pickaxe:
                    _reusableAffected.Add(centerIndex);
                    break;

                case MineToolType.Bomb:
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            int nx = cx + dx;
                            int ny = cy + dy;
                            if (nx >= 0 && nx < MineModel.BoardWidth && ny >= 0 && ny < MineModel.BoardHeight)
                                _reusableAffected.Add(MineModel.GetBlockIndex(nx, ny));
                        }
                    }
                    break;

                case MineToolType.Dynamite:
                    for (int x = 0; x < MineModel.BoardWidth; x++)
                        _reusableAffected.Add(MineModel.GetBlockIndex(x, cy));
                    for (int y = 0; y < MineModel.BoardHeight; y++)
                    {
                        if (y != cy)
                            _reusableAffected.Add(MineModel.GetBlockIndex(cx, y));
                    }
                    break;
            }

            return _reusableAffected;
        }

        /// <summary>도구를 사용하여 블록을 파괴한다.</summary>
        public void UseTool(MineToolType tool, int centerIndex)
        {
            if (!CanUseTool(tool)) return;

            var currencyType = GetToolCurrencyType(tool);
            if (!_currencyService.TrySpend(currencyType, BigNumber.One)) return;

            var affected = GetAffectedBlocks(tool, centerIndex);
            _reusableDestroyed.Clear();

            foreach (int idx in affected)
            {
                if (_model.Board[idx].State == MineBlockState.Hidden)
                {
                    var block = _model.Board[idx];
                    var newState = block.RewardType == MineBlockRewardType.Empty
                        ? MineBlockState.Collected
                        : MineBlockState.Revealed;
                    _model.SetBoardBlock(idx, block.WithState(newState));
                    _reusableDestroyed.Add(idx);

                    if (block.RewardType == MineBlockRewardType.TreasureChest)
                    {
                        _messageBroker.Publish(new MineChestRevealedMessage { ChestIndex = idx });
                    }
                }
            }

            if (_reusableDestroyed.Count > 0)
            {
                _messageBroker.Publish(new MineBlocksDestroyedMessage
                {
                    DestroyedIndices = _reusableDestroyed.ToArray(),
                    ToolUsed = tool
                });
                _messageBroker.Publish(new MineStateChangedMessage());
            }
        }

        /// <summary>블록의 보상을 수집한다.</summary>
        public void CollectReward(int blockIndex)
        {
            if (_model.Board[blockIndex].State != MineBlockState.Revealed) return;

            var block = _model.Board[blockIndex];
            if (block.RewardType == MineBlockRewardType.TreasureChest) return;

            GrantBlockReward(block.RewardType, block.RewardAmount);

            _model.SetBoardBlock(blockIndex, block.WithState(MineBlockState.Collected));

            _messageBroker.Publish(new MineRewardCollectedMessage
            {
                BlockIndex = blockIndex,
                RewardType = block.RewardType,
                Amount = block.RewardAmount
            });
            _messageBroker.Publish(new MineStateChangedMessage());
        }

        /// <summary>미수령 보상을 일괄 수집한다.</summary>
        public void CollectAllUncollected()
        {
            if (CollectAllUncollectedCore())
            {
                _messageBroker.Publish(new MineStateChangedMessage());
            }
        }

        /// <summary>보물상자 현재 등급을 반환한다.</summary>
        public ItemGrade GetChestCurrentGrade() => _chestHandler.GetCurrentGrade(_model);

        /// <summary>보물상자 강화를 시도한다. 성공/실패를 반환한다.</summary>
        public bool TryUpgradeChest()
        {
            if (_model.ChestCompleted) return false;
            if (_model.ChestUpgradesDone >= _chestHandler.UpgradeSlots) return false;

            int slotIndex = _model.ChestUpgradesDone;
            bool success = _chestHandler.PerformUpgrade(_model, slotIndex);

            _messageBroker.Publish(new MineChestUpgradeAttemptedMessage
            {
                SlotIndex = slotIndex,
                Success = success,
                ResultGrade = GetChestCurrentGrade()
            });
            _messageBroker.Publish(new MineStateChangedMessage());

            return success;
        }

        /// <summary>보물상자 보상을 수령한다.</summary>
        public void ClaimChestReward()
        {
            if (!_model.ChestCompleted || _model.ChestRewardClaimed) return;

            var grade = GetChestCurrentGrade();
            var rewardConfig = _config.GetChestRewardConfig(grade);
            GrantChestReward(rewardConfig);

            _model.SetChestRewardClaimed(true);

            if (_model.ChestIndex >= 0)
            {
                _model.SetBoardBlock(_model.ChestIndex,
                    _model.Board[_model.ChestIndex].WithState(MineBlockState.Staircase));
            }

            _messageBroker.Publish(new MineChestRewardClaimedMessage { FinalGrade = grade });
            _messageBroker.Publish(new MineStateChangedMessage());
        }

        /// <summary>다음 층으로 이동한다.</summary>
        public void AdvanceFloor()
        {
            if (!CanAdvanceFloor) return;

            CollectAllUncollected();

            int previousFloor = _model.CurrentFloor;
            if (_model.CurrentFloor < _config.Settings.MaxFloor)
            {
                _model.SetCurrentFloor(_model.CurrentFloor + 1);
            }

            InitializeBoard();

            _messageBroker.Publish(new MineFloorAdvancedMessage
            {
                PreviousFloor = previousFloor,
                NewFloor = _model.CurrentFloor
            });
        }

        /// <summary>진행도 보상을 수령할 수 있는지 확인한다.</summary>
        public bool CanClaimProgressReward(int floor)
        {
            if (_model.CurrentFloor < floor) return false;
            if (_model.HasClaimedProgressFloor(floor)) return false;
            return _config.GetProgressRewardConfig(floor) != null;
        }

        /// <summary>진행도 보상을 수령한다.</summary>
        public void ClaimProgressReward(int floor)
        {
            if (!CanClaimProgressReward(floor)) return;

            var rewardConfig = _config.GetProgressRewardConfig(floor);
            if (rewardConfig == null) return;

            _currencyService.Add(rewardConfig.RewardType, new BigNumber(rewardConfig.RewardAmount, 0));
            _model.AddClaimedProgressFloor(floor);

            _messageBroker.Publish(new MineProgressRewardClaimedMessage
            {
                Floor = floor,
                RewardType = rewardConfig.RewardType,
                RewardAmount = new BigNumber(rewardConfig.RewardAmount, 0)
            });
            _messageBroker.Publish(new MineStateChangedMessage());
        }

        /// <summary>다음 미수령 진행도 보상 층을 반환한다.</summary>
        public int GetNextUnclaimedProgressFloor()
        {
            var allFloors = _config.GetAllProgressFloors();
            for (int i = 0; i < allFloors.Count; i++)
            {
                int floor = allFloors[i];
                if (floor <= _model.CurrentFloor && !_model.HasClaimedProgressFloor(floor))
                    return floor;
            }
            return _config.GetNextProgressFloor(_model.CurrentFloor);
        }

        /// <summary>현재 진행도 비율을 반환한다 (0~1).</summary>
        public float GetProgressFraction()
        {
            int prev = _config.GetPrevProgressFloor(_model.CurrentFloor);
            int next = _config.GetNextProgressFloor(_model.CurrentFloor);

            if (next < 0) return 1f;
            if (prev < 0) prev = 0;

            int range = next - prev;
            if (range <= 0) return 0f;

            return (float)(_model.CurrentFloor - prev) / range;
        }

        /// <summary>빠른 클리어를 실행하고 결과를 반환한다.</summary>
        public MineQuickClearResult ExecuteQuickClear()
        {
            var result = new MineQuickClearResult();

            while (CurrentPickaxeCount > 0)
            {
                _reusableIndices.Clear();
                for (int i = 0; i < MineModel.BoardSize; i++)
                {
                    if (_model.Board[i].State == MineBlockState.Hidden)
                        _reusableIndices.Add(i);
                }

                if (_reusableIndices.Count == 0) break;

                int targetIdx = _reusableIndices[_random.Next(_reusableIndices.Count)];

                if (!_currencyService.TrySpend(CurrencyType.Pickaxe, BigNumber.One)) break;
                result.PickaxesUsed++;

                var block = _model.Board[targetIdx];
                var newState = block.RewardType == MineBlockRewardType.Empty
                    ? MineBlockState.Collected
                    : MineBlockState.Revealed;
                _model.SetBoardBlock(targetIdx, block.WithState(newState));

                if (block.RewardType != MineBlockRewardType.Empty &&
                    block.RewardType != MineBlockRewardType.TreasureChest)
                {
                    var currencyType = GetRewardCurrencyType(block.RewardType);
                    if (currencyType.HasValue)
                    {
                        var amount = new BigNumber(block.RewardAmount, 0);
                        _currencyService.Add(currencyType.Value, amount);
                        result.AddReward(currencyType.Value, amount);
                    }
                    _model.SetBoardBlock(targetIdx, block.WithState(MineBlockState.Collected));
                }

                if (block.RewardType == MineBlockRewardType.TreasureChest)
                {
                    for (int i = 0; i < _chestHandler.UpgradeSlots; i++)
                    {
                        if (!_model.ChestCompleted)
                        {
                            _chestHandler.PerformUpgrade(_model, i);
                        }
                    }

                    var grade = GetChestCurrentGrade();
                    var chestReward = _config.GetChestRewardConfig(grade);
                    GrantChestReward(chestReward, result);

                    _model.SetChestRewardClaimed(true);

                    CollectAllUncollectedCore(result);

                    if (_model.CurrentFloor < _config.Settings.MaxFloor)
                        _model.SetCurrentFloor(_model.CurrentFloor + 1);

                    result.FloorsCleared++;
                    InitializeBoard();
                }
            }

            _messageBroker.Publish(new MineQuickClearCompletedMessage { Result = result });
            _messageBroker.Publish(new MineStateChangedMessage());

            return result;
        }

        /// <summary>미수령 보상 수집 공통 로직.</summary>
        /// <param name="result">빠른 클리어 결과 집계용 (null이면 무시)</param>
        /// <returns>수집된 보상이 있으면 true</returns>
        private bool CollectAllUncollectedCore(MineQuickClearResult result = null)
        {
            bool collected = false;
            for (int i = 0; i < MineModel.BoardSize; i++)
            {
                if (_model.Board[i].State != MineBlockState.Revealed) continue;
                if (_model.Board[i].RewardType == MineBlockRewardType.Empty) continue;
                if (_model.Board[i].RewardType == MineBlockRewardType.TreasureChest) continue;

                var ct = GetRewardCurrencyType(_model.Board[i].RewardType);
                if (ct.HasValue && _model.Board[i].RewardAmount > 0)
                {
                    var amt = new BigNumber(_model.Board[i].RewardAmount, 0);
                    _currencyService.Add(ct.Value, amt);
                    result?.AddReward(ct.Value, amt);
                }
                _model.SetBoardBlock(i, _model.Board[i].WithState(MineBlockState.Collected));
                collected = true;
            }
            return collected;
        }

        /// <summary>
        /// 보물상자 보상을 지급한다.
        /// </summary>
        private void GrantChestReward(MineChestRewardConfig rewardConfig, MineQuickClearResult quickClearResult = null)
        {
            if (rewardConfig == null) return;

            if (rewardConfig.OreAmount > 0)
            {
                var amt = new BigNumber(rewardConfig.OreAmount, 0);
                _currencyService.Add(CurrencyType.MineOre, amt);
                quickClearResult?.AddReward(CurrencyType.MineOre, amt);
            }
            if (rewardConfig.ExpFragmentAmount > 0)
            {
                var amt = new BigNumber(rewardConfig.ExpFragmentAmount, 0);
                _currencyService.Add(CurrencyType.ExpFragment, amt);
                quickClearResult?.AddReward(CurrencyType.ExpFragment, amt);
            }
            if (rewardConfig.GemAmount > 0)
            {
                var amt = new BigNumber(rewardConfig.GemAmount, 0);
                _currencyService.Add(CurrencyType.Gem, amt);
                quickClearResult?.AddReward(CurrencyType.Gem, amt);
            }
        }

        private void GrantBlockReward(MineBlockRewardType rewardType, int amount)
        {
            var currencyType = GetRewardCurrencyType(rewardType);
            if (currencyType.HasValue && amount > 0)
            {
                _currencyService.Add(currencyType.Value, new BigNumber(amount, 0));
            }
        }

        private static CurrencyType? GetRewardCurrencyType(MineBlockRewardType rewardType)
        {
            return rewardType switch
            {
                MineBlockRewardType.Ore => CurrencyType.MineOre,
                MineBlockRewardType.Pickaxe => CurrencyType.Pickaxe,
                MineBlockRewardType.Bomb => CurrencyType.Bomb,
                MineBlockRewardType.Dynamite => CurrencyType.Dynamite,
                MineBlockRewardType.ExpFragment => CurrencyType.ExpFragment,
                _ => null
            };
        }

        private static CurrencyType GetToolCurrencyType(MineToolType tool)
        {
            return tool switch
            {
                MineToolType.Pickaxe => CurrencyType.Pickaxe,
                MineToolType.Bomb => CurrencyType.Bomb,
                MineToolType.Dynamite => CurrencyType.Dynamite,
                _ => CurrencyType.Pickaxe
            };
        }
    }
}
