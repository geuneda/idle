using Cysharp.Threading.Tasks;
using Geuneda.UiService;
using UnityEngine;

namespace IdleRPG.UI
{
    /// <summary>
    /// <see cref="CanvasGroup"/>의 알파를 조절하여 페이드 인/아웃 전환 효과를 제공하는 피처.
    /// <see cref="ITransitionFeature"/>를 구현하여 UiService 생명주기와 연동된다.
    /// </summary>
    public class LoadingFadeFeature : PresenterFeatureBase, ITransitionFeature
    {
        [SerializeField] private CanvasGroup _canvasGroup;

        [Header("페이드 설정")]
        [Tooltip("페이드 인 지속 시간 (초)")]
        [SerializeField, Min(0.01f)] private float _fadeInDuration = 0.3f;

        [Tooltip("페이드 아웃 지속 시간 (초)")]
        [SerializeField, Min(0.01f)] private float _fadeOutDuration = 0.5f;

        private UniTaskCompletionSource _openCompletion;
        private UniTaskCompletionSource _closeCompletion;

        /// <inheritdoc />
        public UniTask OpenTransitionTask =>
            _openCompletion?.Task ?? UniTask.CompletedTask;

        /// <inheritdoc />
        public UniTask CloseTransitionTask =>
            _closeCompletion?.Task ?? UniTask.CompletedTask;

        /// <inheritdoc />
        public override void OnPresenterOpened()
        {
            _openCompletion?.TrySetResult();
            _openCompletion = new UniTaskCompletionSource();
            FadeInAsync().Forget();
        }

        /// <inheritdoc />
        public override void OnPresenterClosing()
        {
            _closeCompletion?.TrySetResult();
            _closeCompletion = new UniTaskCompletionSource();
            FadeOutAsync().Forget();
        }

        private async UniTask FadeInAsync()
        {
            if (_canvasGroup == null)
            {
                _openCompletion?.TrySetResult();
                return;
            }

            _canvasGroup.alpha = 0f;
            float elapsed = 0f;

            while (elapsed < _fadeInDuration)
            {
                elapsed += Time.deltaTime;
                _canvasGroup.alpha = Mathf.Clamp01(elapsed / _fadeInDuration);
                await UniTask.Yield();
            }

            _canvasGroup.alpha = 1f;
            _openCompletion?.TrySetResult();
        }

        private async UniTask FadeOutAsync()
        {
            if (_canvasGroup == null)
            {
                _closeCompletion?.TrySetResult();
                return;
            }

            float elapsed = 0f;

            while (elapsed < _fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                _canvasGroup.alpha = 1f - Mathf.Clamp01(elapsed / _fadeOutDuration);
                await UniTask.Yield();
            }

            _canvasGroup.alpha = 0f;
            _closeCompletion?.TrySetResult();
        }
    }
}
