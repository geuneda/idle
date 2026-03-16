using Geuneda.Services;
using Geuneda.UiService;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IdleRPG.UI
{
    /// <summary>
    /// 기지 탭 컨텐츠 프레젠터. 서브 컨텐츠(광산 등) 진입점을 제공한다.
    /// </summary>
    public class BaseTabPresenter : UiPresenter
    {
        [SerializeField] private TMP_Text _titleText;

        [Header("광산")]
        [SerializeField] private Button _mineEnterButton;

        protected override void OnOpened()
        {
            if (_titleText != null)
                _titleText.text = "기지";

            if (_mineEnterButton != null)
                _mineEnterButton.onClick.AddListener(OnMineEnterClicked);
        }

        protected override void OnClosed()
        {
            if (_mineEnterButton != null)
                _mineEnterButton.onClick.RemoveListener(OnMineEnterClicked);
        }

        private async void OnMineEnterClicked()
        {
            var uiService = MainInstaller.Resolve<IUiService>();
            await uiService.OpenUiAsync<MinePresenter>();
        }
    }
}
