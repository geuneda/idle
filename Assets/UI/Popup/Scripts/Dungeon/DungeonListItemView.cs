using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IdleRPG.UI
{
    /// <summary>
    /// 던전 목록의 개별 항목 뷰. 이름, 입장 횟수, 입장 버튼을 표시한다.
    /// </summary>
    public class DungeonListItemView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _entryCountText;
        [SerializeField] private Button _entryButton;
        [SerializeField] private GameObject _lockOverlay;

        private Action _onEntryClicked;

        /// <summary>
        /// 항목 데이터를 설정한다.
        /// </summary>
        /// <param name="displayName">표시 이름 (예: "보석던전 Lv.1")</param>
        /// <param name="remaining">남은 입장 횟수</param>
        /// <param name="max">최대 입장 횟수</param>
        /// <param name="isLocked">잠금 여부</param>
        /// <param name="onEntryClicked">입장 버튼 클릭 콜백</param>
        public void SetData(string displayName, int remaining, int max, bool isLocked, Action onEntryClicked)
        {
            _onEntryClicked = onEntryClicked;

            if (_nameText != null)
                _nameText.text = displayName;

            if (_entryCountText != null)
                _entryCountText.text = $"({remaining}/{max})";

            if (_lockOverlay != null)
                _lockOverlay.SetActive(isLocked);

            if (_entryButton != null)
            {
                _entryButton.interactable = !isLocked && remaining > 0;
            }
        }

        private void OnEnable()
        {
            if (_entryButton != null)
                _entryButton.onClick.AddListener(HandleEntryClicked);
        }

        private void OnDisable()
        {
            if (_entryButton != null)
                _entryButton.onClick.RemoveListener(HandleEntryClicked);
        }

        private void HandleEntryClicked()
        {
            _onEntryClicked?.Invoke();
        }
    }
}
