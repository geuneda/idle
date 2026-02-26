using Geuneda.Services;
using IdleRPG.Core;

namespace IdleRPG.Core
{
    /// <summary>
    /// 전투 시작 시 발행되는 메시지.
    /// </summary>
    public struct BattleStartedMessage : IMessage
    {
        /// <summary>현재 챕터 번호</summary>
        public int Chapter;

        /// <summary>현재 스테이지 번호</summary>
        public int Stage;

        /// <summary>현재 웨이브 인덱스</summary>
        public int Wave;
    }

    /// <summary>
    /// 적이 스폰되었을 때 발행되는 메시지.
    /// </summary>
    public struct EnemySpawnedMessage : IMessage
    {
        /// <summary>스폰된 적의 인덱스</summary>
        public int EnemyIndex;

        /// <summary>보스 여부</summary>
        public bool IsBoss;
    }

    /// <summary>
    /// 적이 사망했을 때 발행되는 메시지.
    /// </summary>
    public struct EnemyDiedMessage : IMessage
    {
        /// <summary>사망한 적의 인덱스</summary>
        public int EnemyIndex;

        /// <summary>보스 여부</summary>
        public bool IsBoss;
    }

    /// <summary>
    /// 영웅이 사망했을 때 발행되는 메시지.
    /// </summary>
    public struct HeroDiedMessage : IMessage { }

    /// <summary>
    /// 현재 웨이브의 모든 적이 처치되었을 때 발행되는 메시지.
    /// </summary>
    public struct AllEnemiesClearedMessage : IMessage
    {
        /// <summary>클리어된 챕터 번호</summary>
        public int Chapter;

        /// <summary>클리어된 스테이지 번호</summary>
        public int Stage;

        /// <summary>클리어된 웨이브 인덱스</summary>
        public int Wave;
    }

    /// <summary>
    /// 영웅이 피해를 받았을 때 발행되는 메시지.
    /// </summary>
    public struct HeroDamagedMessage : IMessage
    {
        /// <summary>받은 피해량</summary>
        public BigNumber Damage;

        /// <summary>피해 후 남은 체력</summary>
        public BigNumber RemainingHp;
    }

    /// <summary>
    /// 적이 피해를 받았을 때 발행되는 메시지.
    /// </summary>
    public struct EnemyDamagedMessage : IMessage
    {
        /// <summary>피해를 받은 적의 인덱스</summary>
        public int EnemyIndex;

        /// <summary>받은 피해량</summary>
        public BigNumber Damage;

        /// <summary>치명타 여부</summary>
        public bool IsCritical;
    }
}
