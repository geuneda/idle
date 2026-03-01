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
    /// 장비 상세 팝업 프레젠터. 개별 장비의 상세 정보와 장착/강화 버튼을 표시한다.
    /// </summary>
    public class EquipmentDetailPopupPresenter : UiPresenter<EquipmentDetailPopupData>
    {
        [Header("Equipment Info")]
        [SerializeField] private Image _gradeBackground;
        [SerializeField] private Image _equipmentIcon;
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _gradeText;
        [SerializeField] private TMP_Text _levelText;
        [SerializeField] private TMP_Text _ownedCountText;

        [Header("Effects")]
        [SerializeField] private TMP_Text _possessionEffectText;
        [SerializeField] private TMP_Text _equipEffectText;

        [Header("Buttons")]
        [SerializeField] private Button _equipButton;
        [SerializeField] private TMP_Text _equipButtonText;
        [SerializeField] private Button _upgradeButton;
        [SerializeField] private TMP_Text _upgradeButtonText;
        [SerializeField] private Button _closeButton;

        private IEquipmentService _equipmentService;
        private IMessageBrokerService _messageBroker;

        protected override void OnInitialized()
        {
            _equipmentService = MainInstaller.Resolve<IEquipmentService>();
            _messageBroker = MainInstaller.Resolve<IMessageBrokerService>();

            if (_equipButton != null)
                _equipButton.onClick.AddListener(OnEquipClicked);
            if (_upgradeButton != null)
                _upgradeButton.onClick.AddListener(OnUpgradeClicked);
            if (_closeButton != null)
                _closeButton.onClick.AddListener(OnCloseClicked);
        }

        protected override void OnSetData()
        {
            RefreshDisplay();
        }

        protected override void OnOpened()
        {
            _messageBroker.Subscribe<EquipmentUpgradedMessage>(OnUpgraded);
            _messageBroker.Subscribe<EquipmentEquippedMessage>(OnEquipped);
            _messageBroker.Subscribe<EquipmentAcquiredMessage>(OnAcquired);
        }

        protected override void OnClosed()
        {
            _messageBroker?.Unsubscribe<EquipmentUpgradedMessage>(this);
            _messageBroker?.Unsubscribe<EquipmentEquippedMessage>(this);
            _messageBroker?.Unsubscribe<EquipmentAcquiredMessage>(this);
        }

        private void RefreshDisplay()
        {
            var entry = _equipmentService.Config.GetEntry(Data.EquipmentId);
            if (entry == null) return;

            var state = _equipmentService.Model.GetItemState(Data.EquipmentId);
            bool isUnlocked = state != null && state.IsUnlocked;

            RefreshInfo(entry, state, isUnlocked);
            RefreshEffects(entry, state, isUnlocked);
            RefreshButtons(entry, state, isUnlocked);
        }

        private void RefreshInfo(EquipmentEntry entry, EquipmentItemState state, bool isUnlocked)
        {
            if (_nameText != null)
                _nameText.text = entry.DisplayName;
            if (_gradeText != null)
                _gradeText.text = FormatGrade(entry.Grade);

            if (isUnlocked)
            {
                if (_levelText != null)
                    _levelText.text = $"Lv.{state.Level.Value}";
                if (_ownedCountText != null)
                {
                    int required = _equipmentService.GetUpgradeCost(Data.EquipmentId);
                    _ownedCountText.text = $"{state.OwnedCount.Value}/{required}";
                }
            }
            else
            {
                if (_levelText != null)
                    _levelText.text = "미해금";
                if (_ownedCountText != null)
                    _ownedCountText.text = $"{state?.OwnedCount.Value ?? 0}";
            }
        }

        private void RefreshEffects(EquipmentEntry entry, EquipmentItemState state, bool isUnlocked)
        {
            string statName = entry.Type == EquipmentType.Weapon ? "공격력" : "체력";

            if (isUnlocked)
            {
                BigNumber possEffect = _equipmentService.GetPossessionEffect(Data.EquipmentId);
                BigNumber equipEffect = _equipmentService.GetEquipEffect(Data.EquipmentId);

                if (_possessionEffectText != null)
                    _possessionEffectText.text = FormatEffectText(statName, possEffect);
                if (_equipEffectText != null)
                    _equipEffectText.text = FormatEffectText(statName, equipEffect);
            }
            else
            {
                BigNumber basePoss = new BigNumber(entry.BasePossessionEffect, 0);
                BigNumber baseEquip = new BigNumber(entry.BaseEquipEffect, 0);

                if (_possessionEffectText != null)
                    _possessionEffectText.text = FormatEffectText(statName, basePoss);
                if (_equipEffectText != null)
                    _equipEffectText.text = FormatEffectText(statName, baseEquip);
            }
        }

        private static string FormatEffectText(string statName, BigNumber value)
        {
            return $"{statName} <color=#FFD700>+{value.ToFormattedString(2)}%</color>";
        }

        private void RefreshButtons(EquipmentEntry entry, EquipmentItemState state, bool isUnlocked)
        {
            if (_equipButton != null)
            {
                bool isEquipped = _equipmentService.Model.GetEquippedId(entry.Type) == Data.EquipmentId;
                _equipButton.interactable = isUnlocked && !isEquipped;
                if (_equipButtonText != null)
                    _equipButtonText.text = isEquipped ? "장착중" : "장착";
            }

            if (_upgradeButton != null)
            {
                bool canUpgrade = _equipmentService.CanUpgrade(Data.EquipmentId);
                _upgradeButton.interactable = canUpgrade;
                if (_upgradeButtonText != null)
                    _upgradeButtonText.text = canUpgrade ? "강화" : "강화 불가";
            }
        }

        private static string FormatGrade(EquipmentGrade grade)
        {
            return grade switch
            {
                EquipmentGrade.Normal => "일반",
                EquipmentGrade.Advanced => "고급",
                EquipmentGrade.Rare => "희귀",
                EquipmentGrade.Epic => "영웅",
                EquipmentGrade.Legend => "전설",
                EquipmentGrade.Myth => "신화",
                EquipmentGrade.Special => "특별",
                _ => grade.ToString()
            };
        }

        private void OnEquipClicked()
        {
            _equipmentService.TryEquip(Data.EquipmentId);
        }

        private void OnUpgradeClicked()
        {
            _equipmentService.TryUpgrade(Data.EquipmentId);
        }

        private void OnCloseClicked()
        {
            Close(true);
        }

        private void OnUpgraded(EquipmentUpgradedMessage msg)
        {
            if (msg.Id == Data.EquipmentId)
                RefreshDisplay();
        }

        private void OnEquipped(EquipmentEquippedMessage msg)
        {
            var entry = _equipmentService.Config.GetEntry(Data.EquipmentId);
            if (entry != null && msg.SlotType == entry.Type)
                RefreshDisplay();
        }

        private void OnAcquired(EquipmentAcquiredMessage msg)
        {
            if (msg.Id == Data.EquipmentId)
                RefreshDisplay();
        }
    }
}
