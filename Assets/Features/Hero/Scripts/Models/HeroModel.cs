using Geuneda.DataExtensions;
using IdleRPG.Core;

namespace IdleRPG.Hero
{
    public class HeroModel
    {
        public ObservableField<BigNumber> Hp { get; }
        public ObservableField<BigNumber> MaxHp { get; }
        public ObservableField<BigNumber> Attack { get; }
        public ObservableField<BigNumber> Defense { get; }
        public ObservableField<int> Level { get; }

        public HeroModel(HeroConfig config)
        {
            Hp = new ObservableField<BigNumber>(config.BaseHp);
            MaxHp = new ObservableField<BigNumber>(config.BaseHp);
            Attack = new ObservableField<BigNumber>(config.BaseAttack);
            Defense = new ObservableField<BigNumber>(config.BaseDefense);
            Level = new ObservableField<int>(1);
        }

        public bool IsAlive => Hp.Value > BigNumber.Zero;

        public void TakeDamage(BigNumber damage)
        {
            BigNumber reduced = damage - Defense.Value;
            if (reduced < BigNumber.One) reduced = BigNumber.One;

            BigNumber newHp = Hp.Value - reduced;
            Hp.Value = BigNumber.Max(newHp, BigNumber.Zero);
        }

        public void Heal(BigNumber amount)
        {
            BigNumber newHp = Hp.Value + amount;
            Hp.Value = BigNumber.Min(newHp, MaxHp.Value);
        }

        public void FullHeal()
        {
            Hp.Value = MaxHp.Value;
        }
    }
}
