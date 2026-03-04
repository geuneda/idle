using Geuneda.DataExtensions;

namespace IdleRPG.Pet
{
    /// <summary>
    /// 개별 펫의 런타임 상태. 보유 수량과 강화 레벨을 <see cref="ObservableField{T}"/>로 관리한다.
    /// </summary>
    public class PetItemState
    {
        /// <summary>펫 고유 식별자</summary>
        public int Id { get; }

        /// <summary>보유 수량</summary>
        public ObservableField<int> OwnedCount { get; }

        /// <summary>강화 레벨. 0이면 미해금</summary>
        public ObservableField<int> Level { get; }

        /// <summary>해금 여부. 레벨이 1 이상이면 해금된 상태</summary>
        public bool IsUnlocked => Level.Value > 0;

        public PetItemState(int id, int ownedCount = 0, int level = 0)
        {
            Id = id;
            OwnedCount = new ObservableField<int>(ownedCount);
            Level = new ObservableField<int>(level);
        }
    }
}
