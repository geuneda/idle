using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IdleRPG.UI
{
    /// <summary>
    /// 영웅 정보 탭의 스탯 행 뷰. 아이콘과 수치를 표시한다.
    /// </summary>
    public class HeroStatRowView : MonoBehaviour
    {
        [SerializeField] private Image _iconImage;
        [SerializeField] private TMP_Text _valueText;

        /// <summary>
        /// 수치 텍스트를 갱신한다.
        /// </summary>
        /// <param name="formattedValue">포맷된 수치 문자열</param>
        public void UpdateValue(string formattedValue)
        {
            if (_valueText != null)
                _valueText.text = formattedValue;
        }
    }
}
