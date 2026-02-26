using IdleRPG.Core;

namespace IdleRPG.Battle
{
    /// <summary>
    /// 적 유닛의 런타임 상태 모델.
    /// <see cref="EnemyConfig"/>로부터 생성되며, 스테이지 난이도에 따른 배율을 적용한다.
    /// </summary>
    public class EnemyModel
    {
        /// <summary>현재 체력</summary>
        public BigNumber CurrentHp;

        /// <summary>최대 체력</summary>
        public BigNumber MaxHp;

        /// <summary>공격력</summary>
        public BigNumber Attack;

        /// <summary>방어력</summary>
        public BigNumber Defense;

        /// <summary>보스 여부</summary>
        public bool IsBoss;

        /// <summary>원본 <see cref="EnemyConfig"/>의 식별자</summary>
        public int ConfigId;

        /// <summary>
        /// 설정 데이터와 난이도 배율을 기반으로 적 모델을 생성한다.
        /// </summary>
        /// <param name="config">적 기본 스탯 설정</param>
        /// <param name="hpMultiplier">체력 배율 (스테이지 난이도에 따라 결정)</param>
        /// <param name="attackMultiplier">공격력 배율 (스테이지 난이도에 따라 결정)</param>
        public EnemyModel(EnemyConfig config, BigNumber hpMultiplier, BigNumber attackMultiplier)
        {
            ConfigId = config.Id;
            IsBoss = config.IsBoss;
            MaxHp = config.BaseHp * hpMultiplier;
            CurrentHp = MaxHp;
            Attack = config.BaseAttack * attackMultiplier;
            Defense = config.BaseDefense;
        }

        /// <summary>적이 생존 중인지 여부</summary>
        public bool IsAlive => CurrentHp > BigNumber.Zero;

        /// <summary>
        /// 적에게 피해를 입힌다. 방어력만큼 피해를 감소시키며, 최소 1의 피해를 보장한다.
        /// </summary>
        /// <param name="damage">입히려는 피해량</param>
        public void TakeDamage(BigNumber damage)
        {
            BigNumber reduced = damage - Defense;
            if (reduced < BigNumber.One) reduced = BigNumber.One;

            BigNumber newHp = CurrentHp - reduced;
            CurrentHp = BigNumber.Max(newHp, BigNumber.Zero);
        }
    }
}
