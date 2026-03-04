using System;

namespace IdleRPG.Pet
{
    /// <summary>
    /// 개별 펫의 설정 데이터. Google Sheets에서 임포트된다.
    /// </summary>
    [Serializable]
    public class PetEntry
    {
        /// <summary>고유 식별자 (4001~)</summary>
        public int Id;

        /// <summary>펫 등급</summary>
        public PetGrade Grade;

        /// <summary>UI 표시 이름</summary>
        public string DisplayName;

        /// <summary>Addressable 스프라이트 키 (아이콘)</summary>
        public string SpriteKey;

        /// <summary>Addressable RuntimeAnimatorController 키 (펫 본체 Idle/Attack)</summary>
        public string AnimatorKey;

        /// <summary>펫 종족 (예: 드래곤, 요정, 언데드)</summary>
        public string Species;

        /// <summary>레벨 1 보유효과 (공격력 %)</summary>
        public double BasePossessionEffect;

        /// <summary>레벨당 보유효과 증가량</summary>
        public double PossessionEffectPerLevel;

        /// <summary>레벨 1 데미지 퍼센트 (영웅 공격력의 N%)</summary>
        public double BaseDamagePercent;

        /// <summary>레벨당 데미지 퍼센트 증가량</summary>
        public double DamagePercentPerLevel;

        /// <summary>초당 공격 횟수</summary>
        public float AttackSpeed;

        /// <summary>Addressable RuntimeAnimatorController 키 (투사체 애니메이션)</summary>
        public string ProjectileAnimatorKey;

        /// <summary>투사체 이동 속도</summary>
        public float ProjectileSpeed;

        /// <summary>최대 레벨. 0이면 무제한</summary>
        public int MaxLevel;
    }
}
