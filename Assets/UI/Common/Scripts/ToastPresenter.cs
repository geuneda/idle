using Geuneda.UiService;
using TMPro;
using UnityEngine;

namespace IdleRPG.UI
{
    /// <summary>
    /// 토스트 메시지에 전달하는 데이터.
    /// </summary>
    public struct ToastData
    {
        /// <summary>표시할 메시지</summary>
        public string Message;

        /// <summary>표시 지속 시간 (초). 기본값 2초</summary>
        public float Duration;
    }

    /// <summary>
    /// 범용 토스트 메시지 프레젠터. 화면에 메시지를 표시하고 일정 시간 후 자동으로 닫힌다.
    /// 새 메시지가 도착하면 현재 메시지를 빠르게 페이드 아웃한 뒤 새 메시지를 페이드 인한다.
    /// </summary>
    public class ToastPresenter : UiPresenter<ToastData>
    {
        [SerializeField] private TMP_Text _messageText;
        [SerializeField] private CanvasGroup _canvasGroup;

        private const float FadeInTime = 0.2f;
        private const float FadeOutTime = 0.3f;
        private const float QuickFadeTime = 0.15f;
        private const float DefaultDuration = 2f;

        private float _timer;
        private float _showDuration;
        private Phase _phase;
        private float _fadeStartAlpha;

        private string _nextMessage;
        private float _nextDuration;
        private bool _hasNext;

        private enum Phase
        {
            None,
            FadeIn,
            Show,
            FadeOut,
            QuickFadeOut
        }

        /// <inheritdoc />
        protected override void OnSetData()
        {
            float duration = Data.Duration > 0f ? Data.Duration : DefaultDuration;

            if (_phase == Phase.FadeIn || _phase == Phase.Show || _phase == Phase.FadeOut)
            {
                _nextMessage = Data.Message;
                _nextDuration = duration;
                _hasNext = true;
                _fadeStartAlpha = _canvasGroup != null ? _canvasGroup.alpha : 1f;
                _phase = Phase.QuickFadeOut;
                _timer = 0f;
                return;
            }

            if (_phase == Phase.QuickFadeOut)
            {
                _nextMessage = Data.Message;
                _nextDuration = duration;
                _hasNext = true;
                _fadeStartAlpha = _canvasGroup != null ? _canvasGroup.alpha : 0f;
                _timer = 0f;
                return;
            }

            BeginToast(Data.Message, duration);
        }

        /// <inheritdoc />
        protected override void OnClosed()
        {
            _phase = Phase.None;
            _hasNext = false;
        }

        private void BeginToast(string message, float duration)
        {
            _showDuration = Mathf.Max(0f, duration - FadeInTime - FadeOutTime);
            _timer = 0f;
            _phase = Phase.FadeIn;
            _hasNext = false;

            if (_messageText != null)
                _messageText.text = message;

            SetAlpha(0f);
        }

        private void Update()
        {
            if (!IsOpen || _phase == Phase.None) return;

            _timer += Time.deltaTime;

            switch (_phase)
            {
                case Phase.FadeIn:
                {
                    float t = Mathf.Clamp01(_timer / FadeInTime);
                    SetAlpha(t);
                    if (_timer >= FadeInTime)
                    {
                        SetAlpha(1f);
                        _phase = Phase.Show;
                        _timer = 0f;
                    }
                    break;
                }

                case Phase.Show:
                {
                    if (_timer >= _showDuration)
                    {
                        _phase = Phase.FadeOut;
                        _timer = 0f;
                    }
                    break;
                }

                case Phase.FadeOut:
                {
                    float t = Mathf.Clamp01(_timer / FadeOutTime);
                    SetAlpha(1f - t);
                    if (_timer >= FadeOutTime)
                        Close(false);
                    break;
                }

                case Phase.QuickFadeOut:
                {
                    float t = Mathf.Clamp01(_timer / QuickFadeTime);
                    SetAlpha(Mathf.Lerp(_fadeStartAlpha, 0f, t));
                    if (_timer >= QuickFadeTime)
                    {
                        if (_hasNext)
                            BeginToast(_nextMessage, _nextDuration);
                        else
                            Close(false);
                    }
                    break;
                }
            }
        }

        private void SetAlpha(float alpha)
        {
            if (_canvasGroup != null)
                _canvasGroup.alpha = alpha;
        }
    }
}
