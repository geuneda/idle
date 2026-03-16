using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace IdleRPG.UI
{
    /// <summary>
    /// 광산 진행도 보상 슬라이더 뷰.
    /// </summary>
    public class MineProgressSliderView : MonoBehaviour
    {
        [SerializeField] private Image _fillImage;
        [SerializeField] private TMP_Text _currentFloorText;
        [SerializeField] private TMP_Text _nextRewardFloorText;

        /// <summary>
        /// 진행도를 업데이트한다.
        /// </summary>
        public void UpdateProgress(float fraction, int currentFloor, int nextRewardFloor)
        {
            if (_fillImage != null)
                _fillImage.fillAmount = Mathf.Clamp01(fraction);

            if (_currentFloorText != null)
                _currentFloorText.text = $"{currentFloor}";

            if (_nextRewardFloorText != null)
                _nextRewardFloorText.text = nextRewardFloor > 0 ? $"{nextRewardFloor}" : "MAX";
        }
    }
}
