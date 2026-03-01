using Geuneda.DataExtensions;

namespace IdleRPG.Equipment
{
    /// <summary>
    /// 개별 장비의 런타임 상태. 보유 수량과 강화 레벨을 추적한다.
    /// </summary>
    public class EquipmentItemState
    {
        /// <summary>장비 고유 식별자</summary>
        public int Id { get; }

        /// <summary>보유 수량</summary>
        public ObservableField<int> OwnedCount { get; }

        /// <summary>현재 강화 레벨. 0이면 미해금, 1 이상이면 해금 상태</summary>
        public ObservableField<int> Level { get; }

        /// <summary>해금 여부. 레벨이 1 이상이면 해금된 상태이다</summary>
        public bool IsUnlocked => Level.Value > 0;

        /// <summary>
        /// 장비 상태를 초기화한다.
        /// </summary>
        /// <param name="id">장비 고유 식별자</param>
        /// <param name="ownedCount">초기 보유 수량</param>
        /// <param name="level">초기 레벨</param>
        public EquipmentItemState(int id, int ownedCount = 0, int level = 0)
        {
            Id = id;
            OwnedCount = new ObservableField<int>(ownedCount);
            Level = new ObservableField<int>(level);
        }
    }
}
