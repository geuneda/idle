using Geuneda.Services;
using Geuneda.UiService;
using IdleRPG.Core;
using IdleRPG.Skill;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IdleRPG.UI
{
    /// <summary>
    /// 스킬 상세 팝업 프레젠터. 개별 스킬의 상세 정보와 장착/해제/강화 버튼을 표시한다.
    /// </summary>
    public class SkillDetailPopupPresenter : UiPresenter<SkillDetailPopupData>
    {
        [Header("Skill Info")]
        [SerializeField] private Image _iconImage;
        [SerializeField] private Image _gradeBgImage;
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _gradeText;
        [SerializeField] private TMP_Text _levelText;

        [Header("Description")]
        [SerializeField] private TMP_Text _descriptionText;

        [Header("Effects")]
        [SerializeField] private TMP_Text _possessionEffectText;
        [SerializeField] private TMP_Text _ownedCountText;

        [Header("Cooldown")]
        [SerializeField] private TMP_Text _cooldownText;

        [Header("Buttons")]
        [SerializeField] private Button _equipButton;
        [SerializeField] private TMP_Text _equipButtonText;
        [SerializeField] private Button _upgradeButton;
        [SerializeField] private TMP_Text _upgradeButtonText;
        [SerializeField] private Button _closeButton;

        private ISkillService _skillService;
        private IMessageBrokerService _messageBroker;

        protected override void OnInitialized()
        {
            _skillService = MainInstaller.Resolve<ISkillService>();
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
            _messageBroker.Subscribe<SkillAcquiredMessage>(OnSkillAcquired);
            _messageBroker.Subscribe<SkillUpgradedMessage>(OnSkillUpgraded);
            _messageBroker.Subscribe<SkillEquippedMessage>(OnSkillEquipped);
            _messageBroker.Subscribe<SkillUnequippedMessage>(OnSkillUnequipped);
        }

        protected override void OnClosed()
        {
            _messageBroker?.Unsubscribe<SkillAcquiredMessage>(this);
            _messageBroker?.Unsubscribe<SkillUpgradedMessage>(this);
            _messageBroker?.Unsubscribe<SkillEquippedMessage>(this);
            _messageBroker?.Unsubscribe<SkillUnequippedMessage>(this);
        }

        private void RefreshDisplay()
        {
            var entry = _skillService.Config.GetEntry(Data.SkillId);
            if (entry == null) return;

            var state = _skillService.Model.GetItemState(Data.SkillId);
            bool isUnlocked = state != null && state.IsUnlocked;

            RefreshInfo(entry, state, isUnlocked);
            RefreshDescription(entry, state, isUnlocked);
            RefreshEffects(entry, state, isUnlocked);
            RefreshCooldown(entry);
            RefreshButtons(entry, state, isUnlocked);
        }

        private void RefreshInfo(SkillEntry entry, CollectibleItemState state, bool isUnlocked)
        {
            if (_nameText != null)
                _nameText.text = entry.DisplayName;

            if (_gradeText != null)
                _gradeText.text = ItemGradeHelper.GetGradeText(entry.Grade);

            if (_gradeBgImage != null)
                _gradeBgImage.color = ItemGradeHelper.GetBackgroundColor(entry.Grade);

            if (isUnlocked)
            {
                if (_levelText != null)
                    _levelText.text = $"Lv.{state.Level.Value}";
            }
            else
            {
                if (_levelText != null)
                    _levelText.text = "미해금";
            }
        }

        private void RefreshDescription(SkillEntry entry, CollectibleItemState state, bool isUnlocked)
        {
            if (_descriptionText == null) return;

            int level = isUnlocked ? state.Level.Value : 1;
            _descriptionText.text = SkillFormula.FormatDescription(entry, level);
        }

        private void RefreshEffects(SkillEntry entry, CollectibleItemState state, bool isUnlocked)
        {
            if (_possessionEffectText != null)
            {
                if (isUnlocked)
                {
                    BigNumber possEffect = _skillService.GetPossessionEffect(Data.SkillId);
                    _possessionEffectText.text =
                        $"공격력 +{UiColorConstants.GoldTagOpen}{possEffect.ToFormattedString(2)}%{UiColorConstants.GoldTagClose}";
                }
                else
                {
                    BigNumber basePoss = new BigNumber(entry.BasePossessionEffect, 0);
                    _possessionEffectText.text =
                        $"공격력 +{UiColorConstants.GoldTagOpen}{basePoss.ToFormattedString(2)}%{UiColorConstants.GoldTagClose}";
                }
            }

            if (_ownedCountText != null)
            {
                int owned = state?.OwnedCount.Value ?? 0;
                int required = _skillService.GetUpgradeCost(Data.SkillId);
                if (required > 0)
                    _ownedCountText.text = $"{owned}/{required}";
                else
                    _ownedCountText.text = $"{owned}";
            }
        }

        private void RefreshCooldown(SkillEntry entry)
        {
            if (_cooldownText == null) return;

            float cd = entry.Cooldown;
            _cooldownText.text = cd % 1f == 0f ? $"{(int)cd}s" : $"{cd:0.#}s";
        }

        private void RefreshButtons(SkillEntry entry, CollectibleItemState state, bool isUnlocked)
        {
            if (!isUnlocked)
            {
                SetButtonActive(_equipButton, true);
                SetButtonActive(_upgradeButton, false);

                if (_equipButton != null)
                    _equipButton.interactable = false;
                if (_equipButtonText != null)
                    _equipButtonText.text = "미보유";
                return;
            }

            SetButtonActive(_equipButton, true);
            SetButtonActive(_upgradeButton, true);

            if (_equipButton != null)
                _equipButton.interactable = true;

            bool isEquipped = _skillService.Model.IsEquipped(Data.SkillId);
            if (_equipButtonText != null)
                _equipButtonText.text = isEquipped ? "장착해제" : "장착";

            bool canUpgrade = _skillService.CanUpgrade(Data.SkillId);
            int required = _skillService.GetUpgradeCost(Data.SkillId);
            int owned = state?.OwnedCount.Value ?? 0;

            if (_upgradeButton != null)
                _upgradeButton.interactable = canUpgrade;
            if (_upgradeButtonText != null)
                _upgradeButtonText.text = $"강화 ({owned}/{required})";
        }

        private void SetButtonActive(Button button, bool active)
        {
            if (button != null)
                button.gameObject.SetActive(active);
        }

        private void OnEquipClicked()
        {
            var state = _skillService.Model.GetItemState(Data.SkillId);
            if (state == null || !state.IsUnlocked) return;

            bool isEquipped = _skillService.Model.IsEquipped(Data.SkillId);
            if (isEquipped)
            {
                int slotIndex = _skillService.Model.FindEquippedSlotIndex(Data.SkillId);
                if (slotIndex >= 0)
                    _skillService.Unequip(slotIndex);
            }
            else
            {
                TryEquipToFirstEmptySlot();
            }
        }

        private void OnUpgradeClicked()
        {
            var state = _skillService.Model.GetItemState(Data.SkillId);
            if (state == null || !state.IsUnlocked) return;

            _skillService.TryUpgrade(Data.SkillId);
        }

        private void TryEquipToFirstEmptySlot()
        {
            for (int i = 0; i < SkillModel.MAX_SLOTS; i++)
            {
                if (!_skillService.IsSlotUnlocked(i)) continue;
                if (_skillService.Model.GetEquippedSkillId(i) != SkillModel.UNEQUIPPED) continue;

                _skillService.TryEquip(Data.SkillId, i);
                return;
            }

            // 빈 슬롯이 없으면 첫 번째 해금된 슬롯에 교체
            for (int i = 0; i < SkillModel.MAX_SLOTS; i++)
            {
                if (!_skillService.IsSlotUnlocked(i)) continue;

                _skillService.TryEquip(Data.SkillId, i);
                return;
            }
        }

        private void OnCloseClicked()
        {
            Close(true);
        }

        private void OnSkillAcquired(SkillAcquiredMessage msg)
        {
            if (msg.Id == Data.SkillId)
                RefreshDisplay();
        }

        private void OnSkillUpgraded(SkillUpgradedMessage msg)
        {
            if (msg.Id == Data.SkillId)
                RefreshDisplay();
        }

        private void OnSkillEquipped(SkillEquippedMessage msg)
        {
            if (msg.NewId == Data.SkillId || msg.PreviousId == Data.SkillId)
                RefreshDisplay();
        }

        private void OnSkillUnequipped(SkillUnequippedMessage msg)
        {
            if (msg.PreviousId == Data.SkillId)
                RefreshDisplay();
        }
    }
}
