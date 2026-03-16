using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace IdleRPG.UI
{
    /// <summary>
    /// 광산 보드판의 개별 블록 뷰.
    /// </summary>
    public class MineBlockView : MonoBehaviour
    {
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Image _rockOverlay;
        [SerializeField] private Image _rewardIcon;
        [SerializeField] private TMP_Text _rewardAmountText;
        [SerializeField] private Image _staircaseIcon;
        [SerializeField] private GameObject _previewHighlight;
        [SerializeField] private Button _button;

        private int _blockIndex;

        /// <summary>블록 인덱스</summary>
        public int BlockIndex => _blockIndex;

        /// <summary>블록 클릭 이벤트</summary>
        public event Action<int> Clicked;

        /// <summary>
        /// 블록 인덱스를 설정한다.
        /// </summary>
        public void SetIndex(int index)
        {
            _blockIndex = index;
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
            Clicked?.Invoke(_blockIndex);
        }

        /// <summary>
        /// Hidden 상태로 표시한다 (바위 오버레이).
        /// </summary>
        public void ShowHidden()
        {
            SetOverlayActive(true, false, false);
            if (_rewardAmountText != null) _rewardAmountText.text = "";
        }

        /// <summary>
        /// Revealed 상태로 표시한다 (보상 아이콘).
        /// </summary>
        public void ShowRevealed(Sprite rewardSprite, int amount)
        {
            SetOverlayActive(false, true, false);
            if (_rewardIcon != null)
            {
                _rewardIcon.sprite = rewardSprite;
                _rewardIcon.gameObject.SetActive(rewardSprite != null);
            }
            if (_rewardAmountText != null)
                _rewardAmountText.text = amount > 1 ? amount.ToString() : "";
        }

        /// <summary>
        /// Collected 상태로 표시한다 (빈칸).
        /// </summary>
        public void ShowCollected()
        {
            SetOverlayActive(false, false, false);
            if (_rewardAmountText != null) _rewardAmountText.text = "";
        }

        /// <summary>
        /// Staircase 상태로 표시한다 (계단 아이콘).
        /// </summary>
        public void ShowStaircase()
        {
            SetOverlayActive(false, false, true);
            if (_rewardAmountText != null) _rewardAmountText.text = "";
        }

        /// <summary>
        /// 프리뷰 하이라이트를 설정한다.
        /// </summary>
        public void SetPreviewHighlight(bool active)
        {
            if (_previewHighlight != null)
                _previewHighlight.SetActive(active);
        }

        private void SetOverlayActive(bool rock, bool reward, bool staircase)
        {
            if (_rockOverlay != null) _rockOverlay.gameObject.SetActive(rock);
            if (_rewardIcon != null) _rewardIcon.gameObject.SetActive(reward);
            if (_staircaseIcon != null) _staircaseIcon.gameObject.SetActive(staircase);
        }
    }
}
