using Geuneda.Services;
using Geuneda.UiService;
using IdleRPG.Core;
using IdleRPG.Mine;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IdleRPG.UI
{
    /// <summary>
    /// 빠른 클리어 결과 데이터.
    /// </summary>
    public struct MineQuickClearResultData
    {
        /// <summary>결과</summary>
        public MineQuickClearResult Result;
    }

    /// <summary>
    /// 광산 빠른 클리어 결과 팝업 프레젠터.
    /// </summary>
    public class MineQuickClearResultPopupPresenter : UiPresenter<MineQuickClearResultData>
    {
        [SerializeField] private TMP_Text _pickaxesUsedText;
        [SerializeField] private TMP_Text _floorsClearedText;
        [SerializeField] private Transform _rewardListParent;
        [SerializeField] private Button _closeButton;

        private IUiService _uiService;

        protected override void OnInitialized()
        {
            _uiService = MainInstaller.Resolve<IUiService>();
        }

        protected override void OnSetData()
        {
            var result = Data.Result;

            if (_pickaxesUsedText != null)
                _pickaxesUsedText.text = $"{result.PickaxesUsed}";

            if (_floorsClearedText != null)
                _floorsClearedText.text = $"{result.FloorsCleared}";
        }

        protected override void OnOpened()
        {
            if (_closeButton != null) _closeButton.onClick.AddListener(OnCloseClicked);
        }

        protected override void OnClosed()
        {
            if (_closeButton != null) _closeButton.onClick.RemoveListener(OnCloseClicked);
        }

        private void OnCloseClicked()
        {
            _uiService.CloseUi<MineQuickClearResultPopupPresenter>();
        }
    }
}
