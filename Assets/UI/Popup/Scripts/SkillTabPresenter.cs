using System.Collections.Generic;
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
    /// 스킬 탭 프레젠터. 장착 슬롯과 스킬 도감 그리드를 표시하고
    /// 빠른 장착, 전체 강화 기능을 제공한다.
    /// </summary>
    public class SkillTabPresenter : UiPresenter
    {
        [Header("Equip Slots")]
        [SerializeField] private SkillSlotView[] _slotViews;

        [Header("Collection Grid")]
        [SerializeField] private Transform _gridContent;
        [SerializeField] private SkillCollectionItemView _itemPrefab;

        [Header("Summary")]
        [SerializeField] private TMP_Text _totalPossessionEffectText;

        [Header("Buttons")]
        [SerializeField] private Button _quickEquipButton;
        [SerializeField] private Button _upgradeAllButton;

        private ISkillService _skillService;
        private IMessageBrokerService _messageBroker;
        private readonly List<SkillCollectionItemView> _itemViews = new();
        private readonly List<SkillEntry> _sortedEntries = new();

        protected override void OnInitialized()
        {
            _skillService = MainInstaller.Resolve<ISkillService>();
            _messageBroker = MainInstaller.Resolve<IMessageBrokerService>();

            if (_quickEquipButton != null)
                _quickEquipButton.onClick.AddListener(OnQuickEquipClicked);
            if (_upgradeAllButton != null)
                _upgradeAllButton.onClick.AddListener(OnUpgradeAllClicked);
        }

        protected override void OnOpened()
        {
            _messageBroker.Subscribe<SkillAcquiredMessage>(OnSkillAcquired);
            _messageBroker.Subscribe<SkillUpgradedMessage>(OnSkillUpgraded);
            _messageBroker.Subscribe<SkillEquippedMessage>(OnSkillEquipped);
            _messageBroker.Subscribe<SkillUnequippedMessage>(OnSkillUnequipped);
            _messageBroker.Subscribe<SkillEffectsChangedMessage>(OnEffectsChanged);

            BuildGrid();
            RefreshAll();
        }

        protected override void OnClosed()
        {
            _messageBroker?.Unsubscribe<SkillAcquiredMessage>(this);
            _messageBroker?.Unsubscribe<SkillUpgradedMessage>(this);
            _messageBroker?.Unsubscribe<SkillEquippedMessage>(this);
            _messageBroker?.Unsubscribe<SkillUnequippedMessage>(this);
            _messageBroker?.Unsubscribe<SkillEffectsChangedMessage>(this);

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
                itemView.SetSkillId(entry.Id);
                itemView.Clicked += OnItemClicked;
                _itemViews.Add(itemView);
            }
        }

        private void BuildSortedEntries()
        {
            _sortedEntries.Clear();
            var entries = _skillService.Config.GetAllEntries();

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

                if (!_skillService.IsSlotUnlocked(i))
                {
                    var slotConfig = _skillService.Config.GetSlotConfig(i);
                    string condition = slotConfig != null
                        ? $"{slotConfig.UnlockChapter}-{slotConfig.UnlockStage}"
                        : "???";
                    _slotViews[i].SetLocked(condition);
                    _slotViews[i].SetOnClickListener(null);
                    _slotViews[i].SetOnUnequipClickListener(null);
                    continue;
                }

                int equippedId = _skillService.Model.GetEquippedSkillId(i);
                if (equippedId == SkillModel.UNEQUIPPED)
                {
                    _slotViews[i].SetEmpty();
                    _slotViews[i].SetOnClickListener(null);
                    _slotViews[i].SetOnUnequipClickListener(null);
                }
                else
                {
                    var entry = _skillService.Config.GetEntry(equippedId);
                    var state = _skillService.Model.GetItemState(equippedId);
                    if (entry != null && state != null)
                    {
                        Color gradeColor = SkillGradeHelper.GetBackgroundColor(entry.Grade);
                        // TODO: 스프라이트는 Addressable로 로드해야 하므로 null 전달, 프리팹에서 기본 아이콘 사용
                        _slotViews[i].SetEquipped(null);
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

        private void RefreshGridItem(SkillCollectionItemView view)
        {
            int skillId = view.SkillId;
            var entry = _skillService.Config.GetEntry(skillId);
            if (entry == null) return;

            var state = _skillService.Model.GetItemState(skillId);
            bool isUnlocked = state != null && state.IsUnlocked;
            Color gradeColor = SkillGradeHelper.GetBorderColor(entry.Grade);

            if (isUnlocked && _skillService.Model.IsEquipped(skillId))
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
                int requiredCount = _skillService.GetUpgradeCost(skillId);
                bool canUpgrade = _skillService.CanUpgrade(skillId);
                view.SetNormal(null, gradeColor, state.Level.Value, ownedCount, requiredCount,
                    canUpgrade);
            }
        }

        private void RefreshTotalEffect()
        {
            if (_totalPossessionEffectText == null) return;

            BigNumber totalEffect = _skillService.TotalPossessionAttackPercent;
            _totalPossessionEffectText.text =
                $"모든 보유효과 : 공격 +<color=#FFD700>{totalEffect.ToFormattedString(2)}</color>%";
        }

        private void OnSlotClicked(int slotIndex)
        {
            int equippedId = _skillService.Model.GetEquippedSkillId(slotIndex);
            if (equippedId != SkillModel.UNEQUIPPED)
            {
                _skillService.Unequip(slotIndex);
            }
        }

        /// <summary>
        /// 슬롯의 장착해제 버튼 클릭 시 해당 슬롯의 스킬을 해제한다.
        /// </summary>
        private void OnUnequipSlotClicked(int slotIndex)
        {
            _skillService.Unequip(slotIndex);
        }

        private async void OnItemClicked(int skillId)
        {
            await _uiService.OpenUiAsync<SkillDetailPopupPresenter, SkillDetailPopupData>(
                new SkillDetailPopupData { SkillId = skillId });
        }

        private void OnQuickEquipClicked()
        {
            _skillService.QuickEquip();
        }

        private void OnUpgradeAllClicked()
        {
            _skillService.UpgradeAll();
        }

        private void OnSkillAcquired(SkillAcquiredMessage msg)
        {
            RefreshAll();
        }

        private void OnSkillUpgraded(SkillUpgradedMessage msg)
        {
            RefreshAll();
        }

        private void OnSkillEquipped(SkillEquippedMessage msg)
        {
            RefreshAll();
        }

        private void OnSkillUnequipped(SkillUnequippedMessage msg)
        {
            RefreshAll();
        }

        private void OnEffectsChanged(SkillEffectsChangedMessage msg)
        {
            RefreshTotalEffect();
        }
    }
}
