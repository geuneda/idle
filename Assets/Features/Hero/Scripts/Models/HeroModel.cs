using Geuneda.DataExtensions;
using IdleRPG.Core;

namespace IdleRPG.Hero
{
    /// <summary>
    /// 영웅의 런타임 상태를 관리하는 POCO 모델.
    /// <see cref="ObservableField{T}"/>를 사용하여 UI 바인딩을 지원한다.
    /// </summary>
    public class HeroModel
    {
        /// <summary>현재 체력</summary>
        public ObservableField<BigNumber> Hp { get; }

        /// <summary>최대 체력</summary>
        public ObservableField<BigNumber> MaxHp { get; }

        /// <summary>공격력</summary>
        public ObservableField<BigNumber> Attack { get; }

        /// <summary>방어력</summary>
        public ObservableField<BigNumber> Defense { get; }

        /// <summary>초당 체력 재생량</summary>
        public ObservableField<BigNumber> HpRegen { get; }

        /// <summary>공격 속도 (초당 공격 횟수)</summary>
        public ObservableField<float> AttackSpeed { get; }

        /// <summary>치명타 확률 (0~1)</summary>
        public ObservableField<float> CritRate { get; }

        /// <summary>치명타 피해 배율</summary>
        public ObservableField<float> CritDamage { get; }

        /// <summary>투사체 이동 속도</summary>
        public float ProjectileSpeed { get; }

        /// <summary>영웅 레벨</summary>
        public ObservableField<int> Level { get; }

        /// <summary>
        /// <see cref="HeroConfig"/>의 기본값으로 초기화된 영웅 모델을 생성한다.
        /// </summary>
        /// <param name="config">영웅 기본 스탯 설정</param>
        public HeroModel(HeroConfig config)
        {
            Hp = new ObservableField<BigNumber>(config.BaseHp);
            MaxHp = new ObservableField<BigNumber>(config.BaseHp);
            Attack = new ObservableField<BigNumber>(config.BaseAttack);
            Defense = new ObservableField<BigNumber>(config.BaseDefense);
            HpRegen = new ObservableField<BigNumber>(config.BaseHpRegen);
            AttackSpeed = new ObservableField<float>(config.BaseAttackSpeed);
            CritRate = new ObservableField<float>(config.BaseCritRate);
            CritDamage = new ObservableField<float>(config.BaseCritDamage);
            ProjectileSpeed = config.ProjectileSpeed;
            Level = new ObservableField<int>(1);
        }

        /// <summary>영웅이 생존 상태인지 여부</summary>
        public bool IsAlive => Hp.Value > BigNumber.Zero;

        /// <summary>
        /// 피해를 받아 체력을 감소시킨다. 방어력만큼 피해가 경감되며 최소 1의 피해를 받는다.
        /// </summary>
        /// <param name="damage">받은 피해량</param>
        public void TakeDamage(BigNumber damage)
        {
            BigNumber reduced = damage - Defense.Value;
            if (reduced < BigNumber.One) reduced = BigNumber.One;

            BigNumber newHp = Hp.Value - reduced;
            Hp.Value = BigNumber.Max(newHp, BigNumber.Zero);
        }

        /// <summary>
        /// 지정한 양만큼 체력을 회복한다. 최대 체력을 초과하지 않는다.
        /// </summary>
        /// <param name="amount">회복량</param>
        public void Heal(BigNumber amount)
        {
            BigNumber newHp = Hp.Value + amount;
            Hp.Value = BigNumber.Min(newHp, MaxHp.Value);
        }

        /// <summary>
        /// 체력을 최대 체력까지 완전히 회복한다.
        /// </summary>
        public void FullHeal()
        {
            Hp.Value = MaxHp.Value;
        }
    }
}
