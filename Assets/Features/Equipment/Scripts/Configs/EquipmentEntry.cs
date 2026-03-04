using System;
using IdleRPG.Core;

namespace IdleRPG.Equipment
{
    /// <summary>
    /// 개별 장비의 설정 데이터. Google Sheets에서 1행 = 1개 장비 항목으로 관리된다.
    /// <see cref="ICollectibleEntry"/>를 구현하여 공통 수집형 아이템 계약을 충족한다.
    /// </summary>
    [Serializable]
    public class EquipmentEntry : ICollectibleEntry
    {
        /// <summary>장비 고유 식별자</summary>
        public int Id;

        /// <summary>장비 대분류 (무기/방어구/장신구)</summary>
        public EquipmentType Type;

        /// <summary>세부 종류. <see cref="WeaponKind"/> 또는 <see cref="ArmorKind"/>의 int 값</summary>
        public int SubKind;

        /// <summary>장비 등급</summary>
        public ItemGrade Grade;

        /// <summary>장비 표시 이름</summary>
        public string DisplayName;

        /// <summary>스프라이트 Addressable 키</summary>
        public string SpriteKey;

        /// <summary>레벨 1 보유효과 수치 (%)</summary>
        public double BasePossessionEffect;

        /// <summary>레벨당 보유효과 증가량 (%)</summary>
        public double PossessionEffectPerLevel;

        /// <summary>레벨 1 장착효과 수치 (%)</summary>
        public double BaseEquipEffect;

        /// <summary>레벨당 장착효과 증가량 (%)</summary>
        public double EquipEffectPerLevel;

        /// <summary>최대 레벨. 0이면 무제한 (방치형 게임 기본값: 0)</summary>
        public int MaxLevel = 0;

        /// <inheritdoc />
        int ICollectibleEntry.Id => Id;

        /// <inheritdoc />
        ItemGrade ICollectibleEntry.Grade => Grade;

        /// <inheritdoc />
        string ICollectibleEntry.DisplayName => DisplayName;

        /// <inheritdoc />
        string ICollectibleEntry.SpriteKey => SpriteKey;

        /// <inheritdoc />
        double ICollectibleEntry.BasePossessionEffect => BasePossessionEffect;

        /// <inheritdoc />
        double ICollectibleEntry.PossessionEffectPerLevel => PossessionEffectPerLevel;

        /// <inheritdoc />
        int ICollectibleEntry.MaxLevel => MaxLevel;
    }
}
