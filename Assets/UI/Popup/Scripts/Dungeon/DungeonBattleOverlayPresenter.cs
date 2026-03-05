using Geuneda.Services;
using Geuneda.UiService;
using IdleRPG.Battle;
using IdleRPG.Core;
using IdleRPG.Dungeon;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IdleRPG.UI
{
    /// <summary>
    /// 던전 전투 오버레이에 전달하는 데이터.
    /// </summary>
    public struct DungeonBattleOverlayData
    {
        /// <summary>던전 종류</summary>
        public DungeonType Type;

        /// <summary>던전 레벨</summary>
        public int Level;

        /// <summary>총 웨이브 수</summary>
        public int TotalWaves;

        /// <summary>제한 시간 (초)</summary>
        public float TimeLimit;
    }

    /// <summary>
    /// 던전 전투 중 표시되는 전체화면 오버레이 프레젠터.
    /// 나가기 버튼, 타이머, 웨이브 진행도를 표시한다.
    /// </summary>
    public class DungeonBattleOverlayPresenter : UiPresenter<DungeonBattleOverlayData>
    {
        [Header("UI Elements")]
        [SerializeField] private TMP_Text _timerText;
        [SerializeField] private TMP_Text _waveText;
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private Button _exitButton;

        private IMessageBrokerService _messageBroker;
        private IDungeonService _dungeonService;
        private IBattleService _battleService;

        /// <inheritdoc />
        protected override void OnInitialized()
        {
            _messageBroker = MainInstaller.Resolve<IMessageBrokerService>();
            _dungeonService = MainInstaller.Resolve<IDungeonService>();
            _battleService = MainInstaller.Resolve<IBattleService>();

            if (_exitButton != null) _exitButton.onClick.AddListener(OnExitClicked);
        }

        /// <inheritdoc />
        protected override void OnSetData()
        {
            if (_titleText != null)
                _titleText.text = $"{GetDungeonName(Data.Type)} Lv.{Data.Level}";

            UpdateTimer(Data.TimeLimit);
            UpdateWave(0, Data.TotalWaves);
        }

        /// <inheritdoc />
        protected override void OnOpened()
        {
            _messageBroker.Subscribe<DungeonTimerTickMessage>(OnTimerTick);
            _messageBroker.Subscribe<DungeonWaveStartedMessage>(OnWaveStarted);
            _messageBroker.Subscribe<DungeonCompletedMessage>(OnDungeonCompleted);
            _messageBroker.Subscribe<DungeonFailedMessage>(OnDungeonFailed);
        }

        /// <inheritdoc />
        protected override void OnClosed()
        {
            _messageBroker?.UnsubscribeAll(this);
        }

        private void OnTimerTick(DungeonTimerTickMessage msg)
        {
            UpdateTimer(msg.RemainingTime);
        }

        private void OnWaveStarted(DungeonWaveStartedMessage msg)
        {
            UpdateWave(msg.WaveIndex, msg.TotalWaves);
        }

        private void OnDungeonCompleted(DungeonCompletedMessage msg)
        {
            var rewardName = GetCurrencyName(msg.RewardType);
            ShowToastAndExit($"{rewardName} x{msg.RewardAmount.ToFormattedString()} 획득!");
        }

        private void OnDungeonFailed(DungeonFailedMessage msg)
        {
            string reason = msg.Reason == DungeonFailReason.TimeExpired ? "시간 초과" : "전투 실패";
            ShowToastAndExit($"던전 실패 - {reason}");
        }

        private void OnExitClicked()
        {
            ExitDungeon();
        }

        private async void ShowToastAndExit(string message)
        {
            ExitDungeon();
            var toastData = new ToastData { Message = message };
            await _uiService.OpenUiAsync<ToastPresenter, ToastData>(toastData);
        }

        private void ExitDungeon()
        {
            _battleService.Model.IsBattleActive.Value = false;

            Close(false);

            _dungeonService.ExitDungeon();
            _battleService.StartBattle();
        }

        private void UpdateTimer(float remainingSeconds)
        {
            if (_timerText == null) return;

            int totalSeconds = Mathf.CeilToInt(Mathf.Max(0f, remainingSeconds));
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;
            _timerText.text = $"{minutes:D2}:{seconds:D2}";
        }

        private void UpdateWave(int waveIndex, int totalWaves)
        {
            if (_waveText != null)
                _waveText.text = $"Wave {waveIndex + 1}/{totalWaves}";
        }

        private static string GetDungeonName(DungeonType type) => DungeonDisplayHelper.GetDungeonName(type);

        private static string GetCurrencyName(IdleRPG.Economy.CurrencyType type) => DungeonDisplayHelper.GetCurrencyName(type);
    }
}
