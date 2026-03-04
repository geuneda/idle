using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace IdleRPG.UI
{
    /// <summary>
    /// 펫 장착 슬롯 뷰. 잠금, 빈 슬롯, 장착됨 세 가지 상태를 표시한다.
    /// </summary>
    public class PetSlotView : MonoBehaviour
    {
        [SerializeField] private Image _iconImage;
        [SerializeField] private GameObject _lockedOverlay;
        [SerializeField] private TMP_Text _lockConditionText;
        [SerializeField] private GameObject _emptyState;
        [SerializeField] private GameObject _equippedState;
        [SerializeField] private Button _unequipButton;
        [SerializeField] private Button _button;

        /// <summary>
        /// 슬롯을 잠금 상태로 설정한다. 잠금 오버레이와 해금 조건 텍스트를 표시한다.
        /// </summary>
        /// <param name="conditionText">해금 조건 설명 텍스트</param>
        public void SetLocked(string conditionText)
        {
            SetActiveStates(locked: true, empty: false, equipped: false);

            if (_lockConditionText != null)
                _lockConditionText.text = conditionText;
        }

        /// <summary>
        /// 슬롯을 빈 슬롯 상태로 설정한다.
        /// </summary>
        public void SetEmpty()
        {
            SetActiveStates(locked: false, empty: true, equipped: false);
        }

        /// <summary>
        /// 슬롯에 펫이 장착된 상태로 설정한다. 아이콘을 표시한다.
        /// </summary>
        /// <param name="icon">펫 아이콘 스프라이트</param>
        public void SetEquipped(Sprite icon)
        {
            SetActiveStates(locked: false, empty: false, equipped: true);

            if (_iconImage != null)
            {
                _iconImage.sprite = icon;
                _iconImage.enabled = icon != null;
            }
        }

        /// <summary>
        /// 비동기 로드 완료 후 아이콘 스프라이트를 업데이트한다.
        /// </summary>
        /// <param name="icon">로드된 스프라이트</param>
        public void UpdateIcon(Sprite icon)
        {
            if (_iconImage != null)
            {
                _iconImage.sprite = icon;
                _iconImage.enabled = icon != null;
            }
        }

        /// <summary>
        /// 슬롯 클릭 리스너를 설정한다. 기존 리스너는 제거된다.
        /// </summary>
        /// <param name="action">클릭 시 실행할 콜백</param>
        public void SetOnClickListener(UnityAction action)
        {
            if (_button == null) return;

            _button.onClick.RemoveAllListeners();
            if (action != null)
                _button.onClick.AddListener(action);
        }

        /// <summary>
        /// 장착해제 버튼 클릭 리스너를 설정한다. 기존 리스너는 제거된다.
        /// </summary>
        /// <param name="action">클릭 시 실행할 콜백. null이면 버튼을 숨긴다.</param>
        public void SetOnUnequipClickListener(UnityAction action)
        {
            if (_unequipButton == null) return;

            _unequipButton.onClick.RemoveAllListeners();
            _unequipButton.gameObject.SetActive(action != null);
            if (action != null)
                _unequipButton.onClick.AddListener(action);
        }

        private void SetActiveStates(bool locked, bool empty, bool equipped)
        {
            if (_lockedOverlay != null)
                _lockedOverlay.SetActive(locked);
            if (_emptyState != null)
                _emptyState.SetActive(empty);
            if (_equippedState != null)
                _equippedState.SetActive(equipped);

            if (!equipped && _unequipButton != null)
                _unequipButton.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (_button != null)
                _button.onClick.RemoveAllListeners();
            if (_unequipButton != null)
                _unequipButton.onClick.RemoveAllListeners();
        }
    }
}
