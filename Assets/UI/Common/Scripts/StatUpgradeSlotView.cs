using Geuneda.Services;
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
    /// 스탯 강화 슬롯 하나를 표현하는 뷰 컴포넌트.
    /// 스탯 아이콘, 이름, 레벨, 현재 값, 강화 버튼 및 비용을 표시한다.
    /// </summary>
    public class StatUpgradeSlotView : MonoBehaviour
    {
        [SerializeField] private Image _statIcon;
        [SerializeField] private TMP_Text _statNameText;
        [SerializeField] private TMP_Text _levelText;
        [SerializeField] private TMP_Text _statValueText;
        [SerializeField] private Button _upgradeButton;
        [SerializeField] private TMP_Text _costText;
        [SerializeField] private GameObject _lockedOverlay;

        private HeroStatType _statType;
        private IHeroGrowthService _growthService;
        private ICurrencyService _currencyService;
        private IMessageBrokerService _messageBroker;

        /// <summary>
        /// 슬롯을 초기화한다. 대상 스탯과 서비스를 연결하고 이벤트를 바인딩한다.
        /// </summary>
        /// <param name="statType">이 슬롯이 표시할 스탯 유형</param>
        /// <param name="statName">표시용 스탯 이름</param>
        /// <param name="growthService">성장 서비스</param>
        /// <param name="currencyService">재화 서비스</param>
        /// <param name="messageBroker">메시지 브로커</param>
        public void Setup(
            HeroStatType statType,
            string statName,
            IHeroGrowthService growthService,
            ICurrencyService currencyService,
            IMessageBrokerService messageBroker)
        {
            _statType = statType;
            _growthService = growthService;
            _currencyService = currencyService;
            _messageBroker = messageBroker;

            if (_statNameText != null)
                _statNameText.text = statName;

            _upgradeButton.onClick.AddListener(OnUpgradeClicked);

            _messageBroker.Subscribe<StatLevelUpMessage>(OnStatLevelUp);
            _currencyService.Currencies.Observe(CurrencyType.Gold, OnGoldChanged);

            Refresh();
        }

        /// <summary>
        /// 슬롯의 모든 표시 정보를 현재 상태로 갱신한다.
        /// </summary>
        public void Refresh()
        {
            bool unlocked = _growthService.IsUnlocked(_statType);

            if (_lockedOverlay != null)
                _lockedOverlay.SetActive(!unlocked);

            if (!unlocked)
            {
                SetLockedDisplay();
                return;
            }

            int level = _growthService.Model.GetLevel(_statType);
            BigNumber statValue = _growthService.GetCurrentStatValue(_statType);
            BigNumber cost = _growthService.GetLevelUpCost(_statType);
            bool isMaxLevel = _growthService.IsMaxLevel(_statType);
            bool canAfford = _currencyService.HasEnough(CurrencyType.Gold, cost);

            if (_levelText != null)
                _levelText.text = $"Lv {level}";

            if (_statValueText != null)
                _statValueText.text = FormatStatValue(statValue);

            if (isMaxLevel)
            {
                if (_costText != null)
                    _costText.text = "MAX";
                _upgradeButton.interactable = false;
            }
            else
            {
                if (_costText != null)
                    _costText.text = cost.ToFormattedString(2);
                _upgradeButton.interactable = canAfford;
            }
        }

        private void SetLockedDisplay()
        {
            if (_levelText != null)
                _levelText.text = "";

            if (_statValueText != null)
                _statValueText.text = "잠김";

            if (_costText != null)
                _costText.text = "";

            _upgradeButton.interactable = false;
        }

        private string FormatStatValue(BigNumber value)
        {
            double d = value.ToDouble();
            if (d >= 0 && d < 1)
                return $"{d * 100:F1}%";

            return value.ToFormattedString(2);
        }

        private void OnUpgradeClicked()
        {
            _growthService.TryLevelUp(_statType);
        }

        private void OnStatLevelUp(StatLevelUpMessage msg)
        {
            if (msg.StatType == _statType)
                Refresh();
        }

        private void OnGoldChanged(CurrencyType key, BigNumber prev, BigNumber curr,
            Geuneda.DataExtensions.ObservableUpdateType updateType)
        {
            RefreshButtonState();
        }

        private void RefreshButtonState()
        {
            if (!_growthService.IsUnlocked(_statType) || _growthService.IsMaxLevel(_statType))
                return;

            BigNumber cost = _growthService.GetLevelUpCost(_statType);
            _upgradeButton.interactable = _currencyService.HasEnough(CurrencyType.Gold, cost);
        }

        private void OnDestroy()
        {
            _upgradeButton.onClick.RemoveListener(OnUpgradeClicked);
            _messageBroker?.Unsubscribe<StatLevelUpMessage>(this);
            _currencyService?.Currencies.StopObserving(OnGoldChanged);
        }
    }
}
