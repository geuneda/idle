using System.Collections.Generic;
using Geuneda.Services;
using IdleRPG.Core;
using IdleRPG.Stage;

namespace IdleRPG.Pet
{
    /// <summary>
    /// <see cref="IPetService"/>의 구현체.
    /// 펫 획득, 강화, 장착, 보유효과 적용을 처리한다.
    /// </summary>
    public class PetService : IPetService
    {
        private readonly PetConfigCollection _config;
        private readonly PetModel _model;
        private readonly IStageService _stageService;
        private readonly IMessageBrokerService _messageBroker;
        private readonly CommandService<PetModel> _commandService;
        private readonly List<int> _equippedIdsCache = new();

        /// <inheritdoc />
        public PetModel Model => _model;

        /// <inheritdoc />
        public PetConfigCollection Config => _config;

        /// <inheritdoc />
        public BigNumber TotalPossessionAttackPercent { get; private set; } = BigNumber.Zero;

        /// <summary>
        /// <see cref="PetService"/>를 생성한다.
        /// </summary>
        /// <param name="config">펫 설정 컬렉션</param>
        /// <param name="model">펫 런타임 모델</param>
        /// <param name="stageService">스테이지 서비스 (슬롯 해금 판정용)</param>
        /// <param name="messageBroker">메시지 브로커</param>
        public PetService(
            PetConfigCollection config,
            PetModel model,
            IStageService stageService,
            IMessageBrokerService messageBroker)
        {
            _config = config;
            _model = model;
            _stageService = stageService;
            _messageBroker = messageBroker;
            _commandService = new CommandService<PetModel>(model, messageBroker);

            _messageBroker.Subscribe<StageClearedMessage>(OnStageCleared);

            RecalculateAndApplyEffects();
        }

        /// <inheritdoc />
        public void AcquirePet(int id, int count)
        {
            if (count <= 0) return;
            if (_config.GetEntry(id) == null) return;

            _commandService.ExecuteCommand(new AcquirePetCommand
            {
                Id = id,
                Count = count
            });

            RecalculateAndApplyEffects();
        }

        /// <inheritdoc />
        public bool TryUpgrade(int id)
        {
            if (!CanUpgrade(id)) return false;

            int requiredCount = GetUpgradeCost(id);

            _commandService.ExecuteCommand(new UpgradePetCommand
            {
                Id = id,
                RequiredCount = requiredCount
            });

            RecalculateAndApplyEffects();
            return true;
        }

        /// <inheritdoc />
        public int UpgradeAll()
        {
            var entries = _config.GetAllEntries();
            int totalUpgrades = 0;

            bool upgraded = true;
            while (upgraded)
            {
                upgraded = false;
                for (int i = 0; i < entries.Count; i++)
                {
                    if (TryUpgradeInternal(entries[i].Id))
                    {
                        totalUpgrades++;
                        upgraded = true;
                    }
                }
            }

            if (totalUpgrades > 0)
            {
                RecalculateAndApplyEffects();
            }

            return totalUpgrades;
        }

        /// <inheritdoc />
        public bool CanUpgrade(int id)
        {
            var entry = _config.GetEntry(id);
            if (entry == null) return false;

            var state = _model.GetItemState(id);
            if (state == null || !state.IsUnlocked) return false;

            if (PetFormula.IsMaxLevel(entry, state.Level.Value)) return false;

            int required = GetUpgradeCost(id);
            return state.OwnedCount.Value >= required;
        }

        /// <inheritdoc />
        public int GetUpgradeCost(int id)
        {
            var state = _model.GetItemState(id);
            if (state == null) return 0;

            return PetFormula.GetRequiredCount(_config.UpgradeConfig, state.Level.Value);
        }

        /// <inheritdoc />
        public bool TryEquip(int petId, int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= PetModel.MAX_SLOTS) return false;
            if (!IsSlotUnlocked(slotIndex)) return false;

            var state = _model.GetItemState(petId);
            if (state == null || !state.IsUnlocked) return false;

            // 이미 다른 슬롯에 장착되어 있으면 기존 슬롯 해제
            int existingSlot = _model.FindEquippedSlotIndex(petId);
            if (existingSlot >= 0)
            {
                _commandService.ExecuteCommand(new UnequipPetCommand
                {
                    SlotIndex = existingSlot
                });
            }

            _commandService.ExecuteCommand(new EquipPetCommand
            {
                SlotIndex = slotIndex,
                PetId = petId
            });

            RecalculateAndApplyEffects();
            return true;
        }

        /// <inheritdoc />
        public void Unequip(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= PetModel.MAX_SLOTS) return;
            if (_model.GetEquippedPetId(slotIndex) == PetModel.UNEQUIPPED) return;

            _commandService.ExecuteCommand(new UnequipPetCommand
            {
                SlotIndex = slotIndex
            });

