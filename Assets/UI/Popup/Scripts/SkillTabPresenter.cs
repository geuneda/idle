using Geuneda.UiService;
using TMPro;
using UnityEngine;

namespace IdleRPG.UI
{
    /// <summary>
    /// 스킬 탭 컨텐츠 프레젠터. 추후 스킬 장착/관리 기능을 구현한다.
    /// </summary>
    public class SkillTabPresenter : UiPresenter
    {
        [SerializeField] private TMP_Text _titleText;

        protected override void OnOpened()
        {
            if (_titleText != null)
                _titleText.text = "스킬";
        }
    }
}
