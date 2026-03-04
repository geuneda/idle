namespace IdleRPG.Core
{
    /// <summary>
    /// 수집형 아이템 설정의 공통 계약. 장비, 스킬, 펫 Entry가 구현한다.
    /// </summary>
    public interface ICollectibleEntry
    {
        /// <summary>아이템 고유 식별자</summary>
        int Id { get; }

        /// <summary>아이템 등급</summary>
        ItemGrade Grade { get; }

        /// <summary>표시 이름</summary>
        string DisplayName { get; }

        /// <summary>Addressable 스프라이트 키</summary>
        string SpriteKey { get; }

        /// <summary>기본 보유효과</summary>
        double BasePossessionEffect { get; }

        /// <summary>레벨당 보유효과 증가량</summary>
        double PossessionEffectPerLevel { get; }

        /// <summary>최대 레벨. 0이면 무제한</summary>
        int MaxLevel { get; }
    }
}
