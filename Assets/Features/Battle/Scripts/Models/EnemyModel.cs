using IdleRPG.Core;

namespace IdleRPG.Battle
{
    /// <summary>
    /// 적의 런타임 상태를 관리하는 POCO 모델.
    /// <see cref="EnemyConfig"/>와 스테이지 배율을 기반으로 초기화된다.
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

        /// <summary>이동 속도 (초당 유닛)</summary>
        public float MoveSpeed;

        /// <summary>공격 사거리</summary>
        public float AttackRange;

        /// <summary>공격 속도 (초당 공격 횟수)</summary>
        public float AttackSpeed;

        /// <summary>보스 여부</summary>
        public bool IsBoss;

        /// <summary>설정 데이터의 고유 식별자</summary>
        public int ConfigId;

        /// <summary>
        /// <see cref="EnemyConfig"/>와 스테이지 배율을 적용하여 적 모델을 생성한다.
        /// </summary>
        /// <param name="config">적 기본 스탯 설정</param>
        /// <param name="hpMultiplier">스테이지별 체력 배율</param>
        /// <param name="attackMultiplier">스테이지별 공격력 배율</param>
        public EnemyModel(EnemyConfig config, BigNumber hpMultiplier, BigNumber attackMultiplier)
        {
            ConfigId = config.Id;
            IsBoss = config.IsBoss;
            MaxHp = config.BaseHp * hpMultiplier;
            CurrentHp = MaxHp;
            Attack = config.BaseAttack * attackMultiplier;
            Defense = config.BaseDefense;
            MoveSpeed = config.MoveSpeed;
            AttackRange = config.AttackRange;
            AttackSpeed = config.AttackSpeed;
        }

        /// <summary>적이 생존 상태인지 여부</summary>
        public bool IsAlive => CurrentHp > BigNumber.Zero;

        /// <summary>
        /// 피해를 받아 체력을 감소시킨다. 방어력만큼 피해가 경감되며 최소 1의 피해를 받는다.
        /// </summary>
        /// <param name="damage">받은 피해량</param>
        public void TakeDamage(BigNumber damage)
        {
            BigNumber reduced = damage - Defense;
            if (reduced < BigNumber.One) reduced = BigNumber.One;

            BigNumber newHp = CurrentHp - reduced;
            CurrentHp = BigNumber.Max(newHp, BigNumber.Zero);
        }
    }
}
