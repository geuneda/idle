using System.Collections.Generic;
using Geuneda.Services;
using Geuneda.UiService;
using IdleRPG.Core;
using IdleRPG.Equipment;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IdleRPG.UI
{
    /// <summary>
    /// 장비 팝업 프레젠터. 특정 장비 타입의 도감 그리드를 표시하고
    /// 빠른 장착, 전체 강화 기능을 제공한다.
    /// </summary>
    public class EquipmentPopupPresenter : UiPresenter<EquipmentPopupData>
    {
        [Header("Current Equipment")]
        [SerializeField] private Image _currentEquipIcon;
        [SerializeField] private Image _currentEquipIconBg;
        [SerializeField] private TMP_Text _currentEquipNameText;
        [SerializeField] private TMP_Text _currentEquipLevelText;
        [SerializeField] private TMP_Text _currentEquipCountText;
        [SerializeField] private TMP_Text _possessionEffectText;
        [SerializeField] private TMP_Text _equipEffectText;
        [SerializeField] private GameObject _noEquipPanel;
        [SerializeField] private GameObject _currentEquipPanel;

        [Header("Collection Grid")]
        [SerializeField] private Transform _gridContent;
        [SerializeField] private EquipmentCollectionItemView _itemPrefab;

        [Header("Summary")]
        [SerializeField] private TMP_Text _totalPossessionEffectText;

        [Header("Buttons")]
        [SerializeField] private Button _quickEquipButton;
        [SerializeField] private Button _upgradeAllButton;
        [SerializeField] private Button _closeButton;

        private IEquipmentService _equipmentService;
        private IMessageBrokerService _messageBroker;
        private readonly List<EquipmentCollectionItemView> _itemViews = new();

        protected override void OnInitialized()
        {
            _equipmentService = MainInstaller.Resolve<IEquipmentService>();
            _messageBroker = MainInstaller.Resolve<IMessageBrokerService>();

            if (_quickEquipButton != null)
                _quickEquipButton.onClick.AddListener(OnQuickEquipClicked);
            if (_upgradeAllButton != null)
                _upgradeAllButton.onClick.AddListener(OnUpgradeAllClicked);
            if (_closeButton != null)
                _closeButton.onClick.AddListener(OnCloseClicked);
        }

        protected override void OnSetData()
        {
            BuildGrid();
        }

        protected override void OnOpened()
        {
            _messageBroker.Subscribe<EquipmentAcquiredMessage>(OnEquipmentChanged);
            _messageBroker.Subscribe<EquipmentUpgradedMessage>(OnEquipmentUpgraded);
            _messageBroker.Subscribe<EquipmentEquippedMessage>(OnEquipmentEquipped);
            RefreshAll();
        }

        protected override void OnClosed()
        {
            _messageBroker?.Unsubscribe<EquipmentAcquiredMessage>(this);
            _messageBroker?.Unsubscribe<EquipmentUpgradedMessage>(this);
            _messageBroker?.Unsubscribe<EquipmentEquippedMessage>(this);
            ClearGrid();
        }

        private void BuildGrid()
        {
            ClearGrid();

            var entries = _equipmentService.Config.GetEntriesByType(Data.SlotType);
            if (entries == null) return;

            foreach (var entry in entries)
            {
                if (_itemPrefab == null || _gridContent == null) continue;

                var itemView = Instantiate(_itemPrefab, _gridContent);
                itemView.Setup(entry.Id, _equipmentService);
                itemView.Clicked += OnItemClicked;
                _itemViews.Add(itemView);
            }
        }

        private void ClearGrid()
        {
            foreach (var view in _itemViews)
            {
                if (view != null)
                {
                    view.Clicked -= OnItemClicked;
                    Destroy(view.gameObject);
                }
            }
            _itemViews.Clear();
        }

        private void RefreshAll()
        {
            RefreshCurrentEquip();
            RefreshGridItems();
            RefreshTotalEffect();
        }

        private void RefreshCurrentEquip()
        {
            int equippedId = _equipmentService.Model.GetEquippedId(Data.SlotType);
            bool hasEquipped = equippedId != EquipmentModel.UNEQUIPPED;

            if (_noEquipPanel != null)
                _noEquipPanel.SetActive(!hasEquipped);
            if (_currentEquipPanel != null)
                _currentEquipPanel.SetActive(hasEquipped);

            if (!hasEquipped) return;

            var entry = _equipmentService.Config.GetEntry(equippedId);
            var state = _equipmentService.Model.GetItemState(equippedId);
            if (entry == null || state == null) return;

            if (_currentEquipIconBg != null)
                _currentEquipIconBg.color = EquipmentGradeHelper.GetColor(entry.Grade);

            if (_currentEquipNameText != null)
                _currentEquipNameText.text = entry.DisplayName;
            if (_currentEquipLevelText != null)
                _currentEquipLevelText.text = $"Lv.{state.Level.Value}";
            if (_currentEquipCountText != null)
            {
                int required = _equipmentService.GetUpgradeCost(equippedId);
                _currentEquipCountText.text = $"{state.OwnedCount.Value}/{required}";
            }

            BigNumber possEffect = _equipmentService.GetPossessionEffect(equippedId);
            BigNumber equipEffect = _equipmentService.GetEquipEffect(equippedId);

            if (_possessionEffectText != null)
                _possessionEffectText.text = $"+{possEffect.ToFormattedString(2)}%";
            if (_equipEffectText != null)
                _equipEffectText.text = $"+{equipEffect.ToFormattedString(2)}%";
        }

        private void RefreshGridItems()
        {
            foreach (var view in _itemViews)
            {
                view.Refresh();
            }
        }

        private void RefreshTotalEffect()
        {
            if (_totalPossessionEffectText == null) return;

            string effectLabel = Data.SlotType switch
            {
                EquipmentType.Weapon => "공격력",
                EquipmentType.Armor => "체력",
                _ => "효과"
            };

            BigNumber totalEffect = Data.SlotType == EquipmentType.Weapon
                ? _equipmentService.TotalPossessionAttackPercent
                : _equipmentService.TotalPossessionHpPercent;

            _totalPossessionEffectText.text = $"총 보유효과: {effectLabel} +{totalEffect.ToFormattedString(2)}%";
        }

        private async void OnItemClicked(int equipmentId)
        {
            await _uiService.OpenUiAsync<EquipmentDetailPopupPresenter, EquipmentDetailPopupData>(
                new EquipmentDetailPopupData { EquipmentId = equipmentId });
        }

        private void OnQuickEquipClicked()
        {
            _equipmentService.QuickEquip(Data.SlotType);
        }

        private void OnUpgradeAllClicked()
        {
            _equipmentService.UpgradeAll(Data.SlotType);
        }

        private void OnCloseClicked()
        {
            Close(true);
        }

        private void OnEquipmentChanged(EquipmentAcquiredMessage msg)
        {
            RefreshAll();
        }

        private void OnEquipmentUpgraded(EquipmentUpgradedMessage msg)
        {
            RefreshAll();
        }

        private void OnEquipmentEquipped(EquipmentEquippedMessage msg)
        {
            if (msg.SlotType == Data.SlotType)
                RefreshAll();
        }
    }
}
