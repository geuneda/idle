using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Geuneda.DataExtensions;
using Geuneda.Services;
using Geuneda.UiService;
using IdleRPG.Battle;
using IdleRPG.Core;
using IdleRPG.Dungeon;
using IdleRPG.Economy;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IdleRPG.UI
{
    /// <summary>
    /// 던전 입장 팝업에 전달하는 데이터.
    /// </summary>
    public struct DungeonEntryPopupData
    {
        /// <summary>선택된 던전 종류</summary>
        public DungeonType Type;
    }

    /// <summary>
    /// 던전 입장 팝업 프레젠터. 레벨 선택, 보상 표시, 소탕/전투 버튼을 제공한다.
    /// </summary>
    public class DungeonEntryPopupPresenter : UiPresenter<DungeonEntryPopupData>
    {
        [Header("UI Elements")]
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _levelText;
        [SerializeField] private CurrencySlotView _rewardSlotView;
        [SerializeField] private TMP_Text _entryCountText;
        [SerializeField] private Button _prevLevelButton;
        [SerializeField] private Button _nextLevelButton;
        [SerializeField] private Button _sweepButton;
        [SerializeField] private Button _battleButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private GameObject _clearedOverlay;

        private IDungeonService _dungeonService;
        private IBattleService _battleService;
        private IConfigsProvider _configsProvider;
        private CurrencyDisplayConfigCollection _displayCollection;
        private SpriteCache _spriteCache;
        private CancellationTokenSource _cts;
        private int _selectedLevel;

        /// <inheritdoc />
        protected override void OnInitialized()
        {
            _dungeonService = MainInstaller.Resolve<IDungeonService>();
            _battleService = MainInstaller.Resolve<IBattleService>();
            _configsProvider = MainInstaller.Resolve<IConfigsProvider>();

            if (_prevLevelButton != null) _prevLevelButton.onClick.AddListener(OnPrevLevel);
            if (_nextLevelButton != null) _nextLevelButton.onClick.AddListener(OnNextLevel);
            if (_sweepButton != null) _sweepButton.onClick.AddListener(OnSweep);
            if (_battleButton != null) _battleButton.onClick.AddListener(OnBattle);
            if (_closeButton != null) _closeButton.onClick.AddListener(() => Close(false));
        }

        /// <inheritdoc />
        protected override void OnOpened()
        {
            if (_rewardSlotView != null) _rewardSlotView.Clicked += OnRewardSlotClicked;
        }

        /// <inheritdoc />
        protected override void OnSetData()
        {
            _spriteCache?.Dispose();
            _spriteCache = new SpriteCache();
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            _displayCollection = _configsProvider.GetConfig<CurrencyDisplayConfigCollection>();

            _selectedLevel = _dungeonService.GetAvailableMaxLevel(Data.Type);
            RefreshDisplay();
        }

        private void OnPrevLevel()
        {
            if (_selectedLevel > 1)
            {
                _selectedLevel--;
                RefreshDisplay();
            }
        }

        private void OnNextLevel()
        {
            int maxAvailable = _dungeonService.GetAvailableMaxLevel(Data.Type);
            if (_selectedLevel < maxAvailable)
            {
                _selectedLevel++;
                RefreshDisplay();
            }
        }

        private void OnSweep()
        {
            if (_dungeonService.Sweep(Data.Type, _selectedLevel))
            {
                var config = _dungeonService.Config.GetConfig(Data.Type, _selectedLevel);
                var rewardType = DungeonConfigCollection.GetRewardCurrencyType(Data.Type);
                var rewardAmount = (BigNumber)config.RewardAmount;

                ShowToast($"{GetCurrencyName(rewardType)} x{rewardAmount.ToFormattedString()} 획득!");
                RefreshDisplay();
            }
        }

        private void OnBattle()
        {
            if (!_dungeonService.CanEnter(Data.Type, _selectedLevel)) return;

            EnterDungeonAsync().Forget();
        }

        private async UniTaskVoid EnterDungeonAsync()
        {
            Close(false);

            var uiService = _uiService;

            if (uiService.IsVisible<DungeonTabPresenter>())
                uiService.CloseUi<DungeonTabPresenter>();

            await uiService.OpenUiAsync<LoadingPresenter>();
            var loading = uiService.GetUi<LoadingPresenter>();
            if (loading != null) await loading.OpenTransitionTask;

            _battleService.Model.IsBattleActive.Value = false;

            _dungeonService.Enter(Data.Type, _selectedLevel);

            var overlayData = new DungeonBattleOverlayData
            {
                Type = Data.Type,
                Level = _selectedLevel,
                TotalWaves = _dungeonService.CurrentContext.TotalWaves,
                TimeLimit = _dungeonService.RemainingTime
            };
            await uiService.OpenUiAsync<DungeonBattleOverlayPresenter, DungeonBattleOverlayData>(overlayData);

            uiService.CloseUi<LoadingPresenter>();

            _battleService.StartBattle();
        }

        private void RefreshDisplay()
        {
            var type = Data.Type;
            var config = _dungeonService.Config.GetConfig(type, _selectedLevel);

            if (_titleText != null)
                _titleText.text = GetDungeonName(type);

            if (_levelText != null)
                _levelText.text = $"Lv.{_selectedLevel}";

            if (config != null && _rewardSlotView != null)
            {
                var rewardType = DungeonConfigCollection.GetRewardCurrencyType(type);
                var rewardAmount = (BigNumber)config.RewardAmount;
                var displayConfig = _displayCollection?.GetConfig(rewardType);
                var token = _cts?.Token ?? destroyCancellationToken;
                _rewardSlotView.Setup(rewardType, rewardAmount, displayConfig, _spriteCache, token);
            }

            int remaining = _dungeonService.GetRemainingEntries(type);
            int max = _dungeonService.GetMaxDailyEntries(type);
            if (_entryCountText != null)
                _entryCountText.text = $"({remaining}/{max})";

            bool isCleared = _selectedLevel <= _dungeonService.Model.GetClearedLevel(type);
            if (_clearedOverlay != null)
                _clearedOverlay.SetActive(isCleared);

            int maxAvailable = _dungeonService.GetAvailableMaxLevel(type);
            if (_prevLevelButton != null)
                _prevLevelButton.interactable = _selectedLevel > 1;
            if (_nextLevelButton != null)
                _nextLevelButton.interactable = _selectedLevel < maxAvailable;

            if (_sweepButton != null)
                _sweepButton.interactable = _dungeonService.CanSweep(type, _selectedLevel);
            if (_battleButton != null)
                _battleButton.interactable = _dungeonService.CanEnter(type, _selectedLevel);
        }

        /// <inheritdoc />
        protected override void OnClosed()
        {
            if (_rewardSlotView != null) _rewardSlotView.Clicked -= OnRewardSlotClicked;

            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;

            _spriteCache?.Dispose();
            _spriteCache = null;
            _displayCollection = null;
        }

        private async void OnRewardSlotClicked(CurrencyType currencyType)
        {
            try
            {
                var popupData = new CurrencyDetailPopupData { CurrencyType = currencyType };
                await _uiService.OpenUiAsync<CurrencyDetailPopupPresenter, CurrencyDetailPopupData>(popupData);
            }
            catch (OperationCanceledException) { }
        }

        private async void ShowToast(string message)
        {
            var toastData = new ToastData { Message = message };
            await _uiService.OpenUiAsync<ToastPresenter, ToastData>(toastData);
        }

        private static string GetDungeonName(DungeonType type) => DungeonDisplayHelper.GetDungeonName(type);

        private static string GetCurrencyName(CurrencyType type) => DungeonDisplayHelper.GetCurrencyName(type);
    }
}
