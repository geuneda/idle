using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace IdleRPG.UI
{
    /// <summary>
    /// 스킬 도감 그리드의 셀 뷰. 장착중, 잠금(미보유), 보통 세 가지 상태를 표시한다.
    /// </summary>
    public class SkillCollectionItemView : MonoBehaviour
    {
        [SerializeField] private Image _iconImage;
        [SerializeField] private Image _gradeFrame;
        [SerializeField] private TMP_Text _levelText;
        [SerializeField] private TMP_Text _ownedText;
        [SerializeField] private GameObject _equippedOverlay;
        [SerializeField] private TMP_Text _equippedLabel;
        [SerializeField] private GameObject _lockedOverlay;
        [SerializeField] private GameObject _normalState;
        [SerializeField] private Button _button;
        [SerializeField] private GameObject _upgradeBadge;

        private int _skillId;

        /// <summary>스킬 아이템 클릭 이벤트</summary>
        public event Action<int> Clicked;

        /// <summary>
        /// 이 셀이 표시하는 스킬 ID.
        /// </summary>
        public int SkillId => _skillId;

        private void Awake()
        {
            if (_button != null)
                _button.onClick.AddListener(OnButtonClicked);
        }

        /// <summary>
        /// 셀의 스킬 ID를 설정한다.
        /// </summary>
        /// <param name="skillId">표시할 스킬 ID</param>
        public void SetSkillId(int skillId)
        {
            _skillId = skillId;
        }

        /// <summary>
        /// 장착중 상태로 설정한다. 아이콘, 등급 테두리, "장착중" 오버레이를 표시한다.
        /// </summary>
        /// <param name="icon">스킬 아이콘 스프라이트</param>
        /// <param name="gradeColor">등급 테두리 색상</param>
        /// <param name="level">스킬 레벨</param>
        public void SetEquipped(Sprite icon, Color gradeColor, int level)
        {
            SetIcon(icon, gradeColor, full: true);
            SetOverlays(equipped: true, locked: false, normal: false);

            if (_levelText != null)
                _levelText.text = $"Lv.{level}";
            if (_ownedText != null)
                _ownedText.text = "";
            if (_upgradeBadge != null)
                _upgradeBadge.SetActive(false);
        }

        /// <summary>
        /// 잠금(미보유) 상태로 설정한다. 아이콘은 어둡게 표시하고 잠금 오버레이를 활성화한다.
        /// </summary>
        /// <param name="icon">스킬 아이콘 스프라이트</param>
        /// <param name="gradeColor">등급 테두리 색상</param>
        public void SetLocked(Sprite icon, Color gradeColor)
        {
            SetIcon(icon, gradeColor, full: false);
            SetOverlays(equipped: false, locked: true, normal: false);

            if (_levelText != null)
                _levelText.text = "";
            if (_ownedText != null)
                _ownedText.text = "";
            if (_upgradeBadge != null)
                _upgradeBadge.SetActive(false);
        }

        /// <summary>
        /// 보통(보유) 상태로 설정한다. 아이콘, 등급 테두리, 레벨, 보유 현황을 표시한다.
        /// </summary>
        /// <param name="icon">스킬 아이콘 스프라이트</param>
        /// <param name="gradeColor">등급 테두리 색상</param>
        /// <param name="level">스킬 레벨</param>
        /// <param name="ownedCount">현재 보유 수</param>
        /// <param name="requiredCount">강화 필요 수</param>
        /// <param name="canUpgrade">강화 가능 여부</param>
        public void SetNormal(Sprite icon, Color gradeColor, int level, int ownedCount,
            int requiredCount, bool canUpgrade)
        {
            SetIcon(icon, gradeColor, full: true);
            SetOverlays(equipped: false, locked: false, normal: true);

            if (_levelText != null)
                _levelText.text = $"Lv.{level}";
            if (_ownedText != null)
                _ownedText.text = $"{ownedCount}/{requiredCount}";
            if (_upgradeBadge != null)
                _upgradeBadge.SetActive(canUpgrade);
        }

        /// <summary>
        /// 아이템 클릭 리스너를 설정한다. 기존 외부 리스너는 제거되며 내부 이벤트 리스너는 유지된다.
        /// </summary>
        /// <param name="action">클릭 시 실행할 콜백</param>
        public void SetOnClickListener(UnityAction action)
        {
            if (_button == null) return;

            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(OnButtonClicked);
            if (action != null)
                _button.onClick.AddListener(action);
        }

        private void SetIcon(Sprite icon, Color gradeColor, bool full)
        {
            if (_iconImage != null)
            {
                _iconImage.sprite = icon;
                _iconImage.enabled = icon != null;
                _iconImage.color = full ? Color.white : new Color(0.3f, 0.3f, 0.3f, 1f);
            }

            if (_gradeFrame != null)
                _gradeFrame.color = gradeColor;
        }

        private void SetOverlays(bool equipped, bool locked, bool normal)
        {
            if (_equippedOverlay != null)
                _equippedOverlay.SetActive(equipped);
            if (_lockedOverlay != null)
                _lockedOverlay.SetActive(locked);
            if (_normalState != null)
                _normalState.SetActive(normal);
        }

        private void OnButtonClicked()
        {
            Clicked?.Invoke(_skillId);
        }

        private void OnDestroy()
        {
            if (_button != null)
                _button.onClick.RemoveAllListeners();
        }
    }
}