            RecalculateAndApplyEffects();
        }

        /// <inheritdoc />
        public void QuickEquip()
        {
            // 해금된 펫을 유효 DPS 내림차순으로 정렬
            var sorted = new List<(CollectibleItemState state, double dps)>();
            foreach (var item in _model.GetAllUnlockedItems())
            {
                if (_model.IsEquipped(item.Id)) continue;

                var entry = _config.GetEntry(item.Id);
                if (entry == null) continue;

                double dps = PetFormula.CalcEffectiveDps(entry, item.Level.Value);
                sorted.Add((item, dps));
            }
            sorted.Sort((a, b) =>
            {
                int dpsCompare = b.dps.CompareTo(a.dps);
                if (dpsCompare != 0) return dpsCompare;
                int levelCompare = b.state.Level.Value.CompareTo(a.state.Level.Value);
                return levelCompare != 0 ? levelCompare : a.state.Id.CompareTo(b.state.Id);
            });

            int sortedIndex = 0;
            for (int slot = 0; slot < PetModel.MAX_SLOTS; slot++)
            {
                if (!IsSlotUnlocked(slot)) continue;
                if (_model.GetEquippedPetId(slot) != PetModel.UNEQUIPPED) continue;
                if (sortedIndex >= sorted.Count) break;

                _commandService.ExecuteCommand(new EquipPetCommand
                {
                    SlotIndex = slot,
                    PetId = sorted[sortedIndex].state.Id
                });
                sortedIndex++;
            }

            if (sortedIndex > 0)
            {
                RecalculateAndApplyEffects();
            }
        }

        /// <inheritdoc />
        public bool IsSlotUnlocked(int slotIndex)
        {
            var slotConfig = _config.GetSlotConfig(slotIndex);
            if (slotConfig == null) return false;

            return PetFormula.IsSlotUnlocked(
                slotConfig,
                _stageService.Model.HighestChapter,
                _stageService.Model.HighestStage);
        }

        /// <inheritdoc />
        public int GetUnlockedSlotCount()
        {
            int count = 0;
            for (int i = 0; i < PetModel.MAX_SLOTS; i++)
            {
                if (IsSlotUnlocked(i)) count++;
            }
            return count;
        }

        /// <inheritdoc />
        public BigNumber GetPossessionEffect(int id)
        {
            var entry = _config.GetEntry(id);
            if (entry == null) return BigNumber.Zero;

            var state = _model.GetItemState(id);
            if (state == null || !state.IsUnlocked) return BigNumber.Zero;

            return PetFormula.CalcPossessionEffect(entry, state.Level.Value);
        }

        /// <inheritdoc />
        public IReadOnlyList<int> GetEquippedPetIds()
        {
            _equippedIdsCache.Clear();
            for (int i = 0; i < PetModel.MAX_SLOTS; i++)
            {
                int id = _model.GetEquippedPetId(i);
                if (id != PetModel.UNEQUIPPED)
                {
                    _equippedIdsCache.Add(id);
                }
            }
            return _equippedIdsCache;
        }

        /// <inheritdoc />
        public void RecalculateAndApplyEffects()
        {
            BigNumber totalAttack = BigNumber.Zero;

            foreach (var item in _model.GetAllUnlockedItems())
            {
                var entry = _config.GetEntry(item.Id);
                if (entry == null) continue;

                totalAttack = totalAttack + PetFormula.CalcPossessionEffect(entry, item.Level.Value);
            }

            TotalPossessionAttackPercent = totalAttack;

            _messageBroker.Publish(new PetEffectsChangedMessage
            {
                TotalPossessionAttackPercent = totalAttack
            });

            _messageBroker.Publish(new PetBonusChangedMessage());
        }

        private bool TryUpgradeInternal(int id)
        {
            if (!CanUpgrade(id)) return false;

            int requiredCount = GetUpgradeCost(id);

            _commandService.ExecuteCommand(new UpgradePetCommand
            {
                Id = id,
                RequiredCount = requiredCount
            });

            return true;
        }

        private void OnStageCleared(StageClearedMessage message)
        {
            // 스테이지 클리어 시 새 슬롯 해금 여부 확인
            for (int i = 0; i < PetModel.MAX_SLOTS; i++)
            {
                var slotConfig = _config.GetSlotConfig(i);
                if (slotConfig == null) continue;

                bool wasUnlocked = PetFormula.IsSlotUnlocked(
                    slotConfig,
                    _stageService.Model.HighestChapter,
                    _stageService.Model.HighestStage);

                if (wasUnlocked && _model.GetEquippedPetId(i) == PetModel.UNEQUIPPED)
                {
                    _messageBroker.Publish(new PetSlotUnlockedMessage
                    {
                        SlotIndex = i
                    });
                }
            }
        }
    }
}
