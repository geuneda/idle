using System;
using IdleRPG.Core;

namespace IdleRPG.Hero
{
    /// <summary>
    /// 영웅의 기본 스탯 설정 데이터.
    /// 추후 레벨별 성장 계수, 스킬 데이터 등으로 확장 가능하다.
    /// </summary>
    [Serializable]
    public class HeroConfig
    {
        /// <summary>기본 체력</summary>
        public BigNumber BaseHp;

        /// <summary>기본 공격력</summary>
        public BigNumber BaseAttack;

        /// <summary>기본 방어력</summary>
        public BigNumber BaseDefense;
    }
}
