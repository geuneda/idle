using System;
using IdleRPG.Core;

namespace IdleRPG.Skill
{
    /// <summary>
    /// 개별 스킬의 설정 데이터. Google Sheets에서 임포트된다.
    /// <see cref="ICollectibleEntry"/>를 구현하여 공통 수집형 추상화를 지원한다.
    /// </summary>
    [Serializable]
    public class SkillEntry : ICollectibleEntry
    {
        /// <summary>고유 식별자 (3001~3021)</summary>
        public int Id;

        /// <summary>스킬 등급</summary>
        public ItemGrade Grade;

        /// <summary>UI 표시 이름</summary>
        public string DisplayName;

        /// <summary>Addressable 스프라이트 키</summary>
        public string SpriteKey;

        /// <summary>레벨 1 보유효과 (공격력 %)</summary>
        public double BasePossessionEffect;

        /// <summary>레벨당 보유효과 증가량</summary>
        public double PossessionEffectPerLevel;

        /// <summary>스킬 실행 타입</summary>
        public SkillType SkillType;

        /// <summary>쿨타임 (초)</summary>
        public float Cooldown;

        /// <summary>레벨 1 데미지 퍼센트 (공격력의 N%)</summary>
        public double BaseDamagePercent;

        /// <summary>레벨당 데미지 퍼센트 증가량</summary>
        public double DamagePercentPerLevel;

        /// <summary>타격 횟수 (MultiHit 전용)</summary>
        public int HitCount;

        /// <summary>지속시간 (DoT/Buff 전용, 초)</summary>
        public float Duration;

        /// <summary>버프 수치 (Buff 전용)</summary>
        public float BuffValue;

        /// <summary>설명 템플릿. {DamagePercent}, {HitCount}, {Duration} 등의 플레이스홀더 사용</summary>
        public string DescriptionTemplate;

        /// <summary>최대 레벨. 0이면 무제한</summary>
        public int MaxLevel;

        // -- ICollectibleEntry 명시적 구현 --
        int ICollectibleEntry.Id => Id;
        ItemGrade ICollectibleEntry.Grade => Grade;
        string ICollectibleEntry.DisplayName => DisplayName;
        string ICollectibleEntry.SpriteKey => SpriteKey;
        double ICollectibleEntry.BasePossessionEffect => BasePossessionEffect;
        double ICollectibleEntry.PossessionEffectPerLevel => PossessionEffectPerLevel;
        int ICollectibleEntry.MaxLevel => MaxLevel;
    }
}
