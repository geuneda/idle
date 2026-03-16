using Geuneda.Services;
using Geuneda.UiService;
using UnityEngine;
using UnityEngine.UI;

namespace IdleRPG.UI
{
    /// <summary>
    /// 광산 정보(규칙 설명) 팝업 프레젠터.
    /// </summary>
    public class MineInfoPopupPresenter : UiPresenter
    {
        [SerializeField] private Button _closeButton;

        private IUiService _uiService;

        protected override void OnInitialized()
        {
            _uiService = MainInstaller.Resolve<IUiService>();
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
            _uiService.CloseUi<MineInfoPopupPresenter>();
        }
    }
}
