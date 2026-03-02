using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace IdleRPG.UI
{
    /// <summary>
    /// 전투 화면 스킬 슬롯 뷰. 잠금/빈 슬롯/장착 상태를 표시하고,
    /// 쿨다운(Radial360)과 사용중(Vertical) Fill 이미지로 스킬 상태를 시각화한다.
    /// </summary>
    public class BattleSkillSlotView : MonoBehaviour
    {
        [SerializeField] private Image _iconImage;
        [SerializeField] private Image _cooldownFill;
        [SerializeField] private Image _activeFill;
        [SerializeField] private GameObject _lockedOverlay;
        [SerializeField] private GameObject _emptyState;
        [SerializeField] private GameObject _equippedState;
        [SerializeField] private Button _button;

        private bool _isCooldownFillVisible;
        private bool _isActiveFillVisible;

        /// <summary>
        /// 슬롯을 잠금 상태로 설정한다.
        /// </summary>
        public void SetLocked()
        {
            SetActiveStates(locked: true, empty: false, equipped: false);
            ResetFills();
        }

        /// <summary>
        /// 슬롯을 빈 슬롯(해금됨, 미장착) 상태로 설정한다.
        /// </summary>
        public void SetEmpty()
        {
            SetActiveStates(locked: false, empty: true, equipped: false);
            ResetFills();
        }

        /// <summary>
        /// 슬롯에 스킬이 장착된 상태로 설정한다.
        /// </summary>
        /// <param name="icon">스킬 아이콘 스프라이트</param>
        public void SetEquipped(Sprite icon)
        {
            SetActiveStates(locked: false, empty: false, equipped: true);
            ResetFills();

            if (_iconImage != null)
            {
                _iconImage.sprite = icon;
                _iconImage.enabled = icon != null;
            }
        }

        /// <summary>
        /// 쿨다운 Fill을 갱신한다. Radial360 방식으로 12시부터 시계방향으로 사라진다.
        /// </summary>
        /// <param name="remaining">남은 쿨타임 (초)</param>
        /// <param name="total">전체 쿨타임 (초)</param>
        public void UpdateCooldown(float remaining, float total)
        {
            UpdateFill(_cooldownFill, remaining, total, ref _isCooldownFillVisible);
        }

        /// <summary>
        /// 사용중 Fill을 갱신한다. Vertical 방식으로 위에서 아래로 사라진다.
        /// </summary>
        /// <param name="remaining">남은 사용 시간 (초)</param>
        /// <param name="total">전체 사용 시간 (초)</param>
        public void UpdateActive(float remaining, float total)
        {
            UpdateFill(_activeFill, remaining, total, ref _isActiveFillVisible);
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

        private static void UpdateFill(Image fill, float remaining, float total, ref bool isVisible)
        {
            if (fill == null) return;

            if (total <= 0f || remaining <= 0f)
            {
                if (isVisible)
                {
                    fill.fillAmount = 0f;
                    fill.enabled = false;
                    isVisible = false;
                }
                return;
            }

            if (!isVisible)
            {
                fill.enabled = true;
                isVisible = true;
            }
            fill.fillAmount = remaining / total;
        }

        private void SetActiveStates(bool locked, bool empty, bool equipped)
        {
            if (_lockedOverlay != null)
                _lockedOverlay.SetActive(locked);
            if (_emptyState != null)
                _emptyState.SetActive(empty);
            if (_equippedState != null)
                _equippedState.SetActive(equipped);
        }

        private void ResetFills()
        {
            if (_cooldownFill != null)
            {
                _cooldownFill.fillAmount = 0f;
                _cooldownFill.enabled = false;
            }

            if (_activeFill != null)
            {
                _activeFill.fillAmount = 0f;
                _activeFill.enabled = false;
            }

            _isCooldownFillVisible = false;
            _isActiveFillVisible = false;
        }

        private void OnDestroy()
        {
            if (_button != null)
                _button.onClick.RemoveAllListeners();
        }
    }
}
