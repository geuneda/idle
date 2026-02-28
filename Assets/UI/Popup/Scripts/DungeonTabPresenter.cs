using Geuneda.UiService;
using TMPro;
using UnityEngine;

namespace IdleRPG.UI
{
    /// <summary>
    /// 던전 탭 컨텐츠 프레젠터. 추후 던전 입장/관리 기능을 구현한다.
    /// </summary>
    public class DungeonTabPresenter : UiPresenter
    {
        [SerializeField] private TMP_Text _titleText;

        protected override void OnOpened()
        {
            if (_titleText != null)
                _titleText.text = "던전";
        }
    }
}
