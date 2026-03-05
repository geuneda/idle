using Geuneda.Services;
using Geuneda.UiService;
using IdleRPG.Dungeon;
using TMPro;
using UnityEngine;

namespace IdleRPG.UI
{
    /// <summary>
    /// 던전 탭 컨텐츠 프레젠터. 보석/골드/유물 던전 목록을 표시하고 입장 팝업을 연다.
    /// </summary>
    public class DungeonTabPresenter : UiPresenter
    {
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private DungeonListItemView _gemDungeonItem;
        [SerializeField] private DungeonListItemView _goldDungeonItem;
        [SerializeField] private DungeonListItemView _relicDungeonItem;
        [SerializeField] private DungeonListItemView _traitDungeonItem;
        [SerializeField] private DungeonListItemView _ringDungeonItem;

        private IDungeonService _dungeonService;

        /// <inheritdoc />
        protected override void OnInitialized()
        {
            _dungeonService = MainInstaller.Resolve<IDungeonService>();
        }

        /// <inheritdoc />
        protected override void OnOpened()
        {
            if (_titleText != null)
                _titleText.text = "던전";

            RefreshDungeonList();
        }

        private void RefreshDungeonList()
        {
            SetupDungeonItem(_gemDungeonItem, DungeonType.Gem, DungeonDisplayHelper.GetDungeonName(DungeonType.Gem), false);
            SetupDungeonItem(_goldDungeonItem, DungeonType.Gold, DungeonDisplayHelper.GetDungeonName(DungeonType.Gold), false);
            SetupDungeonItem(_relicDungeonItem, DungeonType.Relic, DungeonDisplayHelper.GetDungeonName(DungeonType.Relic), false);

            if (_traitDungeonItem != null)
                _traitDungeonItem.SetData("특성던전", 0, 0, true, null);

            if (_ringDungeonItem != null)
                _ringDungeonItem.SetData("링던전", 0, 0, true, null);
        }

        private void SetupDungeonItem(DungeonListItemView item, DungeonType type, string name, bool isLocked)
        {
            if (item == null) return;

            int maxLevel = _dungeonService.GetAvailableMaxLevel(type);
            int remaining = _dungeonService.GetRemainingEntries(type);
            int max = _dungeonService.GetMaxDailyEntries(type);
            string displayName = $"{name} Lv.{maxLevel}";

            item.SetData(displayName, remaining, max, isLocked, () => OpenEntryPopup(type));
        }

        private async void OpenEntryPopup(DungeonType type)
        {
            var data = new DungeonEntryPopupData
            {
                Type = type
            };
            await _uiService.OpenUiAsync<DungeonEntryPopupPresenter, DungeonEntryPopupData>(data);
        }
    }
}
