using System;
using IdleRPG.Core;

namespace IdleRPG.Battle
{
    [Serializable]
    public class EnemyConfig
    {
        public int Id;
        public BigNumber BaseHp;
        public BigNumber BaseAttack;
        public BigNumber BaseDefense;
        public bool IsBoss;
    }
}
