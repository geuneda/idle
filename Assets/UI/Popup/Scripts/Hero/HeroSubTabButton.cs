using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace IdleRPG.UI
{
    /// <summary>
    /// 영웅 탭 내 서브 탭 버튼. 탭 전환 이벤트를 발생시킨다.
    /// </summary>
    public class HeroSubTabButton : MonoBehaviour
    {
        [SerializeField] private HeroSubTabType _tabType;
        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _labelText;
        [SerializeField] private GameObject _activeIndicator;

        /// <summary>이 버튼이 나타내는 서브 탭 타입</summary>
        public HeroSubTabType TabType => _tabType;

        /// <summary>버튼 클릭 이벤트</summary>
        public event Action<HeroSubTabType> Clicked;

        private void Awake()
        {
            if (_button != null)
                _button.onClick.AddListener(() => Clicked?.Invoke(_tabType));
        }

        /// <summary>
        /// 활성 상태를 설정한다.
        /// </summary>
        /// <param name="isActive">활성 여부</param>
        public void SetActiveState(bool isActive)
        {
            if (_activeIndicator != null)
                _activeIndicator.SetActive(isActive);
        }

        private void OnDestroy()
        {
            if (_button != null)
                _button.onClick.RemoveAllListeners();
        }
    }
}
