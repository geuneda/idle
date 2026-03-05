namespace IdleRPG.Dungeon
{
    /// <summary>
    /// 던전 시스템을 관리하는 서비스 인터페이스.
    /// 던전 진입/퇴장, 보상 지급, 소탕, 타이머 관리를 담당한다.
    /// </summary>
    public interface IDungeonService
    {
        /// <summary>던전 진행 상태 모델</summary>
        DungeonModel Model { get; }

        /// <summary>던전 설정 컬렉션</summary>
        DungeonConfigCollection Config { get; }

        /// <summary>현재 던전 내부에 있는지 여부</summary>
        bool IsInDungeon { get; }

        /// <summary>현재 던전의 남은 시간 (초)</summary>
        float RemainingTime { get; }

        /// <summary>현재 던전 전투 컨텍스트. 던전 외부이면 null</summary>
        DungeonBattleContext CurrentContext { get; }

        /// <summary>
        /// 지정된 던전에 입장 가능한지 확인한다.
        /// </summary>
        /// <param name="type">던전 종류</param>
        /// <param name="level">레벨</param>
        /// <returns>입장 가능하면 true</returns>
        bool CanEnter(DungeonType type, int level);

        /// <summary>
        /// 던전에 입장한다. 전투 컨텍스트를 교체하고 타이머를 시작한다.
        /// </summary>
        /// <param name="type">던전 종류</param>
        /// <param name="level">레벨</param>
        void Enter(DungeonType type, int level);

        /// <summary>
        /// 던전 클리어를 처리한다. 티켓을 소모하고 보상을 지급한다.
        /// </summary>
        /// <param name="type">던전 종류</param>
        /// <param name="level">레벨</param>
        void Complete(DungeonType type, int level);

        /// <summary>
        /// 던전 실패를 처리한다. 티켓을 소모하지 않는다.
        /// </summary>
        /// <param name="type">던전 종류</param>
        /// <param name="level">레벨</param>
        /// <param name="reason">실패 사유</param>
        void Fail(DungeonType type, int level, DungeonFailReason reason);

        /// <summary>
        /// 지정된 던전의 소탕이 가능한지 확인한다.
        /// </summary>
        /// <param name="type">던전 종류</param>
        /// <param name="level">레벨</param>
        /// <returns>소탕 가능하면 true</returns>
        bool CanSweep(DungeonType type, int level);

        /// <summary>
        /// 던전을 소탕한다. 즉시 보상을 지급하고 티켓을 소모한다.
        /// </summary>
        /// <param name="type">던전 종류</param>
        /// <param name="level">레벨</param>
        /// <returns>소탕 성공 여부</returns>
        bool Sweep(DungeonType type, int level);

        /// <summary>
        /// 던전에서 퇴장하고 스테이지 전투로 복귀한다.
        /// </summary>
        void ExitDungeon();

        /// <summary>
        /// 지정 던전 타입의 남은 입장 횟수를 반환한다.
        /// </summary>
        /// <param name="type">던전 종류</param>
        /// <returns>남은 입장 횟수</returns>
        int GetRemainingEntries(DungeonType type);

        /// <summary>
        /// 지정 던전 타입의 일일 최대 입장 횟수를 반환한다.
        /// </summary>
        /// <param name="type">던전 종류</param>
        /// <returns>최대 입장 횟수</returns>
        int GetMaxDailyEntries(DungeonType type);

        /// <summary>
        /// 지정 던전 타입에서 도전 가능한 최대 레벨을 반환한다. (클리어 레벨 + 1)
        /// </summary>
        /// <param name="type">던전 종류</param>
        /// <returns>도전 가능한 최대 레벨</returns>
        int GetAvailableMaxLevel(DungeonType type);

        /// <summary>
        /// UTC 자정 기준 일일 리셋을 확인하고 필요 시 실행한다.
        /// </summary>
        void CheckAndResetDaily();
    }
}
