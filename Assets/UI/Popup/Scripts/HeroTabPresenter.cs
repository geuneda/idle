using Geuneda.Services;
using Geuneda.UiService;
using IdleRPG.Battle;
using IdleRPG.Equipment;
using IdleRPG.Growth;
using IdleRPG.Hero;
using UnityEngine;

namespace IdleRPG.UI
{
    /// <summary>
    /// 영웅 탭 컨텐츠 프레젠터. 서브 탭 구조로 영웅정보/스킨/숙련도/유물/특성을 관리한다.
    /// </summary>
    public class HeroTabPresenter : UiPresenter
    {
        [Header("Sub Tabs")]
        [SerializeField] private HeroSubTabButton[] _subTabButtons;

        [Header("Content Views")]
        [SerializeField] private HeroInfoView _heroInfoView;

        private IEquipmentService _equipmentService;
        private IHeroGrowthService _growthService;
        private IMessageBrokerService _messageBroker;
        private HeroSubTabType _activeTab;

        protected override void OnInitialized()
        {
            _equipmentService = MainInstaller.Resolve<IEquipmentService>();
            _growthService = MainInstaller.Resolve<IHeroGrowthService>();
            _messageBroker = MainInstaller.Resolve<IMessageBrokerService>();
            var battleService = MainInstaller.Resolve<IBattleService>();

            SetupSubTabButtons();
            SetupHeroInfoView(battleService.HeroModel);
        }

        protected override void OnOpened()
        {
            _heroInfoView?.Subscribe();
            SwitchTab(HeroSubTabType.HeroInfo);
        }

        protected override void OnClosed()
        {
            _heroInfoView?.Unsubscribe();
        }

        private void SetupSubTabButtons()
        {
            if (_subTabButtons == null) return;

            foreach (var button in _subTabButtons)
            {
                if (button != null)
                    button.Clicked += OnSubTabClicked;
            }
        }

        private void SetupHeroInfoView(HeroModel heroModel)
        {
            if (_heroInfoView != null)
            {
                _heroInfoView.Setup(
                    _equipmentService, _growthService, _messageBroker, heroModel);
                _heroInfoView.OnSlotClicked = OnEquipmentSlotClicked;
            }
        }

        private void OnSubTabClicked(HeroSubTabType tabType)
        {
            if (tabType != HeroSubTabType.HeroInfo) return;

            SwitchTab(tabType);
        }

        private void SwitchTab(HeroSubTabType tabType)
        {
            _activeTab = tabType;
            UpdateTabButtonStates();
            UpdateContentVisibility();
        }

        private void UpdateTabButtonStates()
        {
            if (_subTabButtons == null) return;

            foreach (var button in _subTabButtons)
            {
                if (button != null)
                    button.SetActiveState(button.TabType == _activeTab);
            }
        }

        private void UpdateContentVisibility()
        {
            if (_heroInfoView != null)
            {
                _heroInfoView.gameObject.SetActive(true);
                _heroInfoView.Refresh();
            }
        }

        private async void OnEquipmentSlotClicked(EquipmentType slotType)
        {
            await _uiService.OpenUiAsync<EquipmentPopupPresenter, EquipmentPopupData>(
                new EquipmentPopupData { SlotType = slotType });
        }
    }
}
