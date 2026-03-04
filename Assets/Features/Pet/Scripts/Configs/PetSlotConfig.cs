using System;

namespace IdleRPG.Pet
{
    /// <summary>
    /// 펫 장착 슬롯의 해금 조건. 스테이지 진행도에 따라 슬롯이 순차적으로 해금된다.
    /// </summary>
    [Serializable]
    public class PetSlotConfig
    {
        /// <summary>슬롯 인덱스 (0~4)</summary>
        public int SlotIndex;

        /// <summary>해금에 필요한 챕터 번호</summary>
        public int UnlockChapter;

        /// <summary>해금에 필요한 스테이지 번호</summary>
        public int UnlockStage;
    }
}
