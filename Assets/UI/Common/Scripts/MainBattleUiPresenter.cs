using System.Collections.Generic;
using Geuneda.DataExtensions;
using Geuneda.Services;
using Geuneda.UiService;
using IdleRPG.Core;
using IdleRPG.Economy;
using IdleRPG.Growth;
using IdleRPG.Hero;
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

        [Header("스킬 슬롯")] // 추후 구현

        [Header("스탯 강화")]
        [SerializeField] private Transform _statScrollContent;
        [SerializeField] private StatUpgradeSlotView _statSlotPrefab;

        private IHeroGrowthService _growthService;
        private ICurrencyService _currencyService;
        private IMessageBrokerService _messageBroker;
        private IConfigsProvider _configsProvider;
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

            _messageBroker.Subscribe<CombatPowerChangedMessage>(OnCombatPowerChanged);

            CreateStatSlots();
        }

        protected override void OnOpened()
        {
            RefreshCombatPower();
            RefreshAllStatSlots();
        }

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

        private void RefreshAllStatSlots()
        {
            foreach (var slot in _statSlots)
            {
                slot.Refresh();
            }
        }

        protected override void OnClosed()
        {
            _messageBroker?.Unsubscribe<CombatPowerChangedMessage>(this);
        }
    }
}
