using Geuneda.Services;
using Geuneda.UiService;
using IdleRPG.Economy;
using IdleRPG.Mine;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IdleRPG.UI
{
    /// <summary>
    /// 광산 메인 화면 프레젠터.
    /// </summary>
    public class MinePresenter : UiPresenter
    {
        [Header("HUD")]
        [SerializeField] private CurrencySlotView _goldSlot;
        [SerializeField] private CurrencySlotView _gemSlot;
        [SerializeField] private CurrencySlotView _oreSlot;
        [SerializeField] private TMP_Text _floorText;
        [SerializeField] private Button _infoButton;

        [Header("Pickaxe")]
        [SerializeField] private TMP_Text _pickaxeCountText;
        [SerializeField] private TMP_Text _pickaxeTimerText;

        [Header("Progress")]
        [SerializeField] private MineProgressSliderView _progressSlider;
        [SerializeField] private Button _progressDetailButton;

        [Header("Board")]
        [SerializeField] private MineBlockView[] _blockViews;

        [Header("Tools")]
        [SerializeField] private MineToolButtonView _pickaxeButton;
        [SerializeField] private MineToolButtonView _bombButton;
        [SerializeField] private MineToolButtonView _dynamiteButton;

        [Header("Functions")]
        [SerializeField] private Button _exitButton;
        [SerializeField] private Button _quickClearButton;

        private IMineService _mineService;
        private ICurrencyService _currencyService;
        private IMessageBrokerService _messageBroker;
        private IUiService _uiService;
        private MineToolType _selectedTool = MineToolType.Pickaxe;
        private int _cachedTimerMinutes = -1;
        private int _cachedTimerSeconds = -1;
        private const int CachedTimerMaxSentinel = -2;
        private const float TimerRefreshInterval = 0.25f;
        private float _timerRefreshAccumulator;

        protected override void OnInitialized()
        {
            _mineService = MainInstaller.Resolve<IMineService>();
            _currencyService = MainInstaller.Resolve<ICurrencyService>();
            _messageBroker = MainInstaller.Resolve<IMessageBrokerService>();
            _uiService = MainInstaller.Resolve<IUiService>();
        }

        protected override void OnOpened()
        {
            SubscribeMessages();
            SubscribeButtons();

            if (_exitButton != null) _exitButton.onClick.AddListener(OnExitClicked);
            if (_quickClearButton != null) _quickClearButton.onClick.AddListener(OnQuickClearClicked);
            if (_infoButton != null) _infoButton.onClick.AddListener(OnInfoClicked);
            if (_progressDetailButton != null) _progressDetailButton.onClick.AddListener(OnProgressDetailClicked);

            RefreshAll();
        }

        protected override void OnClosed()
        {
            UnsubscribeMessages();
            UnsubscribeButtons();

            if (_exitButton != null) _exitButton.onClick.RemoveListener(OnExitClicked);
            if (_quickClearButton != null) _quickClearButton.onClick.RemoveListener(OnQuickClearClicked);
            if (_infoButton != null) _infoButton.onClick.RemoveListener(OnInfoClicked);
            if (_progressDetailButton != null) _progressDetailButton.onClick.RemoveListener(OnProgressDetailClicked);
        }

        private void SubscribeMessages()
        {
            _messageBroker.Subscribe<MineBlocksDestroyedMessage>(OnBlocksDestroyed);
            _messageBroker.Subscribe<MineRewardCollectedMessage>(OnRewardCollected);
            _messageBroker.Subscribe<MineFloorAdvancedMessage>(OnFloorAdvanced);
            _messageBroker.Subscribe<MineChestRevealedMessage>(OnChestRevealed);
            _messageBroker.Subscribe<MineChestRewardClaimedMessage>(OnChestRewardClaimed);
            _messageBroker.Subscribe<MinePickaxeRechargedMessage>(OnPickaxeRecharged);
            _messageBroker.Subscribe<CurrencyChangedMessage>(OnCurrencyChanged);
        }

        private void UnsubscribeMessages()
        {
            _messageBroker?.Unsubscribe<MineBlocksDestroyedMessage>(this);
            _messageBroker?.Unsubscribe<MineRewardCollectedMessage>(this);
            _messageBroker?.Unsubscribe<MineFloorAdvancedMessage>(this);
            _messageBroker?.Unsubscribe<MineChestRevealedMessage>(this);
            _messageBroker?.Unsubscribe<MineChestRewardClaimedMessage>(this);
            _messageBroker?.Unsubscribe<MinePickaxeRechargedMessage>(this);
            _messageBroker?.Unsubscribe<CurrencyChangedMessage>(this);
        }

        private void SubscribeButtons()
        {
            if (_pickaxeButton != null) _pickaxeButton.Clicked += OnToolSelected;
            if (_bombButton != null) _bombButton.Clicked += OnToolSelected;
            if (_dynamiteButton != null) _dynamiteButton.Clicked += OnToolSelected;

            if (_blockViews != null)
            {
                for (int i = 0; i < _blockViews.Length; i++)
                {
                    if (_blockViews[i] != null) _blockViews[i].Clicked += OnBlockClicked;
                }
            }
        }

        private void UnsubscribeButtons()
        {
            if (_pickaxeButton != null) _pickaxeButton.Clicked -= OnToolSelected;
            if (_bombButton != null) _bombButton.Clicked -= OnToolSelected;
            if (_dynamiteButton != null) _dynamiteButton.Clicked -= OnToolSelected;

            if (_blockViews != null)
            {
                for (int i = 0; i < _blockViews.Length; i++)
                {
                    if (_blockViews[i] != null) _blockViews[i].Clicked -= OnBlockClicked;
                }
            }
        }

        private void Update()
        {
            _timerRefreshAccumulator += Time.deltaTime;
            if (_timerRefreshAccumulator >= TimerRefreshInterval)
            {
                _timerRefreshAccumulator -= TimerRefreshInterval;
                RefreshPickaxeTimer();
            }
        }

        private void RefreshAll()
        {
            RefreshFloor();
            RefreshPickaxeCount();
            RefreshToolButtons();
            RefreshBoard();
            RefreshProgress();
        }

        private void RefreshFloor()
        {
            if (_floorText != null)
                _floorText.text = $"{_mineService.Model.CurrentFloor}";
        }

        private void RefreshPickaxeCount()
        {
            if (_pickaxeCountText != null)
                _pickaxeCountText.text = $"{_mineService.CurrentPickaxeCount}/{_mineService.MaxPickaxeCount}";
        }

        private void RefreshPickaxeTimer()
        {
            if (_pickaxeTimerText == null) return;

            if (_mineService.IsPickaxeMaxed)
            {
                if (_cachedTimerMinutes != CachedTimerMaxSentinel)
                {
                    _pickaxeTimerText.text = "MAX";
                    _cachedTimerMinutes = CachedTimerMaxSentinel;
                }
                return;
            }

            var remaining = _mineService.NextRechargeTime;
            int minutes = (int)remaining.TotalMinutes;
            int seconds = remaining.Seconds;

            if (minutes != _cachedTimerMinutes || seconds != _cachedTimerSeconds)
            {
                _cachedTimerMinutes = minutes;
                _cachedTimerSeconds = seconds;
                _pickaxeTimerText.text = $"{minutes:D2}:{seconds:D2}";
            }
        }

        private void RefreshToolButtons()
        {
            int pickaxeCount = _mineService.CurrentPickaxeCount;
            int bombCount = (int)_currencyService.GetAmount(CurrencyType.Bomb).ToDouble();
            int dynamiteCount = (int)_currencyService.GetAmount(CurrencyType.Dynamite).ToDouble();

            _pickaxeButton?.SetData(MineToolType.Pickaxe, pickaxeCount, _selectedTool == MineToolType.Pickaxe);
            _bombButton?.SetData(MineToolType.Bomb, bombCount, _selectedTool == MineToolType.Bomb);
            _dynamiteButton?.SetData(MineToolType.Dynamite, dynamiteCount, _selectedTool == MineToolType.Dynamite);
        }

        private void RefreshBoard()
        {
            if (_blockViews == null || _mineService.Model.Board == null) return;

            for (int i = 0; i < _blockViews.Length && i < _mineService.Model.Board.Count; i++)
            {
                if (_blockViews[i] == null) continue;
                _blockViews[i].SetIndex(i);

                var block = _mineService.Model.Board[i];
                switch (block.State)
                {
                    case MineBlockState.Hidden:
                        _blockViews[i].ShowHidden();
                        break;
                    case MineBlockState.Revealed:
                        _blockViews[i].ShowRevealed(null, block.RewardAmount);
                        break;
                    case MineBlockState.Collected:
                        _blockViews[i].ShowCollected();
                        break;
                    case MineBlockState.Staircase:
                        _blockViews[i].ShowStaircase();
                        break;
                }
            }
        }

        private void RefreshProgress()
        {
            if (_progressSlider == null) return;
            float fraction = _mineService.GetProgressFraction();
            int nextFloor = _mineService.Config.GetNextProgressFloor(_mineService.Model.CurrentFloor);
            _progressSlider.UpdateProgress(fraction, _mineService.Model.CurrentFloor, nextFloor);
        }

        private void OnToolSelected(MineToolType tool)
        {
            _selectedTool = tool;
            RefreshToolButtons();
        }

        /// <summary>
        /// 블록 클릭 시 호출한다 (MineBlockView에서 호출).
        /// </summary>
        public void OnBlockClicked(int blockIndex)
        {
            if (_mineService.Model.Board == null) return;

            var block = _mineService.Model.Board[blockIndex];

            switch (block.State)
            {
                case MineBlockState.Hidden:
                    if (_mineService.CanUseTool(_selectedTool))
                    {
                        _mineService.UseTool(_selectedTool, blockIndex);
                    }
                    break;

                case MineBlockState.Revealed:
                    if (block.RewardType == MineBlockRewardType.TreasureChest)
                    {
                        OpenChestUpgradePopup();
                    }
                    else
                    {
                        _mineService.CollectReward(blockIndex);
                    }
                    break;

                case MineBlockState.Staircase:
                    if (_mineService.CanAdvanceFloor)
                    {
                        _mineService.AdvanceFloor();
                    }
                    break;
            }
        }

        private async void OpenChestUpgradePopup()
        {
            var data = new MineChestUpgradePopupData
            {
                InitialGrade = _mineService.GetChestCurrentGrade()
            };
            await _uiService.OpenUiAsync<MineChestUpgradePopupPresenter, MineChestUpgradePopupData>(data);
        }

        private async void OnQuickClearClicked()
        {
            if (!_mineService.CanQuickClear) return;
            var result = _mineService.ExecuteQuickClear();
            var data = new MineQuickClearResultData { Result = result };
            await _uiService.OpenUiAsync<MineQuickClearResultPopupPresenter, MineQuickClearResultData>(data);
            RefreshAll();
        }

        private void OnExitClicked()
        {
            _uiService.CloseUi<MinePresenter>();
        }

        private async void OnInfoClicked()
        {
            await _uiService.OpenUiAsync<MineInfoPopupPresenter>();
        }

        private async void OnProgressDetailClicked()
        {
            await _uiService.OpenUiAsync<MineProgressRewardPopupPresenter>();
        }

        private void OnBlocksDestroyed(MineBlocksDestroyedMessage msg)
        {
            RefreshBoard();
            RefreshPickaxeCount();
            RefreshToolButtons();
        }

        private void OnRewardCollected(MineRewardCollectedMessage msg)
        {
            RefreshBoard();
        }

        private void OnFloorAdvanced(MineFloorAdvancedMessage msg)
        {
            RefreshAll();
        }

        private void OnChestRevealed(MineChestRevealedMessage msg)
        {
            RefreshBoard();
        }

        private void OnChestRewardClaimed(MineChestRewardClaimedMessage msg)
        {
            RefreshBoard();
        }

        private void OnPickaxeRecharged(MinePickaxeRechargedMessage msg)
        {
            RefreshPickaxeCount();
        }

        private void OnCurrencyChanged(CurrencyChangedMessage msg)
        {
            RefreshToolButtons();
        }
    }
}
