using System;
using IdleRPG.Equipment;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IdleRPG.UI
{
    /// <summary>
    /// 장비 도감 그리드의 셀 뷰. 장비 아이콘, 등급 배경, 레벨, 잠금 상태를 표시한다.
    /// </summary>
    public class EquipmentCollectionItemView : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private Image _gradeBackground;
        [SerializeField] private TMP_Text _levelText;
        [SerializeField] private GameObject _lockedOverlay;
        [SerializeField] private GameObject _equippedBadge;
        [SerializeField] private GameObject _upgradeAvailableBadge;
        [SerializeField] private Button _button;

        private int _equipmentId;
        private IEquipmentService _equipmentService;

        /// <summary>장비 아이템 클릭 이벤트</summary>
        public event Action<int> Clicked;

        /// <summary>
        /// 이 셀이 표시하는 장비 ID.
        /// </summary>
        public int EquipmentId => _equipmentId;

        private void Awake()
        {
            if (_button != null)
                _button.onClick.AddListener(OnButtonClicked);
        }

        /// <summary>
        /// 셀을 초기화한다. 장비 ID와 서비스를 연결한다.
        /// </summary>
        /// <param name="equipmentId">표시할 장비 ID</param>
        /// <param name="equipmentService">장비 서비스</param>
        public void Setup(int equipmentId, IEquipmentService equipmentService)
        {
            _equipmentId = equipmentId;
            _equipmentService = equipmentService;
            Refresh();
        }

        /// <summary>
        /// 현재 장비 상태를 기반으로 표시를 갱신한다.
        /// </summary>
        public void Refresh()
        {
            if (_equipmentService == null) return;

            var entry = _equipmentService.Config.GetEntry(_equipmentId);
            if (entry == null) return;

            var state = _equipmentService.Model.GetItemState(_equipmentId);
            bool isUnlocked = state != null && state.IsUnlocked;

            if (_lockedOverlay != null)
                _lockedOverlay.SetActive(!isUnlocked);

            if (isUnlocked)
            {
                if (_levelText != null)
                    _levelText.text = $"Lv.{state.Level.Value}";

                bool isEquipped = IsCurrentlyEquipped(entry.Type);
                if (_equippedBadge != null)
                    _equippedBadge.SetActive(isEquipped);

                bool canUpgrade = _equipmentService.CanUpgrade(_equipmentId);
                if (_upgradeAvailableBadge != null)
                    _upgradeAvailableBadge.SetActive(canUpgrade);
            }
            else
            {
                if (_levelText != null)
                    _levelText.text = "";
                if (_equippedBadge != null)
                    _equippedBadge.SetActive(false);
                if (_upgradeAvailableBadge != null)
                    _upgradeAvailableBadge.SetActive(false);
            }
        }

        private bool IsCurrentlyEquipped(EquipmentType type)
        {
            int equippedId = _equipmentService.Model.GetEquippedId(type);
            return equippedId == _equipmentId;
        }

        private void OnButtonClicked()
        {
            Clicked?.Invoke(_equipmentId);
        }

        private void OnDestroy()
        {
            if (_button != null)
                _button.onClick.RemoveAllListeners();
        }
    }
}
