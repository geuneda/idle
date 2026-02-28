using System;
using UnityEngine;
using UnityEngine.UI;

namespace IdleRPG.UI
{
    /// <summary>
    /// 하단 탭 바의 개별 탭 버튼. 기본 아이콘과 닫기 아이콘을 전환하며
    /// 클릭 이벤트를 상위 <see cref="BottomTabBarPresenter"/>에 전달한다.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class BottomTabButton : MonoBehaviour
    {
        [SerializeField] private BottomTabType _tabType;
        [SerializeField] private Image _iconImage;
        [SerializeField] private Sprite _defaultIcon;
        [SerializeField] private Sprite _closeIcon;

        private Button _button;

        /// <summary>이 버튼의 탭 종류</summary>
        public BottomTabType TabType => _tabType;

        /// <summary>버튼 클릭 시 발생하는 이벤트. 탭 종류를 전달한다.</summary>
        public event Action<BottomTabType> Clicked;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(OnClick);
        }

        /// <summary>
        /// 탭의 활성/비활성 상태에 따라 아이콘을 전환한다.
        /// </summary>
        /// <param name="isActive">true이면 닫기 아이콘, false이면 기본 아이콘</param>
        public void SetActiveState(bool isActive)
        {
            if (_iconImage != null)
            {
                _iconImage.sprite = isActive ? _closeIcon : _defaultIcon;
            }
        }

        private void OnClick()
        {
            Clicked?.Invoke(_tabType);
        }

        private void OnDestroy()
        {
            if (_button != null)
                _button.onClick.RemoveListener(OnClick);
        }
    }
}
