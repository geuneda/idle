using System.Collections;
using DG.Tweening;
using Geuneda.Services;
using Geuneda.UiService;
using IdleRPG.Mine;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IdleRPG.UI
{
    /// <summary>
    /// 보물상자 강화 팝업 프레젠터.
    /// 터치 한 번으로 강화를 시도하고, 강화 완료 후 터치하면 보상을 수령한다.
    /// </summary>
    public class MineChestUpgradePopupPresenter : UiPresenter<MineChestUpgradePopupData>
    {
        [SerializeField] private TMP_Text _gradeText;
        [SerializeField] private Image _chestIcon;
        [SerializeField] private TMP_Text _actionText;
        [SerializeField] private TMP_Text _resultText;
        [SerializeField] private Button _touchButton;

        private static readonly WaitForSeconds ResultDelay = new(0.8f);
        private static readonly WaitForSeconds ClaimDelay = new(1.2f);

        private IMineService _mineService;
        private bool _isAnimating;

        protected override void OnInitialized()
        {
            _mineService = MainInstaller.Resolve<IMineService>();
        }

        protected override void OnSetData()
        {
            if (_resultText != null)
                _resultText.gameObject.SetActive(false);
            RefreshDisplay();
        }

        protected override void OnOpened()
        {
            if (_touchButton != null) _touchButton.onClick.AddListener(OnTouchClicked);
        }

        protected override void OnClosed()
        {
            _isAnimating = false;
            if (_chestIcon != null) _chestIcon.transform.DOKill();
            if (_touchButton != null) _touchButton.onClick.RemoveListener(OnTouchClicked);
        }

        private void OnTouchClicked()
        {
            if (_isAnimating) return;

            if (_mineService.IsChestCompleted)
            {
                if (_mineService.Model.ChestRewardClaimed) return;
                StartCoroutine(PlayClaimSequence());
                return;
            }

            StartCoroutine(PlayUpgradeSequence());
        }

        private IEnumerator PlayUpgradeSequence()
        {
            _isAnimating = true;
            _touchButton.interactable = false;

            var shakeTween = _chestIcon.transform
                .DOShakeRotation(0.5f, new Vector3(0f, 0f, 15f), 10, 90f)
                .SetEase(Ease.OutQuad);
            yield return shakeTween.WaitForCompletion();

            bool success = _mineService.TryUpgradeChest();

            if (_resultText != null)
            {
                _resultText.gameObject.SetActive(true);
                _resultText.text = success ? "성공!" : "실패...";
                _resultText.color = success ? Color.green : Color.red;
            }

            if (success && _chestIcon != null)
            {
                _chestIcon.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 5);
            }

            yield return ResultDelay;

            if (_resultText != null)
                _resultText.gameObject.SetActive(false);

            RefreshDisplay();
            _touchButton.interactable = true;
            _isAnimating = false;
        }

        private IEnumerator PlayClaimSequence()
        {
            _isAnimating = true;
            _touchButton.interactable = false;

            var shakeTween = _chestIcon.transform
                .DOShakeRotation(0.7f, new Vector3(0f, 0f, 20f), 15, 90f)
                .SetEase(Ease.OutQuad);
            yield return shakeTween.WaitForCompletion();

            if (_chestIcon != null)
            {
                var punchTween = _chestIcon.transform.DOPunchScale(Vector3.one * 0.3f, 0.4f, 5);
                yield return punchTween.WaitForCompletion();
            }

            var grade = _mineService.GetChestCurrentGrade();
            _mineService.ClaimChestReward();

            if (_resultText != null)
            {
                _resultText.gameObject.SetActive(true);
                _resultText.text = $"{ItemGradeHelper.GetGradeText(grade)} 보상 획득!";
                _resultText.color = Color.yellow;
            }

            yield return ClaimDelay;

            _isAnimating = false;
            _uiService.CloseUi<MineChestUpgradePopupPresenter>();
        }

        private void RefreshDisplay()
        {
            var grade = _mineService.GetChestCurrentGrade();
            if (_gradeText != null)
                _gradeText.text = ItemGradeHelper.GetGradeText(grade);

            if (_actionText != null)
                _actionText.text = _mineService.IsChestCompleted ? "탭하여 열기" : "터치하여 강화시도";
        }
    }
}
