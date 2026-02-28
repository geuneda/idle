using System;
using System.Collections.Generic;
using Geuneda.DataExtensions;
using IdleRPG.Hero;

namespace IdleRPG.Growth
{
    /// <summary>
    /// 영웅의 스탯별 성장 레벨을 관리하는 POCO 모델.
    /// <see cref="ObservableDictionary{TKey,TValue}"/>를 사용하여 UI 바인딩을 지원한다.
    /// </summary>
    [Serializable]
    public class HeroGrowthModel
    {
        private readonly ObservableDictionary<HeroStatType, int> _statLevels;

        /// <summary>스탯별 레벨의 읽기 전용 뷰. UI에서 옵저빙하여 표시를 갱신할 수 있다.</summary>
        public IObservableDictionaryReader<HeroStatType, int> StatLevels => _statLevels;

        /// <summary>
        /// 모든 성장 가능 스탯을 레벨 0으로 초기화한다.
        /// </summary>
        public HeroGrowthModel()
        {
            var initial = new Dictionary<HeroStatType, int>
            {
                { HeroStatType.Attack, 0 },
                { HeroStatType.Hp, 0 },
                { HeroStatType.HpRegen, 0 },
                { HeroStatType.AttackSpeed, 0 },
                { HeroStatType.CritRate, 0 },
                { HeroStatType.CritDamage, 0 },
                { HeroStatType.DoubleShot, 0 },
                { HeroStatType.TripleShot, 0 },
                { HeroStatType.AdvancedAttack, 0 },
                { HeroStatType.EnemyBonusDamage, 0 }
            };
            _statLevels = new ObservableDictionary<HeroStatType, int>(initial);
        }

        /// <summary>
        /// 지정한 스탯의 현재 레벨을 반환한다.
        /// </summary>
        /// <param name="statType">조회할 스탯 유형</param>
        /// <returns>현재 레벨. 등록되지 않은 스탯이면 0</returns>
        public int GetLevel(HeroStatType statType)
        {
            return _statLevels.TryGetValue(statType, out var level) ? level : 0;
        }

        /// <summary>
        /// 지정한 스탯의 레벨을 1 증가시킨다.
        /// </summary>
        /// <param name="statType">레벨업할 스탯 유형</param>
        /// <returns>증가 후 레벨</returns>
        public int IncrementLevel(HeroStatType statType)
        {
            int current = GetLevel(statType);
            int next = current + 1;
            _statLevels[statType] = next;
            return next;
        }

        /// <summary>
        /// 지정한 스탯의 레벨을 설정한다. 저장 데이터 복원 시 사용한다.
        /// </summary>
        /// <param name="statType">대상 스탯 유형</param>
        /// <param name="level">설정할 레벨</param>
        public void SetLevel(HeroStatType statType, int level)
        {
            _statLevels[statType] = level;
        }
    }
}
