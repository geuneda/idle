using System;
using System.Threading;
using Geuneda.DataExtensions;
using Geuneda.Services;
using Geuneda.UiService;
using IdleRPG.Core;
using IdleRPG.Economy;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IdleRPG.UI
{
    /// <summary>
    /// 재화 상세 팝업 프레젠터. 재화의 아이콘, 이름, 설명, 보유량을 표시한다.
    /// </summary>
    public class CurrencyDetailPopupPresenter : UiPresenter<CurrencyDetailPopupData>
    {
        [Header("Currency Info")]
        [SerializeField] private Image _iconImage;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _descriptionText;
        [SerializeField] private TMP_Text _ownedAmountText;

        [Header("Buttons")]
        [SerializeField] private Button _closeButton;

        private ICurrencyService _currencyService;
        private IConfigsProvider _configsProvider;
        private SpriteCache _spriteCache;
        private CancellationTokenSource _cts;

        /// <inheritdoc />
        protected override void OnInitialized()
        {
            _currencyService = MainInstaller.Resolve<ICurrencyService>();
            _configsProvider = MainInstaller.Resolve<IConfigsProvider>();

            if (_closeButton != null)
                _closeButton.onClick.AddListener(OnCloseClicked);
        }

        /// <inheritdoc />
        protected override void OnSetData()
        {
            _spriteCache?.Dispose();
            _spriteCache = new SpriteCache();
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();

            RefreshDisplay();
        }

        /// <inheritdoc />
        protected override void OnOpened()
        {
            _currencyService.Currencies.Observe(Data.CurrencyType, OnCurrencyChanged);
        }

        /// <inheritdoc />
        protected override void OnClosed()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;

            _currencyService?.Currencies.StopObserving(OnCurrencyChanged);

            _spriteCache?.Dispose();
            _spriteCache = null;
        }

        private void RefreshDisplay()
        {
            var collection = _configsProvider.GetConfig<CurrencyDisplayConfigCollection>();
            var displayConfig = collection?.GetConfig(Data.CurrencyType);

            if (displayConfig != null)
            {
                if (_nameText != null)
                    _nameText.text = displayConfig.DisplayName;

                if (_descriptionText != null)
                    _descriptionText.text = displayConfig.Description;

                if (_backgroundImage != null
                    && ColorUtility.TryParseHtmlString(displayConfig.BackgroundColorHex, out var color))
                {
                    _backgroundImage.color = color;
                }

                if (!string.IsNullOrEmpty(displayConfig.IconSpriteKey))
                {
                    LoadIconAsync(displayConfig.IconSpriteKey);
                }
            }

            RefreshAmount();
        }

        private void RefreshAmount()
        {
            if (_ownedAmountText != null)
            {
                var amount = _currencyService.GetAmount(Data.CurrencyType);
                _ownedAmountText.text = amount.ToFormattedString();
            }
        }

        private async void LoadIconAsync(string spriteKey)
        {
            try
            {
                var token = _cts?.Token ?? destroyCancellationToken;
                var sprite = await _spriteCache.LoadAsync(spriteKey, token);
                if (_iconImage != null && sprite != null)
                    _iconImage.sprite = sprite;
            }
            catch (OperationCanceledException) { }
        }

        private void OnCurrencyChanged(
            CurrencyType key, BigNumber prev, BigNumber curr, ObservableUpdateType updateType)
        {
            if (key == Data.CurrencyType)
            {
                RefreshAmount();
            }
        }

        private void OnCloseClicked()
        {
            Close(true);
        }
    }
}
