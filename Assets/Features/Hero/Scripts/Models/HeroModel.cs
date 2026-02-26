using Geuneda.DataExtensions;
using IdleRPG.Core;

namespace IdleRPG.Hero
{
    /// <summary>
    /// 영웅의 런타임 상태 모델. <see cref="ObservableField{T}"/>로 UI 자동 바인딩을 지원한다.
    /// </summary>
    /// <remarks>
    /// 최소 구현으로 HP/공격력/방어력/레벨만 포함한다.
    /// 추후 장비, 스킬, 버프 시스템이 추가되면 스탯 계산 로직을 확장한다.
    /// </remarks>
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

        /// <summary>레벨</summary>
        public ObservableField<int> Level { get; }

        /// <summary>
        /// 설정 데이터를 기반으로 영웅 모델을 초기화한다.
        /// </summary>
        /// <param name="config">영웅 기본 스탯 설정</param>
        public HeroModel(HeroConfig config)
        {
            Hp = new ObservableField<BigNumber>(config.BaseHp);
            MaxHp = new ObservableField<BigNumber>(config.BaseHp);
            Attack = new ObservableField<BigNumber>(config.BaseAttack);
            Defense = new ObservableField<BigNumber>(config.BaseDefense);
            Level = new ObservableField<int>(1);
        }

        /// <summary>영웅이 생존 중인지 여부</summary>
        public bool IsAlive => Hp.Value > BigNumber.Zero;

        /// <summary>
        /// 영웅에게 피해를 입힌다. 방어력만큼 피해를 감소시키며, 최소 1의 피해를 보장한다.
        /// </summary>
        /// <param name="damage">입히려는 피해량</param>
        public void TakeDamage(BigNumber damage)
        {
            BigNumber reduced = damage - Defense.Value;
            if (reduced < BigNumber.One) reduced = BigNumber.One;

            BigNumber newHp = Hp.Value - reduced;
            Hp.Value = BigNumber.Max(newHp, BigNumber.Zero);
        }

        /// <summary>
        /// 영웅의 체력을 회복한다. 최대 체력을 초과하지 않는다.
        /// </summary>
        /// <param name="amount">회복량</param>
        public void Heal(BigNumber amount)
        {
            BigNumber newHp = Hp.Value + amount;
            Hp.Value = BigNumber.Min(newHp, MaxHp.Value);
        }

        /// <summary>
        /// 체력을 최대치로 완전 회복한다.
        /// </summary>
        public void FullHeal()
        {
            Hp.Value = MaxHp.Value;
        }
    }
}
