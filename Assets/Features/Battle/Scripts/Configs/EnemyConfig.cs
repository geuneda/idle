using System;
using IdleRPG.Core;

namespace IdleRPG.Battle
{
    /// <summary>
    /// 적 유닛의 기본 스탯 설정 데이터.
    /// <see cref="Geuneda.DataExtensions.ConfigsProvider"/>에 등록하여 ID로 조회한다.
    /// </summary>
    [Serializable]
    public class EnemyConfig
    {
        /// <summary>적 설정 고유 식별자</summary>
        public int Id;

        /// <summary>기본 체력</summary>
        public BigNumber BaseHp;

        /// <summary>기본 공격력</summary>
        public BigNumber BaseAttack;

        /// <summary>기본 방어력</summary>
        public BigNumber BaseDefense;

        /// <summary>보스 여부</summary>
        public bool IsBoss;
    }
}
