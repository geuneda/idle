using IdleRPG.Equipment;
using IdleRPG.Growth;
using IdleRPG.Hero;
using Geuneda.Services;
using TMPro;
using UnityEngine;

namespace IdleRPG.UI
{
    /// <summary>
    /// 영웅정보 서브 탭 뷰. 장비 슬롯 3개와 전체 스탯을 표시한다.
    /// </summary>
    public class HeroInfoView : MonoBehaviour
    {
        [Header("Equipment Slots")]
        [SerializeField] private EquipmentSlotView _weaponSlot;
        [SerializeField] private EquipmentSlotView _armorSlot;
        [SerializeField] private EquipmentSlotView _accessorySlot;

        [Header("Combat Power")]
        [SerializeField] private TMP_Text _combatPowerText;

        [Header("Stat Rows")]
        [SerializeField] private HeroStatRowView _attackRow;
        [SerializeField] private HeroStatRowView _hpRow;
        [SerializeField] private HeroStatRowView _hpRegenRow;
        [SerializeField] private HeroStatRowView _attackSpeedRow;
        [SerializeField] private HeroStatRowView _critRateRow;
        [SerializeField] private HeroStatRowView _critDamageRow;
        [SerializeField] private HeroStatRowView _doubleShotRow;
        [SerializeField] private HeroStatRowView _tripleShotRow;
        [SerializeField] private HeroStatRowView _advancedAttackRow;
        [SerializeField] private HeroStatRowView _enemyBonusDamageRow;

        private IEquipmentService _equipmentService;
        private IHeroGrowthService _growthService;
        private IMessageBrokerService _messageBroker;
        private HeroModel _heroModel;
        private bool _isSetup;

        /// <summary>
        /// 장비 슬롯 클릭 시 발생하는 콜백. 부모 Presenter가 설정한다.
        /// </summary>
        public System.Action<EquipmentType> OnSlotClicked { get; set; }

        /// <summary>
        /// 부모 Presenter로부터 서비스를 주입받아 초기화한다. 1회만 호출해야 한다.
        /// </summary>
        /// <param name="equipmentService">장비 서비스</param>
        /// <param name="growthService">성장 서비스</param>
        /// <param name="messageBroker">메시지 브로커</param>
        /// <param name="heroModel">영웅 모델</param>
        public void Setup(
            IEquipmentService equipmentService,
            IHeroGrowthService growthService,
            IMessageBrokerService messageBroker,
            HeroModel heroModel)
        {
            if (_isSetup) return;
            _isSetup = true;

            _equipmentService = equipmentService;
            _growthService = growthService;
            _messageBroker = messageBroker;
            _heroModel = heroModel;

            SetupEquipmentSlots();
        }

        /// <summary>
        /// 메시지 구독을 시작한다. 부모 Presenter의 OnOpened에서 호출한다.
        /// </summary>
        public void Subscribe()
        {
            if (_messageBroker == null) return;

            _messageBroker.Subscribe<CombatPowerChangedMessage>(OnCombatPowerChanged);
            _messageBroker.Subscribe<EquipmentEffectsChangedMessage>(OnEquipmentEffectsChanged);
            _messageBroker.Subscribe<StatLevelUpMessage>(OnStatLevelUp);
        }

        /// <summary>
        /// 메시지 구독을 해제한다. 부모 Presenter의 OnClosed에서 호출한다.
        /// </summary>
        public void Unsubscribe()
        {
            _messageBroker?.Unsubscribe<CombatPowerChangedMessage>(this);
            _messageBroker?.Unsubscribe<EquipmentEffectsChangedMessage>(this);
            _messageBroker?.Unsubscribe<StatLevelUpMessage>(this);
        }

        /// <summary>
        /// 전체 표시를 갱신한다.
        /// </summary>
        public void Refresh()
        {
            RefreshEquipmentSlots();
            RefreshCombatPower();
            RefreshStats();
        }

        private void SetupEquipmentSlots()
        {
            if (_weaponSlot != null)
            {
                _weaponSlot.SetLocked(false);
                _weaponSlot.Clicked += HandleSlotClicked;
            }

            if (_armorSlot != null)
            {
                _armorSlot.SetLocked(false);
                _armorSlot.Clicked += HandleSlotClicked;
            }

            if (_accessorySlot != null)
            {
                _accessorySlot.SetLocked(true);
                _accessorySlot.Clicked += HandleSlotClicked;
            }
        }

        private void HandleSlotClicked(EquipmentType slotType)
        {
            OnSlotClicked?.Invoke(slotType);
        }

        private void RefreshEquipmentSlots()
        {
            if (_equipmentService == null) return;

            _weaponSlot?.Refresh(_equipmentService);
            _armorSlot?.Refresh(_equipmentService);
            _accessorySlot?.Refresh(_equipmentService);
        }

        private void RefreshCombatPower()
        {
            if (_combatPowerText != null && _growthService != null)
                _combatPowerText.text = _growthService.CombatPower.ToFormattedString(2);
        }

        private void RefreshStats()
        {
            if (_heroModel == null) return;

            _attackRow?.UpdateValue(_heroModel.Attack.Value.ToFormattedString(2));
            _hpRow?.UpdateValue(_heroModel.MaxHp.Value.ToFormattedString(2));
            _hpRegenRow?.UpdateValue(_heroModel.HpRegen.Value.ToFormattedString(2));
            _attackSpeedRow?.UpdateValue($"{_heroModel.AttackSpeed.Value:F1}");
            _critRateRow?.UpdateValue(FormatPercent(_heroModel.CritRate.Value));
            _critDamageRow?.UpdateValue(FormatPercent(_heroModel.CritDamage.Value));
            _doubleShotRow?.UpdateValue(FormatPercent(_heroModel.DoubleShotRate.Value));
            _tripleShotRow?.UpdateValue(FormatPercent(_heroModel.TripleShotRate.Value));
            _advancedAttackRow?.UpdateValue(FormatPercent(_heroModel.AdvancedAttackBonus.Value));
            _enemyBonusDamageRow?.UpdateValue(FormatPercent(_heroModel.EnemyBonusDamage.Value));
        }

        private static string FormatPercent(float value)
        {
            return $"{value * 100f:F1}%";
        }

        private void OnCombatPowerChanged(CombatPowerChangedMessage msg)
        {
            RefreshCombatPower();
        }

        private void OnEquipmentEffectsChanged(EquipmentEffectsChangedMessage msg)
        {
            RefreshEquipmentSlots();
            RefreshStats();
        }

        private void OnStatLevelUp(StatLevelUpMessage msg)
        {
            RefreshStats();
            RefreshCombatPower();
        }

        private void OnDestroy()
        {
            if (_weaponSlot != null)
                _weaponSlot.Clicked -= HandleSlotClicked;
            if (_armorSlot != null)
                _armorSlot.Clicked -= HandleSlotClicked;
            if (_accessorySlot != null)
                _accessorySlot.Clicked -= HandleSlotClicked;

            Unsubscribe();
        }
    }
}
