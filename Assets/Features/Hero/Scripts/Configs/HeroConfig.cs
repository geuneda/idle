using System;
using IdleRPG.Core;

namespace IdleRPG.Hero
{
    /// <summary>
    /// 영웅의 기본 스탯 설정 데이터. <see cref="HeroModel"/> 초기화에 사용된다.
    /// </summary>
    [Serializable]
    public class HeroConfig
    {
        /// <summary>기본 체력</summary>
        public BigNumber BaseHp = 100;

        /// <summary>기본 공격력</summary>
        public BigNumber BaseAttack = 10;

        /// <summary>기본 방어력</summary>
        public BigNumber BaseDefense = 2;

        /// <summary>기본 공격 속도 (초당 공격 횟수)</summary>
        public float BaseAttackSpeed = 1f;

        /// <summary>기본 초당 체력 재생량</summary>
        public BigNumber BaseHpRegen = BigNumber.Zero;

        /// <summary>기본 치명타 확률 (0~1)</summary>
        public float BaseCritRate = 0f;

        /// <summary>기본 치명타 피해 배율</summary>
        public float BaseCritDamage = 1.5f;

        /// <summary>투사체 이동 속도</summary>
        public float ProjectileSpeed = 10f;
    }
}
