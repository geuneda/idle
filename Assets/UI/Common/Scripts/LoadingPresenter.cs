using Geuneda.Services;
using Geuneda.UiService;
using IdleRPG.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IdleRPG.UI
{
    /// <summary>
    /// 로딩 화면 프레젠터. 프로그레스 바와 진행률 텍스트를 표시한다.
    /// <see cref="LoadingProgressMessage"/>를 구독하여 진행률을 실시간 갱신한다.
    /// </summary>
    public class LoadingPresenter : UiPresenter
    {
        [Header("진행률")]
        [SerializeField] private Image _progressFill;
        [SerializeField] private TMP_Text _progressText;

        [Header("상태 텍스트")]
        [SerializeField] private TMP_Text _statusText;

        private IMessageBrokerService _messageBroker;

        /// <inheritdoc />
        protected override void OnInitialized()
        {
            _messageBroker = MainInstaller.Resolve<IMessageBrokerService>();
        }

        /// <inheritdoc />
        protected override void OnOpened()
        {
            SetProgress(0f);
            SetStatusText("로딩 중...");
            _messageBroker.Subscribe<LoadingProgressMessage>(OnLoadingProgress);
        }

        /// <inheritdoc />
        protected override void OnClosed()
        {
            _messageBroker?.Unsubscribe<LoadingProgressMessage>(this);
        }

        private void OnLoadingProgress(LoadingProgressMessage message)
        {
            SetProgress(message.Progress);
        }

        private void SetProgress(float progress)
        {
            float clamped = Mathf.Clamp01(progress);

            if (_progressFill != null)
            {
                _progressFill.fillAmount = clamped;
            }

            if (_progressText != null)
            {
                _progressText.text = $"{Mathf.RoundToInt(clamped * 100)}%";
            }
        }

        private void SetStatusText(string text)
        {
            if (_statusText != null)
            {
                _statusText.text = text;
            }
        }
    }
}
