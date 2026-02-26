using Geuneda.DataExtensions;

namespace IdleRPG.Battle
{
    /// <summary>
    /// 전투의 런타임 상태를 관리하는 POCO 모델.
    /// </summary>
    public class BattleModel
    {
        /// <summary>전투 진행 중 여부</summary>
        public ObservableField<bool> IsBattleActive { get; }

        /// <summary>현재 웨이브에서 생존 중인 적 수</summary>
        public int AliveEnemyCount;

        /// <summary>현재 웨이브의 총 적 수</summary>
        public int CurrentWaveEnemyCount;

        /// <summary>
        /// 비활성 상태로 초기화된 전투 모델을 생성한다.
        /// </summary>
        public BattleModel()
        {
            IsBattleActive = new ObservableField<bool>(false);
        }
    }
}
