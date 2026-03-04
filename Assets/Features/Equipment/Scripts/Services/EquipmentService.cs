using Geuneda.Services;
using IdleRPG.Core;
using IdleRPG.Growth;
using IdleRPG.Hero;

namespace IdleRPG.Equipment
{
    /// <summary>
    /// <see cref="IEquipmentService"/>의 구현체.
    /// 장비 획득, 강화, 장착, 효과 적용을 처리한다.
    /// </summary>
    public class EquipmentService : IEquipmentService
    {
        private readonly EquipmentConfigCollection _config;
        private readonly EquipmentModel _model;
        private readonly HeroModel _heroModel;
        private readonly IHeroGrowthService _growthService;
        private readonly IMessageBrokerService _messageBroker;
        private readonly CommandService<EquipmentModel> _commandService;
        private ISkillBonusProvider _skillBonusProvider;
        private IPetBonusProvider _petBonusProvider;

        /// <inheritdoc />
        public EquipmentModel Model => _model;

        /// <inheritdoc />
        public EquipmentConfigCollection Config => _config;

        /// <inheritdoc />
        public BigNumber TotalPossessionAttackPercent { get; private set; } = BigNumber.Zero;

        /// <inheritdoc />
        public BigNumber TotalPossessionHpPercent { get; private set; } = BigNumber.Zero;

        /// <inheritdoc />
        public BigNumber EquippedAttackPercent { get; private set; } = BigNumber.Zero;

        /// <inheritdoc />
        public BigNumber EquippedHpPercent { get; private set; } = BigNumber.Zero;

        /// <summary>
        /// <see cref="EquipmentService"/>를 생성한다.
        /// </summary>
        /// <param name="config">장비 설정 컬렉션</param>
        /// <param name="model">장비 런타임 모델</param>
        /// <param name="heroModel">영웅 모델 (스탯 적용 대상)</param>
        /// <param name="growthService">성장 서비스 (기본 스탯 조회)</param>
        /// <param name="messageBroker">메시지 브로커</param>
        public EquipmentService(
            EquipmentConfigCollection config,
            EquipmentModel model,
            HeroModel heroModel,
            IHeroGrowthService growthService,
            IMessageBrokerService messageBroker)
        {
            _config = config;
            _model = model;
            _heroModel = heroModel;
            _growthService = growthService;
            _messageBroker = messageBroker;
            _commandService = new CommandService<EquipmentModel>(model, messageBroker);

            _messageBroker.Subscribe<StatLevelUpMessage>(OnStatLevelUp);
            _messageBroker.Subscribe<SkillBonusChangedMessage>(OnSkillBonusChanged);
            _messageBroker.Subscribe<PetBonusChangedMessage>(OnPetBonusChanged);

            RecalculateAndApplyEffects();
        }

