using Geuneda.DataExtensions;
using Geuneda.Services;
using Geuneda.UiService;
using IdleRPG.Core;
using IdleRPG.Economy;
using IdleRPG.Stage;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IdleRPG.UI
{
    /// <summary>
    /// 상단 HUD. 재화(골드/젬), 스테이지/웨이브 진행 상황, 보스 도전 버튼을 표시한다.
    /// </summary>
    public class TopHudPresenter : UiPresenter
    {
        [Header("재화")]
        [SerializeField] private TMP_Text _goldText;
        [SerializeField] private TMP_Text _gemText;

        [Header("스테이지")]
        [SerializeField] private TMP_Text _stageText;

        [Header("웨이브 진행도")]
        [SerializeField] private Image _waveProgressFill;

        [Header("보스 도전")]
        [SerializeField] private Button _bossChallengeButton;

        private ICurrencyService _currencyService;
        private IStageService _stageService;

        protected override void OnInitialized()
        {
            _currencyService = MainInstaller.Resolve<ICurrencyService>();
            _stageService = MainInstaller.Resolve<IStageService>();

            BindCurrency();
            BindStage();
            BindBossChallenge();
        }

        protected override void OnOpened()
        {
            RefreshAll();
        }

        private void BindCurrency()
        {
            _currencyService.Currencies.Observe(CurrencyType.Gold, OnCurrencyChanged);
            _currencyService.Currencies.Observe(CurrencyType.Gem, OnCurrencyChanged);
        }

        private void BindStage()
        {
            var model = _stageService.Model;
            model.CurrentChapter.Observe(OnStageChanged);
            model.CurrentStage.Observe(OnStageChanged);
            model.CurrentWave.Observe(OnWaveChanged);
        }

        private void BindBossChallenge()
        {
            _stageService.Model.IsBossAutoChallenge.Observe(OnBossAutoChallengeChanged);

            if (_bossChallengeButton != null)
            {
                _bossChallengeButton.onClick.AddListener(OnBossChallengeClicked);
            }
        }

        private void RefreshAll()
        {
            RefreshGold();
            RefreshGem();
            RefreshStageText();
            RefreshWaveProgress();
            RefreshBossChallengeButton();
        }

        private void OnCurrencyChanged(CurrencyType key, BigNumber prev, BigNumber curr,
            ObservableUpdateType updateType)
        {
            switch (key)
            {
                case CurrencyType.Gold:
                    RefreshGold();
                    break;
                case CurrencyType.Gem:
                    RefreshGem();
                    break;
            }
        }

        private void OnStageChanged(int prev, int curr)
        {
            RefreshStageText();
            RefreshWaveProgress();
        }

        private void OnWaveChanged(int prev, int curr)
        {
            RefreshWaveProgress();
        }

        private void OnBossAutoChallengeChanged(bool prev, bool curr)
        {
            RefreshBossChallengeButton();
        }

        /// <summary>
        /// 보스 도전 버튼 클릭 시 보스 자동 도전을 재활성화한다.
        /// </summary>
        private void OnBossChallengeClicked()
        {
            _stageService.ToggleBossAutoChallenge(true);
        }

        private void RefreshGold()
        {
            if (_goldText != null)
            {
                var amount = _currencyService.GetAmount(CurrencyType.Gold);
                _goldText.text = amount.ToFormattedString(2);
            }
        }

        private void RefreshGem()
        {
            if (_gemText != null)
            {
                var amount = _currencyService.GetAmount(CurrencyType.Gem);
                _gemText.text = amount.ToFormattedString(2);
            }
        }

        private void RefreshStageText()
        {
            if (_stageText != null)
            {
                _stageText.text = $"보통 {_stageService.GetStageDisplayName()}";
            }
        }

        /// <summary>
        /// 웨이브 진행도를 Fill 이미지와 텍스트로 갱신한다.
        /// </summary>
        private void RefreshWaveProgress()
        {
            int currentWave = _stageService.Model.CurrentWave.Value;
            int totalWaves = _stageService.WavesPerStage;

            if (_waveProgressFill != null)
            {
                _waveProgressFill.fillAmount = (currentWave + 1) / (float)totalWaves;
            }
        }

        /// <summary>
        /// 보스 도전 버튼 상태를 갱신한다.
        /// 보스 자동 도전이 꺼진 상태(보스 실패 후)에서만 버튼이 활성화된다.
        /// </summary>
        private void RefreshBossChallengeButton()
        {
            if (_bossChallengeButton != null)
            {
                bool isAutoOff = !_stageService.Model.IsBossAutoChallenge.Value;
                _bossChallengeButton.gameObject.SetActive(isAutoOff);
            }
        }

        protected override void OnClosed()
        {
            _currencyService?.Currencies.StopObserving(OnCurrencyChanged);

            if (_stageService != null)
            {
                var model = _stageService.Model;
                model.CurrentChapter.StopObserving(OnStageChanged);
                model.CurrentStage.StopObserving(OnStageChanged);
                model.CurrentWave.StopObserving(OnWaveChanged);
                model.IsBossAutoChallenge.StopObserving(OnBossAutoChallengeChanged);
            }

            if (_bossChallengeButton != null)
            {
                _bossChallengeButton.onClick.RemoveListener(OnBossChallengeClicked);
            }
        }
    }
}
