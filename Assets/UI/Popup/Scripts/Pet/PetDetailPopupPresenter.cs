using Geuneda.Services;
using Geuneda.UiService;
using IdleRPG.Battle;
using IdleRPG.Core;
using IdleRPG.Pet;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace IdleRPG.UI
{
    /// <summary>
    /// 펫 상세 팝업 프레젠터. 개별 펫의 상세 정보와 장착/해제/강화 버튼을 표시한다.
    /// </summary>
    public class PetDetailPopupPresenter : UiPresenter<PetDetailPopupData>
    {
        [Header("Pet Info")]
        [SerializeField] private Image _iconImage;
        [SerializeField] private Image _gradeBgImage;
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _gradeText;
        [SerializeField] private TMP_Text _levelText;

        [Header("Species")]
        [SerializeField] private TMP_Text _speciesText;

        [Header("Effects")]
        [SerializeField] private TMP_Text _possessionEffectText;
        [SerializeField] private TMP_Text _ownedCountText;

        [Header("Combat")]
        [SerializeField] private TMP_Text _attackPowerText;
        [SerializeField] private TMP_Text _attackSpeedText;

        [Header("Buttons")]
        [SerializeField] private Button _equipButton;
        [SerializeField] private TMP_Text _equipButtonText;
        [SerializeField] private Button _upgradeButton;
        [SerializeField] private TMP_Text _upgradeButtonText;
        [SerializeField] private Button _closeButton;

        private IPetService _petService;
        private IBattleService _battleService;
        private IMessageBrokerService _messageBroker;
        private AsyncOperationHandle<Sprite> _iconHandle;
        private int _iconLoadSession;
        private string _currentIconKey;

        protected override void OnInitialized()
        {
            _petService = MainInstaller.Resolve<IPetService>();
            _battleService = MainInstaller.Resolve<IBattleService>();
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
            _messageBroker.Subscribe<PetAcquiredMessage>(OnPetAcquired);
            _messageBroker.Subscribe<PetUpgradedMessage>(OnPetUpgraded);
            _messageBroker.Subscribe<PetEquippedMessage>(OnPetEquipped);
            _messageBroker.Subscribe<PetUnequippedMessage>(OnPetUnequipped);
        }

        protected override void OnClosed()
        {
            _messageBroker?.Unsubscribe<PetAcquiredMessage>(this);
            _messageBroker?.Unsubscribe<PetUpgradedMessage>(this);
            _messageBroker?.Unsubscribe<PetEquippedMessage>(this);
            _messageBroker?.Unsubscribe<PetUnequippedMessage>(this);

            ReleaseIconHandle();
        }

        private void RefreshDisplay()
        {
            var entry = _petService.Config.GetEntry(Data.PetId);
            if (entry == null) return;

            var state = _petService.Model.GetItemState(Data.PetId);
            bool isUnlocked = state != null && state.IsUnlocked;

            RefreshInfo(entry, state, isUnlocked);
            RefreshSpecies(entry);
            RefreshEffects(entry, state, isUnlocked);
            RefreshCombat(entry, state, isUnlocked);
            RefreshButtons(entry, state, isUnlocked);
            LoadIconSprite(entry.SpriteKey);
        }

        private void RefreshInfo(PetEntry entry, CollectibleItemState state, bool isUnlocked)
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

        private void RefreshSpecies(PetEntry entry)
        {
            if (_speciesText != null)
                _speciesText.text = entry.Species;
        }

        private void RefreshEffects(PetEntry entry, CollectibleItemState state, bool isUnlocked)
        {
            if (_possessionEffectText != null)
            {
                if (isUnlocked)
                {
                    BigNumber possEffect = _petService.GetPossessionEffect(Data.PetId);
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
                int required = _petService.GetUpgradeCost(Data.PetId);
                if (required > 0)
                    _ownedCountText.text = $"{owned}/{required}";
                else
                    _ownedCountText.text = $"{owned}";
            }
        }

        private void RefreshCombat(PetEntry entry, CollectibleItemState state, bool isUnlocked)
        {
            if (_attackPowerText != null)
            {
                int level = isUnlocked ? state.Level.Value : 1;
                double damagePercent = PetFormula.CalcDamagePercent(entry, level);
                BigNumber heroAttack = _battleService.HeroModel.Attack.Value;
                BigNumber petDamage = PetFormula.CalcPetDamage(heroAttack, damagePercent);
                _attackPowerText.text = petDamage.ToFormattedString(2);
            }

            if (_attackSpeedText != null)
            {
                _attackSpeedText.text = $"{entry.AttackSpeed:0.#}/초";
            }
        }

        private void RefreshButtons(PetEntry entry, CollectibleItemState state, bool isUnlocked)
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

            bool isEquipped = _petService.Model.IsEquipped(Data.PetId);
            if (_equipButtonText != null)
                _equipButtonText.text = isEquipped ? "장착해제" : "장착";

            bool canUpgrade = _petService.CanUpgrade(Data.PetId);
            int required = _petService.GetUpgradeCost(Data.PetId);
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
            var state = _petService.Model.GetItemState(Data.PetId);
            if (state == null || !state.IsUnlocked) return;

            bool isEquipped = _petService.Model.IsEquipped(Data.PetId);
            if (isEquipped)
            {
                int slotIndex = _petService.Model.FindEquippedSlotIndex(Data.PetId);
                if (slotIndex >= 0)
                    _petService.Unequip(slotIndex);
            }
            else
            {
                TryEquipToFirstEmptySlot();
            }
        }

        private void OnUpgradeClicked()
        {
            var state = _petService.Model.GetItemState(Data.PetId);
            if (state == null || !state.IsUnlocked) return;

            _petService.TryUpgrade(Data.PetId);
        }

        private void TryEquipToFirstEmptySlot()
        {
            for (int i = 0; i < PetModel.MAX_SLOTS; i++)
            {
                if (!_petService.IsSlotUnlocked(i)) continue;
                if (_petService.Model.GetEquippedPetId(i) != PetModel.UNEQUIPPED) continue;

                _petService.TryEquip(Data.PetId, i);
                return;
            }

            // 빈 슬롯이 없으면 첫 번째 해금된 슬롯에 교체
            for (int i = 0; i < PetModel.MAX_SLOTS; i++)
            {
                if (!_petService.IsSlotUnlocked(i)) continue;

                _petService.TryEquip(Data.PetId, i);
                return;
            }
        }

        private void OnCloseClicked()
        {
            Close(true);
        }

        private void OnPetAcquired(PetAcquiredMessage msg)
        {
            if (msg.Id == Data.PetId)
                RefreshDisplay();
        }

        private void OnPetUpgraded(PetUpgradedMessage msg)
        {
            if (msg.Id == Data.PetId)
                RefreshDisplay();
        }

        private void OnPetEquipped(PetEquippedMessage msg)
        {
            if (msg.NewId == Data.PetId || msg.PreviousId == Data.PetId)
                RefreshDisplay();
        }

        private void OnPetUnequipped(PetUnequippedMessage msg)
        {
            if (msg.PreviousId == Data.PetId)
                RefreshDisplay();
        }

        /// <summary>
        /// Addressable에서 아이콘 스프라이트를 비동기로 로드하여 표시한다.
        /// </summary>
        /// <param name="spriteKey">Addressable 스프라이트 키</param>
        private async void LoadIconSprite(string spriteKey)
        {
            if (spriteKey == _currentIconKey) return;

            ReleaseIconHandle();
            _currentIconKey = spriteKey;
            if (string.IsNullOrEmpty(spriteKey) || _iconImage == null) return;

            _iconLoadSession++;
            int session = _iconLoadSession;

            try
            {
                _iconHandle = Addressables.LoadAssetAsync<Sprite>(spriteKey);
                var sprite = await _iconHandle.Task;

                if (this == null || session != _iconLoadSession) return;
                if (_iconImage != null)
                {
                    _iconImage.sprite = sprite;
                    _iconImage.enabled = sprite != null;
                }
            }
            catch (System.Exception ex)
            {
                DevLog.LogWarning($"[PetDetail] 아이콘 로드 실패: {ex.Message}");
            }
        }

        /// <summary>
        /// 아이콘 스프라이트 핸들을 해제한다.
        /// </summary>
        private void ReleaseIconHandle()
        {
            _currentIconKey = null;
            if (_iconHandle.IsValid())
            {
                Addressables.Release(_iconHandle);
                _iconHandle = default;
            }
        }
    }
}
