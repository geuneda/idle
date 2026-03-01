using Geuneda.UiService;
using IdleRPG.OfflineReward;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IdleRPG.UI
{
    /// <summary>
    /// 오프라인 보상 팝업 프레젠터. 수령 전까지 닫을 수 없다.
    /// </summary>
    public class OfflineRewardPopupPresenter : UiPresenter<OfflineRewardPopupData>
    {
        private const string TitleText = "오프라인 보상";
        private const string ElapsedTimeFormat = "{0}분";
        private const string ClaimButtonText = "받기";
        private const string AdDoubleClaimButtonText = "광고 2배";

        [Header("타이틀")]
        [SerializeField] private TMP_Text _titleText;

        [Header("경과 시간")]
        [SerializeField] private TMP_Text _elapsedTimeText;

        [Header("골드 보상")]
        [SerializeField] private TMP_Text _goldAmountText;

        [Header("아이템 드롭")]
        [SerializeField] private Transform _dropItemContainer;

        [Header("버튼")]
        [SerializeField] private Button _claimButton;
        [SerializeField] private TMP_Text _claimButtonText;
        [SerializeField] private Button _adDoubleClaimButton;
        [SerializeField] private TMP_Text _adDoubleClaimButtonText;

        /// <summary>
        /// 초기화 시 버튼 이벤트를 등록하고 고정 텍스트를 설정한다.
        /// </summary>
        protected override void OnInitialized()
        {
            _claimButton.onClick.AddListener(OnClaimClicked);
            _adDoubleClaimButton.onClick.AddListener(OnAdDoubleClaimClicked);

            ApplyStaticTexts();
        }

        /// <summary>
        /// 데이터가 설정되면 화면을 갱신한다.
        /// </summary>
        protected override void OnSetData()
        {
            RefreshDisplay();
        }

        /// <summary>
        /// 팝업이 열리면 화면을 갱신한다.
        /// </summary>
        protected override void OnOpened()
        {
            RefreshDisplay();
        }

        /// <summary>
        /// 팝업이 닫히면 버튼 이벤트를 해제한다.
        /// </summary>
        protected override void OnClosed()
        {
            _claimButton.onClick.RemoveListener(OnClaimClicked);
            _adDoubleClaimButton.onClick.RemoveListener(OnAdDoubleClaimClicked);
        }

        private void RefreshDisplay()
        {
            var result = Data.Result;
            if (result == null) return;

            if (_elapsedTimeText != null)
            {
                _elapsedTimeText.text = string.Format(ElapsedTimeFormat, (int)result.ClampedMinutes);
            }

            if (_goldAmountText != null)
            {
                _goldAmountText.text = result.Gold.ToFormattedString(2);
            }

            RefreshDropItems(result);
        }

        /// <summary>
        /// 데이터에 의존하지 않는 고정 텍스트를 일괄 적용한다.
        /// </summary>
        private void ApplyStaticTexts()
        {
            if (_titleText != null) _titleText.text = TitleText;
            if (_claimButtonText != null) _claimButtonText.text = ClaimButtonText;
            if (_adDoubleClaimButtonText != null) _adDoubleClaimButtonText.text = AdDoubleClaimButtonText;
        }

        private void RefreshDropItems(OfflineRewardResult result)
        {
            if (_dropItemContainer == null) return;

            // 기존 아이템 제거
            for (int i = _dropItemContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(_dropItemContainer.GetChild(i).gameObject);
            }

            // TODO: 펫/장비/스킬 시스템 구현 후 아이템 슬롯 프리팹으로 교체 필요
            // 현재는 임시 텍스트로 드롭 결과만 표시 (테스트용)
            foreach (var drop in result.Drops)
            {
                var textObj = new GameObject($"Drop_{drop.DropType}_{drop.Grade}");
                textObj.transform.SetParent(_dropItemContainer, false);
                var text = textObj.AddComponent<TextMeshProUGUI>();
                text.text = $"{drop.DropType} ({drop.Grade}) x{drop.Count}";
                text.fontSize = 24;
                text.alignment = TextAlignmentOptions.Center;
            }
        }

        private void OnClaimClicked()
        {
            Data.OnClaimed?.Invoke(false);
            Close(true);
        }

        private void OnAdDoubleClaimClicked()
        {
            // 추후 광고 SDK 연동. 현재는 바로 2배 적용.
            Data.OnClaimed?.Invoke(true);
            Close(true);
        }
    }
}
