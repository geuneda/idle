using System.Collections.Generic;
using Geuneda.Services;
using IdleRPG.Core;
using IdleRPG.Stage;

namespace IdleRPG.Skill
{
    /// <summary>
    /// <see cref="ISkillService"/>의 구현체.
    /// 스킬 획득, 강화, 장착, 보유효과 적용을 처리한다.
    /// </summary>
    public class SkillService : ISkillService
    {
        private readonly SkillConfigCollection _config;
        private readonly SkillModel _model;
        private readonly IStageService _stageService;
        private readonly IMessageBrokerService _messageBroker;
        private readonly CommandService<SkillModel> _commandService;
        private readonly List<int> _equippedIdsCache = new();

        /// <inheritdoc />
        public SkillModel Model => _model;

        /// <inheritdoc />
        public SkillConfigCollection Config => _config;

        /// <inheritdoc />
        public BigNumber TotalPossessionAttackPercent { get; private set; } = BigNumber.Zero;

        /// <summary>
        /// <see cref="SkillService"/>를 생성한다.
        /// </summary>
        /// <param name="config">스킬 설정 컬렉션</param>
        /// <param name="model">스킬 런타임 모델</param>
        /// <param name="stageService">스테이지 서비스 (슬롯 해금 판정용)</param>
        /// <param name="messageBroker">메시지 브로커</param>
        public SkillService(
            SkillConfigCollection config,
            SkillModel model,
            IStageService stageService,
            IMessageBrokerService messageBroker)
        {
            _config = config;
            _model = model;
            _stageService = stageService;
            _messageBroker = messageBroker;
            _commandService = new CommandService<SkillModel>(model, messageBroker);

            _messageBroker.Subscribe<StageClearedMessage>(OnStageCleared);

            RecalculateAndApplyEffects();
        }

        /// <inheritdoc />
        public void AcquireSkill(int id, int count)
        {
            if (count <= 0) return;
            if (_config.GetEntry(id) == null) return;

            _commandService.ExecuteCommand(new AcquireSkillCommand
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

            _commandService.ExecuteCommand(new UpgradeSkillCommand
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

            if (SkillFormula.IsMaxLevel(entry, state.Level.Value)) return false;

            int required = GetUpgradeCost(id);
            return state.OwnedCount.Value >= required;
        }

        /// <inheritdoc />
        public int GetUpgradeCost(int id)
        {
            var state = _model.GetItemState(id);
            if (state == null) return 0;

            return SkillFormula.GetRequiredCount(_config.UpgradeConfig, state.Level.Value);
        }

        /// <inheritdoc />
        public bool TryEquip(int skillId, int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= SkillModel.MAX_SLOTS) return false;
            if (!IsSlotUnlocked(slotIndex)) return false;

            var state = _model.GetItemState(skillId);
            if (state == null || !state.IsUnlocked) return false;

            // 이미 다른 슬롯에 장착되어 있으면 기존 슬롯 해제
            int existingSlot = _model.FindEquippedSlotIndex(skillId);
            if (existingSlot >= 0)
            {
                _commandService.ExecuteCommand(new UnequipSkillCommand
                {
                    SlotIndex = existingSlot
                });
            }

            _commandService.ExecuteCommand(new EquipSkillCommand
            {
                SlotIndex = slotIndex,
                SkillId = skillId
            });

            RecalculateAndApplyEffects();
            return true;
        }

        /// <inheritdoc />
        public void Unequip(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= SkillModel.MAX_SLOTS) return;
            if (_model.GetEquippedSkillId(slotIndex) == SkillModel.UNEQUIPPED) return;

            _commandService.ExecuteCommand(new UnequipSkillCommand
            {
                SlotIndex = slotIndex
            });

            RecalculateAndApplyEffects();
        }

        /// <inheritdoc />
        public void QuickEquip()
        {
            // 해금된 스킬을 레벨 내림차순으로 정렬
            var sorted = new List<SkillItemState>();
            foreach (var item in _model.GetAllUnlockedItems())
            {
                if (!_model.IsEquipped(item.Id))
                {
                    sorted.Add(item);
                }
            }
            sorted.Sort((a, b) => b.Level.Value.CompareTo(a.Level.Value));

            int sortedIndex = 0;
            for (int slot = 0; slot < SkillModel.MAX_SLOTS; slot++)
            {
                if (!IsSlotUnlocked(slot)) continue;
                if (_model.GetEquippedSkillId(slot) != SkillModel.UNEQUIPPED) continue;
                if (sortedIndex >= sorted.Count) break;

                _commandService.ExecuteCommand(new EquipSkillCommand
                {
                    SlotIndex = slot,
                    SkillId = sorted[sortedIndex].Id
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

            return SkillFormula.IsSlotUnlocked(
                slotConfig,
                _stageService.Model.HighestChapter,
                _stageService.Model.HighestStage);
        }

        /// <inheritdoc />
        public int GetUnlockedSlotCount()
        {
            int count = 0;
            for (int i = 0; i < SkillModel.MAX_SLOTS; i++)
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

            return SkillFormula.CalcPossessionEffect(entry, state.Level.Value);
        }

        /// <inheritdoc />
        public IReadOnlyList<int> GetEquippedSkillIds()
        {
            _equippedIdsCache.Clear();
            for (int i = 0; i < SkillModel.MAX_SLOTS; i++)
            {
                int id = _model.GetEquippedSkillId(i);
                if (id != SkillModel.UNEQUIPPED)
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

                totalAttack = totalAttack + SkillFormula.CalcPossessionEffect(entry, item.Level.Value);
            }

            TotalPossessionAttackPercent = totalAttack;

            _messageBroker.Publish(new SkillEffectsChangedMessage
            {
                TotalPossessionAttackPercent = totalAttack
            });

            _messageBroker.Publish(new SkillBonusChangedMessage());
        }

        private bool TryUpgradeInternal(int id)
        {
            if (!CanUpgrade(id)) return false;

            int requiredCount = GetUpgradeCost(id);

            _commandService.ExecuteCommand(new UpgradeSkillCommand
            {
                Id = id,
                RequiredCount = requiredCount
            });

            return true;
        }

        private void OnStageCleared(StageClearedMessage message)
        {
            // 스테이지 클리어 시 새 슬롯 해금 여부 확인
            for (int i = 0; i < SkillModel.MAX_SLOTS; i++)
            {
                var slotConfig = _config.GetSlotConfig(i);
                if (slotConfig == null) continue;

                bool wasUnlocked = SkillFormula.IsSlotUnlocked(
                    slotConfig,
                    _stageService.Model.HighestChapter,
                    _stageService.Model.HighestStage);

                if (wasUnlocked && _model.GetEquippedSkillId(i) == SkillModel.UNEQUIPPED)
                {
                    _messageBroker.Publish(new SkillSlotUnlockedMessage
                    {
                        SlotIndex = i
                    });
                }
            }
        }
    }
}
