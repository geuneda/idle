using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using IdleRPG.Mine;

namespace IdleRPG.UI
{
    /// <summary>
    /// 광산 도구 선택 버튼 뷰.
    /// </summary>
    public class MineToolButtonView : MonoBehaviour
    {
        [SerializeField] private Image _iconImage;
        [SerializeField] private TMP_Text _countText;
        [SerializeField] private GameObject _selectedOverlay;
        [SerializeField] private Button _button;

        private MineToolType _toolType;

        /// <summary>도구 종류</summary>
        public MineToolType ToolType => _toolType;

        /// <summary>버튼 클릭 이벤트</summary>
        public event Action<MineToolType> Clicked;

        /// <summary>
        /// 도구 데이터를 설정한다.
        /// </summary>
        public void SetData(MineToolType toolType, int count, bool isSelected)
        {
            _toolType = toolType;
            if (_countText != null) _countText.text = count.ToString();
            if (_selectedOverlay != null) _selectedOverlay.SetActive(isSelected);
        }

        /// <summary>
        /// 수량을 업데이트한다.
        /// </summary>
        public void UpdateCount(int count)
        {
            if (_countText != null) _countText.text = count.ToString();
        }

        /// <summary>
        /// 선택 상태를 설정한다.
        /// </summary>
        public void SetSelected(bool selected)
        {
            if (_selectedOverlay != null) _selectedOverlay.SetActive(selected);
        }

        private void OnEnable()
        {
            if (_button != null) _button.onClick.AddListener(HandleClick);
        }

        private void OnDisable()
        {
            if (_button != null) _button.onClick.RemoveListener(HandleClick);
        }

        private void HandleClick()
        {
            Clicked?.Invoke(_toolType);
        }
    }
}
