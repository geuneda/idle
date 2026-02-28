using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Geuneda.Services;
using Geuneda.UiService;
using IdleRPG.Core;
using UnityEngine;

namespace IdleRPG.UI
{
    /// <summary>
    /// 하단 탭 바 프레젠터. 6개 탭 버튼을 관리하며 탭 전환 로직을 담당한다.
    /// 탭 선택 시 해당 탭의 컨텐츠 프레젠터를 열고, 재선택 시 닫는다.
    /// 다른 탭 선택 시 기존 탭을 닫고 새 탭을 연다.
    /// </summary>
    public class BottomTabBarPresenter : UiPresenter
    {
        [SerializeField] private BottomTabButton[] _tabButtons;

        private BottomTabType? _activeTab;
        private IUiService _uiService;
        private IMessageBrokerService _messageBroker;

        private static readonly Dictionary<BottomTabType, Type> TabPresenterTypes = new()
        {
            { BottomTabType.Hero, typeof(HeroTabPresenter) },
            { BottomTabType.Skill, typeof(SkillTabPresenter) },
            { BottomTabType.Pet, typeof(PetTabPresenter) },
            { BottomTabType.Dungeon, typeof(DungeonTabPresenter) },
            { BottomTabType.Base, typeof(BaseTabPresenter) },
            { BottomTabType.Summon, typeof(SummonTabPresenter) }
        };

        protected override void OnInitialized()
        {
            _uiService = MainInstaller.Resolve<IUiService>();
            _messageBroker = MainInstaller.Resolve<IMessageBrokerService>();

            foreach (var button in _tabButtons)
            {
                button.Clicked += OnTabClicked;
            }
        }

        protected override void OnOpened()
        {
            _activeTab = null;
            ResetAllButtons();
        }

        private void OnTabClicked(BottomTabType tabType)
        {
            if (_activeTab.HasValue && _activeTab.Value == tabType)
            {
                CloseActiveTabAsync().Forget();
            }
            else if (_activeTab.HasValue)
            {
                SwitchTabAsync(tabType).Forget();
            }
            else
            {
                OpenTabAsync(tabType).Forget();
            }
        }

        private async UniTaskVoid OpenTabAsync(BottomTabType tabType)
        {
            _uiService.CloseUi<MainBattleUiPresenter>();

            if (TabPresenterTypes.TryGetValue(tabType, out var presenterType))
            {
                await _uiService.OpenUiAsync(presenterType);
            }

            _activeTab = tabType;
            UpdateButtonStates();

            _messageBroker.Publish(new TabOpenedMessage { TabIndex = (int)tabType });
        }

        private async UniTaskVoid CloseActiveTabAsync()
        {
            if (!_activeTab.HasValue) return;

            if (TabPresenterTypes.TryGetValue(_activeTab.Value, out var presenterType))
            {
                _uiService.CloseUi(presenterType);
            }

            _activeTab = null;
            UpdateButtonStates();

            await _uiService.OpenUiAsync<MainBattleUiPresenter>();

            _messageBroker.Publish(new TabClosedMessage());
        }

        private async UniTaskVoid SwitchTabAsync(BottomTabType newTab)
        {
            if (_activeTab.HasValue && TabPresenterTypes.TryGetValue(_activeTab.Value, out var oldType))
            {
                _uiService.CloseUi(oldType);
            }

            if (TabPresenterTypes.TryGetValue(newTab, out var newType))
            {
                await _uiService.OpenUiAsync(newType);
            }

            _activeTab = newTab;
            UpdateButtonStates();

            _messageBroker.Publish(new TabOpenedMessage { TabIndex = (int)newTab });
        }

        private void UpdateButtonStates()
        {
            foreach (var button in _tabButtons)
            {
                bool isActive = _activeTab.HasValue && button.TabType == _activeTab.Value;
                button.SetActiveState(isActive);
            }
        }

        private void ResetAllButtons()
        {
            foreach (var button in _tabButtons)
            {
                button.SetActiveState(false);
            }
        }

        protected override void OnClosed()
        {
            foreach (var button in _tabButtons)
            {
                button.Clicked -= OnTabClicked;
            }
        }
    }
}
