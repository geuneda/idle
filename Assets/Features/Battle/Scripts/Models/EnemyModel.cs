using IdleRPG.Core;

namespace IdleRPG.Battle
{
    public class EnemyModel
    {
        public BigNumber CurrentHp;
        public BigNumber MaxHp;
        public BigNumber Attack;
        public BigNumber Defense;
        public bool IsBoss;
        public int ConfigId;

        public EnemyModel(EnemyConfig config, BigNumber hpMultiplier, BigNumber attackMultiplier)
        {
            ConfigId = config.Id;
            IsBoss = config.IsBoss;
            MaxHp = config.BaseHp * hpMultiplier;
            CurrentHp = MaxHp;
            Attack = config.BaseAttack * attackMultiplier;
            Defense = config.BaseDefense;
        }

        public bool IsAlive => CurrentHp > BigNumber.Zero;

        public void TakeDamage(BigNumber damage)
        {
            BigNumber reduced = damage - Defense;
            if (reduced < BigNumber.One) reduced = BigNumber.One;

            BigNumber newHp = CurrentHp - reduced;
            CurrentHp = BigNumber.Max(newHp, BigNumber.Zero);
        }
    }
}
