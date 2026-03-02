using System.Collections.Generic;
using Geuneda.DataExtensions;
using Geuneda.Services;
using Geuneda.UiService;
using IdleRPG.Core;
using IdleRPG.Economy;
using IdleRPG.Growth;
using IdleRPG.Hero;
using IdleRPG.Skill;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IdleRPG.UI
{
    /// <summary>
    /// 메인 전투 화면의 하단 UI. 스킬 슬롯, 전투력, 스탯 강화 스크롤을 표시한다.
    /// 하단 탭이 열리면 숨겨지고, 탭이 닫히면 다시 표시된다.
    /// </summary>
    public class MainBattleUiPresenter : UiPresenter
    {
        [Header("전투력")]
        [SerializeField] private TMP_Text _combatPowerText;

        [Header("스킬 슬롯")]
        [SerializeField] private BattleSkillSlotView[] _skillSlotViews;

        [Header("스킬 자동 사용")]
        [SerializeField] private Button _autoSkillButton;
        [SerializeField] private GameObject _autoOnState;
        [SerializeField] private GameObject _autoOffState;

        [Header("스탯 강화")]
        [SerializeField] private Transform _statScrollContent;
        [SerializeField] private StatUpgradeSlotView _statSlotPrefab;

        private IHeroGrowthService _growthService;
        private ICurrencyService _currencyService;
        private IMessageBrokerService _messageBroker;
        private IConfigsProvider _configsProvider;
        private ISkillService _skillService;
        private ISkillExecutionService _skillExecutionService;
        private readonly List<StatUpgradeSlotView> _statSlots = new();

        private static readonly Dictionary<HeroStatType, string> StatDisplayNames = new()
        {
            { HeroStatType.Attack, "공격" },
            { HeroStatType.Hp, "체력" },
            { HeroStatType.HpRegen, "체력재생" },
            { HeroStatType.AttackSpeed, "공격속도" },
            { HeroStatType.CritRate, "치명타확률" },
            { HeroStatType.CritDamage, "치명타피해" },
            { HeroStatType.DoubleShot, "이중사격" },
            { HeroStatType.TripleShot, "삼중사격" },
            { HeroStatType.AdvancedAttack, "고급공격" },
            { HeroStatType.EnemyBonusDamage, "적추가피해" }
        };

        protected override void OnInitialized()
        {
            _growthService = MainInstaller.Resolve<IHeroGrowthService>();
            _currencyService = MainInstaller.Resolve<ICurrencyService>();
            _messageBroker = MainInstaller.Resolve<IMessageBrokerService>();
            _configsProvider = MainInstaller.Resolve<IConfigsProvider>();
            _skillService = MainInstaller.Resolve<ISkillService>();
            _skillExecutionService = MainInstaller.Resolve<ISkillExecutionService>();

            _messageBroker.Subscribe<CombatPowerChangedMessage>(OnCombatPowerChanged);
            _messageBroker.Subscribe<SkillEquippedMessage>(OnSkillEquipped);
            _messageBroker.Subscribe<SkillUnequippedMessage>(OnSkillUnequipped);
            _messageBroker.Subscribe<SkillSlotUnlockedMessage>(OnSkillSlotUnlocked);

            SetupAutoSkillButton();
            SetupSkillSlotButtons();
            CreateStatSlots();
        }

        protected override void OnOpened()
        {
            RefreshCombatPower();
            RefreshAllSkillSlots();
            RefreshAutoSkillState();
            RefreshAllStatSlots();
        }

        private void Update()
        {
            if (!IsOpen) return;

            UpdateSkillSlotFills();
        }

        #region 스킬 슬롯

        private void SetupSkillSlotButtons()
        {
            if (_skillSlotViews == null) return;

            for (int i = 0; i < _skillSlotViews.Length; i++)
            {
                if (_skillSlotViews[i] == null) continue;
                int slotIndex = i;
                _skillSlotViews[i].SetOnClickListener(() => OnSkillSlotClicked(slotIndex));
            }
        }

        private void RefreshAllSkillSlots()
        {
            if (_skillSlotViews == null) return;

            for (int i = 0; i < _skillSlotViews.Length; i++)
            {
                RefreshSkillSlot(i);
            }
        }

        private void RefreshSkillSlot(int slotIndex)
        {
            if (_skillSlotViews == null || slotIndex < 0 || slotIndex >= _skillSlotViews.Length)
                return;

            var slotView = _skillSlotViews[slotIndex];
            if (slotView == null) return;

            if (!_skillService.IsSlotUnlocked(slotIndex))
            {
                slotView.SetLocked();
                return;
            }

            int equippedId = _skillService.Model.GetEquippedSkillId(slotIndex);
            if (equippedId == SkillModel.UNEQUIPPED)
            {
                slotView.SetEmpty();
                return;
            }

            slotView.SetEquipped(null);
        }

        private void UpdateSkillSlotFills()
        {
            if (_skillSlotViews == null || _skillExecutionService == null) return;

            for (int i = 0; i < _skillSlotViews.Length && i < SkillModel.MAX_SLOTS; i++)
            {
                if (_skillSlotViews[i] == null) continue;

                int equippedId = _skillService.Model.GetEquippedSkillId(i);
                if (equippedId == SkillModel.UNEQUIPPED) continue;

                bool isActive = _skillExecutionService.IsSlotActive(i);
                float cdRemaining = _skillExecutionService.GetCooldownRemaining(i);

                if (!isActive && cdRemaining <= 0f) continue;

                if (isActive)
                {
                    float remaining = _skillExecutionService.GetActiveRemaining(i);
                    float total = _skillExecutionService.GetActiveTotal(i);
                    _skillSlotViews[i].UpdateActive(remaining, total);
                }
                else
                {
                    float total = _skillExecutionService.GetCooldownTotal(i);
                    _skillSlotViews[i].UpdateCooldown(cdRemaining, total);
                }
            }
        }

        private void OnSkillSlotClicked(int slotIndex)
        {
            if (_skillExecutionService == null) return;
            if (_skillExecutionService.IsAutoEnabled) return;

            _skillExecutionService.RequestManualActivation(slotIndex);
        }

        private void OnSkillEquipped(SkillEquippedMessage msg)
        {
            RefreshSkillSlot(msg.SlotIndex);
        }

        private void OnSkillUnequipped(SkillUnequippedMessage msg)
        {
            RefreshSkillSlot(msg.SlotIndex);
        }

        private void OnSkillSlotUnlocked(SkillSlotUnlockedMessage msg)
        {
            RefreshSkillSlot(msg.SlotIndex);
        }

        #endregion

        #region 자동 스킬

        private void SetupAutoSkillButton()
        {
            if (_autoSkillButton != null)
                _autoSkillButton.onClick.AddListener(OnAutoSkillToggleClicked);
        }

        private void RefreshAutoSkillState()
        {
            if (_skillExecutionService == null) return;

            bool isAuto = _skillExecutionService.IsAutoEnabled;
            if (_autoOnState != null)
                _autoOnState.SetActive(isAuto);
            if (_autoOffState != null)
                _autoOffState.SetActive(!isAuto);
        }

        private void OnAutoSkillToggleClicked()
        {
            if (_skillExecutionService == null) return;

            _skillExecutionService.SetAutoEnabled(!_skillExecutionService.IsAutoEnabled);
            RefreshAutoSkillState();
        }

        #endregion

        #region 전투력

        private void OnCombatPowerChanged(CombatPowerChangedMessage msg)
        {
            UpdateCombatPowerText(msg.CurrentPower);
        }

        private void RefreshCombatPower()
        {
            if (_combatPowerText != null)
            {
                UpdateCombatPowerText(_growthService.CombatPower);
            }
        }

        private void UpdateCombatPowerText(BigNumber power)
        {
            if (_combatPowerText != null)
            {
                _combatPowerText.text = power.ToFormattedString(2);
            }
        }

        #endregion

        #region 스탯 강화

        private void CreateStatSlots()
        {
            if (_statScrollContent == null || _statSlotPrefab == null) return;

            var growthConfig = _configsProvider.GetConfig<GrowthConfig>();
            foreach (var entry in growthConfig.Entries)
            {
                var slot = Instantiate(_statSlotPrefab, _statScrollContent);
                string displayName = StatDisplayNames.TryGetValue(entry.StatType, out var name)
                    ? name
                    : entry.StatType.ToString();

                slot.Setup(entry.StatType, displayName, _growthService, _currencyService, _messageBroker);
                _statSlots.Add(slot);
            }
        }

        private void RefreshAllStatSlots()
        {
            foreach (var slot in _statSlots)
            {
                slot.Refresh();
            }
        }

        #endregion

        protected override void OnClosed()
        {
            _messageBroker?.Unsubscribe<CombatPowerChangedMessage>(this);
            _messageBroker?.Unsubscribe<SkillEquippedMessage>(this);
            _messageBroker?.Unsubscribe<SkillUnequippedMessage>(this);
            _messageBroker?.Unsubscribe<SkillSlotUnlockedMessage>(this);

            if (_autoSkillButton != null)
                _autoSkillButton.onClick.RemoveListener(OnAutoSkillToggleClicked);
        }
    }
}