        /// <inheritdoc />
        public void AcquireEquipment(int id, int count)
        {
            if (count <= 0) return;
            if (_config.GetEntry(id) == null) return;

            _commandService.ExecuteCommand(new AcquireEquipmentCommand
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

            var state = _model.GetItemState(id);
            int requiredCount = GetUpgradeCost(id);

            _commandService.ExecuteCommand(new UpgradeEquipmentCommand
            {
                Id = id,
                RequiredCount = requiredCount
            });

            RecalculateAndApplyEffects();
            return true;
        }

        /// <inheritdoc />
        public int UpgradeAll(EquipmentType type)
        {
            var entries = _config.GetEntriesByType(type);
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

            if (EquipmentFormula.IsMaxLevel(entry, state.Level.Value)) return false;

            int required = GetUpgradeCost(id);
            return state.OwnedCount.Value >= required;
        }

        /// <inheritdoc />
        public int GetUpgradeCost(int id)
        {
            var state = _model.GetItemState(id);
            if (state == null) return 0;

            return EquipmentFormula.GetRequiredCount(_config.UpgradeConfig, state.Level.Value);
        }

        /// <inheritdoc />
        public bool TryEquip(int id)
        {
            var entry = _config.GetEntry(id);
            if (entry == null) return false;

            var state = _model.GetItemState(id);
            if (state == null || !state.IsUnlocked) return false;

            _commandService.ExecuteCommand(new EquipCommand
            {
                SlotType = entry.Type,
                EquipmentId = id
            });

            RecalculateAndApplyEffects();
            return true;
        }

        /// <inheritdoc />
        public void Unequip(EquipmentType type)
        {
            if (!_model.IsEquipped(type)) return;

            _commandService.ExecuteCommand(new UnequipCommand
            {
                SlotType = type
            });

            RecalculateAndApplyEffects();
        }

        /// <inheritdoc />
        public void QuickEquip(EquipmentType type)
        {
            int? bestId = EquipmentFormula.FindBestEquipmentId(_config, _model, type);
            if (bestId.HasValue)
            {
                TryEquip(bestId.Value);
            }
        }

        /// <inheritdoc />
        public BigNumber GetPossessionEffect(int id)
        {
            var entry = _config.GetEntry(id);
            if (entry == null) return BigNumber.Zero;

            var state = _model.GetItemState(id);
            if (state == null || !state.IsUnlocked) return BigNumber.Zero;

            return EquipmentFormula.CalcPossessionEffect(entry, state.Level.Value);
        }

        /// <inheritdoc />
        public BigNumber GetEquipEffect(int id)
        {
            var entry = _config.GetEntry(id);
            if (entry == null) return BigNumber.Zero;

            var state = _model.GetItemState(id);
            if (state == null || !state.IsUnlocked) return BigNumber.Zero;

            return EquipmentFormula.CalcEquipEffect(entry, state.Level.Value);
        }

        /// <inheritdoc />
        public void RecalculateAndApplyEffects()
        {
            BigNumber possessionAttack = BigNumber.Zero;
            BigNumber possessionHp = BigNumber.Zero;
            BigNumber equipAttack = BigNumber.Zero;
            BigNumber equipHp = BigNumber.Zero;

            foreach (var item in _model.GetAllUnlockedItems())
            {
                var entry = _config.GetEntry(item.Id);
                if (entry == null) continue;

                var possession = EquipmentFormula.CalcPossessionEffect(entry, item.Level.Value);

                switch (entry.Type)
                {
                    case EquipmentType.Weapon:
                        possessionAttack = possessionAttack + possession;
                        break;
                    case EquipmentType.Armor:
                        possessionHp = possessionHp + possession;
                        break;
                }
            }

            int equippedWeaponId = _model.GetEquippedId(EquipmentType.Weapon);
            if (equippedWeaponId != EquipmentModel.UNEQUIPPED)
            {
                var entry = _config.GetEntry(equippedWeaponId);
                var state = _model.GetItemState(equippedWeaponId);
                if (entry != null && state != null && state.IsUnlocked)
                {
                    equipAttack = EquipmentFormula.CalcEquipEffect(entry, state.Level.Value);
                }
            }

            int equippedArmorId = _model.GetEquippedId(EquipmentType.Armor);
            if (equippedArmorId != EquipmentModel.UNEQUIPPED)
            {
                var entry = _config.GetEntry(equippedArmorId);
                var state = _model.GetItemState(equippedArmorId);
                if (entry != null && state != null && state.IsUnlocked)
                {
                    equipHp = EquipmentFormula.CalcEquipEffect(entry, state.Level.Value);
                }
            }

            TotalPossessionAttackPercent = possessionAttack;
            TotalPossessionHpPercent = possessionHp;
            EquippedAttackPercent = equipAttack;
            EquippedHpPercent = equipHp;

            ApplyToHeroModel();

            _messageBroker.Publish(new EquipmentEffectsChangedMessage
            {
                TotalAttackBonusPercent = possessionAttack + equipAttack,
                TotalHpBonusPercent = possessionHp + equipHp
            });
        }

        private void ApplyToHeroModel()
        {
            BigNumber skillAttackPercent = _skillBonusProvider?.TotalPossessionAttackPercent ?? BigNumber.Zero;
            BigNumber petAttackPercent = _petBonusProvider?.TotalPossessionAttackPercent ?? BigNumber.Zero;
            BigNumber totalAttackPercent = TotalPossessionAttackPercent + EquippedAttackPercent + skillAttackPercent + petAttackPercent;
            BigNumber totalHpPercent = TotalPossessionHpPercent + EquippedHpPercent;

            BigNumber baseAttack = _growthService.GetCurrentStatValue(HeroStatType.Attack);
            BigNumber baseHp = _growthService.GetCurrentStatValue(HeroStatType.Hp);

            BigNumber hundred = new BigNumber(100, 0);
            BigNumber finalAttack = baseAttack * (hundred + totalAttackPercent) / hundred;
            BigNumber finalHp = baseHp * (hundred + totalHpPercent) / hundred;

            _heroModel.SetBigNumberStat(HeroStatType.Attack, finalAttack);
            _heroModel.SetBigNumberStat(HeroStatType.Hp, finalHp);

            BigNumber baseHpRegen = _growthService.GetCurrentStatValue(HeroStatType.HpRegen);
            _heroModel.SetBigNumberStat(HeroStatType.HpRegen, baseHpRegen);

            var previousPower = _growthService.CombatPower;
            _growthService.RecalculateCombatPower();

            _messageBroker.Publish(new CombatPowerChangedMessage
            {
                PreviousPower = previousPower,
                CurrentPower = _growthService.CombatPower
            });
        }

        private bool TryUpgradeInternal(int id)
        {
            if (!CanUpgrade(id)) return false;

            int requiredCount = GetUpgradeCost(id);

            _commandService.ExecuteCommand(new UpgradeEquipmentCommand
            {
                Id = id,
                RequiredCount = requiredCount
            });

            return true;
        }

        /// <summary>
        /// 스킬 보유효과 제공자를 설정한다. 스탯 재계산에 스킬 보유효과를 포함시킨다.
        /// </summary>
        /// <param name="provider">스킬 보너스 제공자</param>
        public void SetSkillBonusProvider(ISkillBonusProvider provider)
        {
            _skillBonusProvider = provider;
            RecalculateAndApplyEffects();
        }

        /// <summary>
        /// 펫 보유효과 제공자를 설정한다. 스탯 재계산에 펫 보유효과를 포함시킨다.
        /// </summary>
        /// <param name="provider">펫 보너스 제공자</param>
        public void SetPetBonusProvider(IPetBonusProvider provider)
        {
            _petBonusProvider = provider;
            RecalculateAndApplyEffects();
        }

        private void OnSkillBonusChanged(SkillBonusChangedMessage message)
        {
            RecalculateAndApplyEffects();
        }

        private void OnPetBonusChanged(PetBonusChangedMessage message)
        {
            RecalculateAndApplyEffects();
        }

        private void OnStatLevelUp(StatLevelUpMessage message)
        {
            if (message.StatType == HeroStatType.Attack ||
                message.StatType == HeroStatType.Hp ||
                message.StatType == HeroStatType.HpRegen)
            {
                RecalculateAndApplyEffects();
            }
        }
    }
}
