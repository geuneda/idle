using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using IdleRPG.Core;
using IdleRPG.Economy;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IdleRPG.UI
{
    /// <summary>
    /// 재사용 가능한 재화 슬롯 뷰. 아이콘, 배경색, 수량을 표시하며 클릭 이벤트를 제공한다.
    /// </summary>
    public class CurrencySlotView : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private Image _background;
        [SerializeField] private TMP_Text _amountText;
        [SerializeField] private Button _button;

        private CurrencyType _currencyType;

        /// <summary>재화 슬롯 클릭 이벤트</summary>
        public event Action<CurrencyType> Clicked;

        /// <summary>
        /// 이 슬롯이 표시하는 재화 타입.
        /// </summary>
        public CurrencyType CurrencyType => _currencyType;

        private void Awake()
        {
            if (_button != null)
                _button.onClick.AddListener(OnButtonClicked);
        }

        /// <summary>
        /// 슬롯을 초기화한다. 재화 타입, 수량, 표시 설정을 적용한다.
        /// </summary>
        /// <param name="type">재화 종류</param>
        /// <param name="amount">표시할 수량</param>
        /// <param name="displayConfig">재화 표시 설정</param>
        /// <param name="spriteCache">스프라이트 캐시</param>
        /// <param name="token">취소 토큰</param>
        public void Setup(
            CurrencyType type,
            BigNumber amount,
            CurrencyDisplayConfig displayConfig,
            SpriteCache spriteCache,
            CancellationToken token = default)
        {
            _currencyType = type;

            if (displayConfig != null)
            {
                if (_background != null
                    && ColorUtility.TryParseHtmlString(displayConfig.BackgroundColorHex, out var color))
                {
                    _background.color = color;
                }

                if (spriteCache != null && !string.IsNullOrEmpty(displayConfig.IconSpriteKey))
                {
                    LoadIconAsync(displayConfig.IconSpriteKey, spriteCache, token).Forget();
                }
            }

            UpdateAmount(amount);
        }

        /// <summary>
        /// 수량 텍스트를 갱신한다.
        /// </summary>
        /// <param name="amount">갱신할 수량</param>
        public void UpdateAmount(BigNumber amount)
        {
            if (_amountText != null)
                _amountText.text = amount.ToFormattedString();
        }

        private async UniTaskVoid LoadIconAsync(
            string spriteKey, SpriteCache spriteCache, CancellationToken token)
        {
            try
            {
                var sprite = await spriteCache.LoadAsync(spriteKey, token);
                if (_icon != null && sprite != null)
                    _icon.sprite = sprite;
            }
            catch (OperationCanceledException) { }
        }

        private void OnButtonClicked()
        {
            Clicked?.Invoke(_currencyType);
        }

        private void OnDestroy()
        {
            if (_button != null)
                _button.onClick.RemoveListener(OnButtonClicked);

            Clicked = null;
        }
    }
}
