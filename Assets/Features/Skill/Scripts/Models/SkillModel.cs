using IdleRPG.Core;

namespace IdleRPG.Skill
{
    /// <summary>
    /// 스킬 시스템의 런타임 데이터 모델. <see cref="IndexedCollectibleModel"/>을 상속하여
    /// 6개 장착 슬롯을 인덱스로 관리한다.
    /// </summary>
    public class SkillModel : IndexedCollectibleModel
    {
        /// <summary>최대 스킬 장착 슬롯 수</summary>
        public new const int MAX_SLOTS = 6;

        public SkillModel() : base(MAX_SLOTS) { }

        /// <summary>지정 슬롯에 장착된 스킬 ID를 반환한다.</summary>
        public int GetEquippedSkillId(int slotIndex) => GetEquippedId(slotIndex);
    }
}
