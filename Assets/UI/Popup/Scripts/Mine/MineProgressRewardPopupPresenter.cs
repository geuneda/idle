using Geuneda.Services;
using Geuneda.UiService;
using IdleRPG.Mine;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IdleRPG.UI
{
    /// <summary>
    /// 광산 진행도 보상 팝업 프레젠터.
    /// </summary>
    public class MineProgressRewardPopupPresenter : UiPresenter
    {
        [SerializeField] private Transform _contentParent;
        [SerializeField] private Button _closeButton;

        private IMineService _mineService;
        private IUiService _uiService;

        protected override void OnInitialized()
        {
            _mineService = MainInstaller.Resolve<IMineService>();
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
            _uiService.CloseUi<MineProgressRewardPopupPresenter>();
        }
    }
}
