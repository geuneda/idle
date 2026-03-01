using System;
using IdleRPG.Equipment;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IdleRPG.UI
{
    /// <summary>
    /// 장비 슬롯 뷰. 장착된 장비의 아이콘과 등급 배경을 표시한다.
    /// </summary>
    public class EquipmentSlotView : MonoBehaviour
    {
        [SerializeField] private EquipmentType _slotType;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Image _equipmentIcon;
        [SerializeField] private GameObject _emptyState;
        [SerializeField] private GameObject _equippedState;
        [SerializeField] private TMP_Text _levelText;
        [SerializeField] private GameObject _lockOverlay;
        [SerializeField] private Button _slotButton;

        /// <summary>이 슬롯의 장비 타입</summary>
        public EquipmentType SlotType => _slotType;

        /// <summary>슬롯 클릭 이벤트</summary>
        public event Action<EquipmentType> Clicked;

        private void Awake()
        {
            if (_slotButton != null)
                _slotButton.onClick.AddListener(() => Clicked?.Invoke(_slotType));
        }

        /// <summary>
        /// 슬롯 잠금 상태를 설정한다.
        /// </summary>
        /// <param name="locked">잠금 여부</param>
        public void SetLocked(bool locked)
        {
            if (_lockOverlay != null)
                _lockOverlay.SetActive(locked);
            if (_slotButton != null)
                _slotButton.interactable = !locked;
        }

        /// <summary>
        /// 장비 서비스를 기반으로 슬롯 표시를 갱신한다.
        /// </summary>
        /// <param name="service">장비 서비스</param>
        public void Refresh(IEquipmentService service)
        {
            int equippedId = service.Model.GetEquippedId(_slotType);
            bool hasEquipped = equippedId != EquipmentModel.UNEQUIPPED;

            if (_emptyState != null)
                _emptyState.SetActive(!hasEquipped);
            if (_equippedState != null)
                _equippedState.SetActive(hasEquipped);

            if (hasEquipped)
            {
                var entry = service.Config.GetEntry(equippedId);
                var state = service.Model.GetItemState(equippedId);
                if (entry != null && state != null)
                {
                    if (_levelText != null)
                        _levelText.text = $"Lv.{state.Level.Value}";
                }
            }
        }

        private void OnDestroy()
        {
            if (_slotButton != null)
                _slotButton.onClick.RemoveAllListeners();
        }
    }
}
