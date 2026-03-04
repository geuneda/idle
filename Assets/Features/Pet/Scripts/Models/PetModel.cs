using IdleRPG.Core;

namespace IdleRPG.Pet
{
    /// <summary>
    /// 펫 시스템의 런타임 데이터 모델. <see cref="IndexedCollectibleModel"/>을 상속하여
    /// 5개 장착 슬롯을 인덱스로 관리한다.
    /// </summary>
    public class PetModel : IndexedCollectibleModel
    {
        /// <summary>최대 펫 장착 슬롯 수</summary>
        public new const int MAX_SLOTS = 5;

        public PetModel() : base(MAX_SLOTS) { }

        /// <summary>지정 슬롯에 장착된 펫 ID를 반환한다.</summary>
        public int GetEquippedPetId(int slotIndex) => GetEquippedId(slotIndex);
    }
}
