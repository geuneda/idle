using System;

namespace IdleRPG.Battle
{
    /// <summary>
    /// 적의 기본 스탯 설정 데이터. <see cref="EnemyModel"/> 초기화에 사용된다.
    /// </summary>
    [Serializable]
    public class EnemyConfig
    {
        /// <summary>적 고유 식별자</summary>
        public int Id;

        /// <summary>기본 체력</summary>
        public double BaseHp = 30;

        /// <summary>기본 공격력</summary>
        public double BaseAttack = 5;

        /// <summary>이동 속도 (초당 유닛)</summary>
        public float MoveSpeed = 2f;

        /// <summary>공격 사거리</summary>
        public float AttackRange = 1f;

        /// <summary>공격 속도 (초당 공격 횟수)</summary>
        public float AttackSpeed = 1f;

        /// <summary>보스 여부</summary>
        public bool IsBoss;
    }
}
