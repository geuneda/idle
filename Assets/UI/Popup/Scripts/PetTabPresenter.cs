using System.Collections.Generic;
using Geuneda.Services;
using Geuneda.UiService;
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
    /// 펫 탭 프레젠터. 장착 슬롯과 펫 도감 그리드를 표시하고
    /// 빠른 장착, 전체 강화 기능을 제공한다.
    /// </summary>
    public class PetTabPresenter : UiPresenter
    {
        [Header("Equip Slots")]
        [SerializeField] private PetSlotView[] _slotViews;

        [Header("Collection Grid")]
        [SerializeField] private Transform _gridContent;
        [SerializeField] private PetCollectionItemView _itemPrefab;

        [Header("Summary")]
        [SerializeField] private TMP_Text _totalPossessionEffectText;

        [Header("Buttons")]
        [SerializeField] private Button _quickEquipButton;
        [SerializeField] private Button _upgradeAllButton;

        private IPetService _petService;
        private IMessageBrokerService _messageBroker;
        private readonly List<PetCollectionItemView> _itemViews = new();
        private readonly List<PetEntry> _sortedEntries = new();
        private readonly List<AsyncOperationHandle<Sprite>> _spriteHandles = new();
        private int _refreshSession;

        protected override void OnInitialized()
        {
            _petService = MainInstaller.Resolve<IPetService>();
            _messageBroker = MainInstaller.Resolve<IMessageBrokerService>();

            if (_quickEquipButton != null)
                _quickEquipButton.onClick.AddListener(OnQuickEquipClicked);
            if (_upgradeAllButton != null)
                _upgradeAllButton.onClick.AddListener(OnUpgradeAllClicked);
        }

        protected override void OnOpened()
        {
            _messageBroker.Subscribe<PetAcquiredMessage>(OnPetAcquired);
            _messageBroker.Subscribe<PetUpgradedMessage>(OnPetUpgraded);
            _messageBroker.Subscribe<PetEquippedMessage>(OnPetEquipped);
            _messageBroker.Subscribe<PetUnequippedMessage>(OnPetUnequipped);
            _messageBroker.Subscribe<PetEffectsChangedMessage>(OnEffectsChanged);

            BuildGrid();
            RefreshAll();
        }

        protected override void OnClosed()
        {
            _messageBroker?.Unsubscribe<PetAcquiredMessage>(this);
            _messageBroker?.Unsubscribe<PetUpgradedMessage>(this);
            _messageBroker?.Unsubscribe<PetEquippedMessage>(this);
            _messageBroker?.Unsubscribe<PetUnequippedMessage>(this);
            _messageBroker?.Unsubscribe<PetEffectsChangedMessage>(this);

            ReleaseSpriteHandles();
            ClearGrid();
        }

        private void BuildGrid()
        {
            ClearGrid();
            BuildSortedEntries();

            if (_itemPrefab == null || _gridContent == null) return;

            foreach (var entry in _sortedEntries)
            {
                var itemView = Instantiate(_itemPrefab, _gridContent);
                itemView.SetPetId(entry.Id);
                itemView.Clicked += OnItemClicked;
                _itemViews.Add(itemView);
            }
        }

        private void BuildSortedEntries()
        {
            _sortedEntries.Clear();
            var entries = _petService.Config.GetAllEntries();

            for (int i = 0; i < entries.Count; i++)
            {
                _sortedEntries.Add(entries[i]);
            }

            // 등급 오름차순 -> ID 오름차순
            _sortedEntries.Sort((a, b) =>
            {
                int gradeCompare = ((int)a.Grade).CompareTo((int)b.Grade);
                return gradeCompare != 0 ? gradeCompare : a.Id.CompareTo(b.Id);
            });
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
            _refreshSession++;
            ReleaseSpriteHandles();
            RefreshSlots();
            RefreshGridItems();
            RefreshTotalEffect();
        }

        private void RefreshSlots()
        {
            if (_slotViews == null) return;

            for (int i = 0; i < _slotViews.Length; i++)
            {
                if (_slotViews[i] == null) continue;

                int slotIndex = i;

                if (!_petService.IsSlotUnlocked(i))
                {
                    var slotConfig = _petService.Config.GetSlotConfig(i);
                    string condition = slotConfig != null
                        ? $"{slotConfig.UnlockChapter}-{slotConfig.UnlockStage}"
                        : "???";
                    _slotViews[i].SetLocked(condition);
                    _slotViews[i].SetOnClickListener(null);
                    _slotViews[i].SetOnUnequipClickListener(null);
                    continue;
                }

                int equippedId = _petService.Model.GetEquippedPetId(i);
                if (equippedId == PetModel.UNEQUIPPED)
                {
                    _slotViews[i].SetEmpty();
                    _slotViews[i].SetOnClickListener(null);
                    _slotViews[i].SetOnUnequipClickListener(null);
                }
                else
                {
                    var entry = _petService.Config.GetEntry(equippedId);
                    var state = _petService.Model.GetItemState(equippedId);
                    if (entry != null && state != null)
                    {
                        _slotViews[i].SetEquipped(null);
                        var slotView = _slotViews[i];
                        LoadSpriteAsync(entry.SpriteKey, sprite => slotView.UpdateIcon(sprite));
                    }

                    _slotViews[i].SetOnClickListener(() => OnSlotClicked(slotIndex));
                    _slotViews[i].SetOnUnequipClickListener(() => OnUnequipSlotClicked(slotIndex));
                }
            }
        }

        private void RefreshGridItems()
        {
            for (int i = 0; i < _itemViews.Count; i++)
            {
                RefreshGridItem(_itemViews[i]);
            }
        }

        private void RefreshGridItem(PetCollectionItemView view)
        {
            int petId = view.PetId;
            var entry = _petService.Config.GetEntry(petId);
            if (entry == null) return;

            var state = _petService.Model.GetItemState(petId);
            bool isUnlocked = state != null && state.IsUnlocked;
            Color gradeColor = PetGradeHelper.GetBorderColor(entry.Grade);

            if (isUnlocked && _petService.Model.IsEquipped(petId))
            {
                view.SetEquipped(null, gradeColor, state.Level.Value);
            }
            else if (!isUnlocked)
            {
                view.SetLocked(null, gradeColor);
            }
            else
            {
                int ownedCount = state.OwnedCount.Value;
                int requiredCount = _petService.GetUpgradeCost(petId);
                bool canUpgrade = _petService.CanUpgrade(petId);
                view.SetNormal(null, gradeColor, state.Level.Value, ownedCount, requiredCount,
                    canUpgrade);
            }

            LoadSpriteAsync(entry.SpriteKey, sprite => view.UpdateIcon(sprite));
        }

        private void RefreshTotalEffect()
        {
            if (_totalPossessionEffectText == null) return;

            BigNumber totalEffect = _petService.TotalPossessionAttackPercent;
            _totalPossessionEffectText.text =
                $"모든 보유효과 : 공격 +<color=#FFD700>{totalEffect.ToFormattedString(2)}</color>%";
        }

        private void OnSlotClicked(int slotIndex)
        {
            int equippedId = _petService.Model.GetEquippedPetId(slotIndex);
            if (equippedId != PetModel.UNEQUIPPED)
            {
                _petService.Unequip(slotIndex);
            }
        }

        /// <summary>
        /// 슬롯의 장착해제 버튼 클릭 시 해당 슬롯의 펫을 해제한다.
        /// </summary>
        private void OnUnequipSlotClicked(int slotIndex)
        {
            _petService.Unequip(slotIndex);
        }

        private async void OnItemClicked(int petId)
        {
            await _uiService.OpenUiAsync<PetDetailPopupPresenter, PetDetailPopupData>(
                new PetDetailPopupData { PetId = petId });
        }

        private void OnQuickEquipClicked()
        {
            _petService.QuickEquip();
        }

        private void OnUpgradeAllClicked()
        {
            _petService.UpgradeAll();
        }

        private void OnPetAcquired(PetAcquiredMessage msg)
        {
            RefreshAll();
        }

        private void OnPetUpgraded(PetUpgradedMessage msg)
        {
            RefreshAll();
        }

        private void OnPetEquipped(PetEquippedMessage msg)
        {
            RefreshAll();
        }

        private void OnPetUnequipped(PetUnequippedMessage msg)
        {
            RefreshAll();
        }

        private void OnEffectsChanged(PetEffectsChangedMessage msg)
        {
            RefreshTotalEffect();
        }

        /// <summary>
        /// Addressable 스프라이트를 비동기로 로드하고 완료 시 콜백을 호출한다.
        /// </summary>
        /// <param name="spriteKey">Addressable 스프라이트 키</param>
        /// <param name="onLoaded">로드 완료 콜백</param>
        private async void LoadSpriteAsync(string spriteKey, System.Action<Sprite> onLoaded)
        {
            if (string.IsNullOrEmpty(spriteKey)) return;

            int session = _refreshSession;

            try
            {
                var handle = Addressables.LoadAssetAsync<Sprite>(spriteKey);
                _spriteHandles.Add(handle);
                var sprite = await handle.Task;

                if (this == null || session != _refreshSession) return;
                onLoaded?.Invoke(sprite);
            }
            catch (System.Exception ex)
            {
                DevLog.LogWarning($"[PetTab] 스프라이트 로드 실패: {ex.Message}");
            }
        }

        /// <summary>
        /// 로드된 스프라이트 핸들을 모두 해제한다.
        /// </summary>
        private void ReleaseSpriteHandles()
        {
            foreach (var handle in _spriteHandles)
            {
                if (handle.IsValid()) Addressables.Release(handle);
            }
            _spriteHandles.Clear();
        }
    }
}
