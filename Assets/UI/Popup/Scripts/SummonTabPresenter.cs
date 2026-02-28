using Geuneda.UiService;
using TMPro;
using UnityEngine;

namespace IdleRPG.UI
{
    /// <summary>
    /// 소환 탭 컨텐츠 프레젠터. 추후 가챠/소환 기능을 구현한다.
    /// </summary>
    public class SummonTabPresenter : UiPresenter
    {
        [SerializeField] private TMP_Text _titleText;

        protected override void OnOpened()
        {
            if (_titleText != null)
                _titleText.text = "소환";
        }
    }
}
