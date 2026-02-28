using Geuneda.UiService;
using TMPro;
using UnityEngine;

namespace IdleRPG.UI
{
    /// <summary>
    /// 영웅 탭 컨텐츠 프레젠터. 추후 영웅 관리 기능을 구현한다.
    /// </summary>
    public class HeroTabPresenter : UiPresenter
    {
        [SerializeField] private TMP_Text _titleText;

        protected override void OnOpened()
        {
            if (_titleText != null)
                _titleText.text = "영웅";
        }
    }
}
