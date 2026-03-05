namespace IdleRPG.Core
{
    /// <summary>
    /// 전투 모드별 로직을 추상화하는 전략 인터페이스.
    /// 스테이지 전투와 던전 전투의 공통 계약을 정의한다.
    /// </summary>
    public interface IBattleContext
    {
        /// <summary>현재 웨이브의 적 출현 수를 반환한다.</summary>
        int GetWaveEnemyCount();

        /// <summary>현재 적 체력 배율을 반환한다.</summary>
        BigNumber GetHpMultiplier();

        /// <summary>현재 적 공격력 배율을 반환한다.</summary>
        BigNumber GetAttackMultiplier();

        /// <summary>현재 웨이브가 보스 웨이브인지 확인한다.</summary>
        bool IsBossWave();

        /// <summary>영웅 사망 후 자동으로 전투를 재시작할지 여부</summary>
        bool ShouldAutoRestart { get; }

        /// <summary>웨이브가 시작될 때 호출된다.</summary>
        void OnWaveStarted();

        /// <summary>
        /// 현재 웨이브의 모든 적이 처치되었을 때 호출된다.
        /// </summary>
        /// <returns>true면 다음 웨이브를 계속 진행, false면 전투 종료</returns>
        bool OnAllEnemiesCleared();

        /// <summary>영웅이 사망했을 때 호출된다.</summary>
        void OnHeroDied();

        /// <summary>
        /// 매 프레임 호출되어 컨텍스트별 타이머 등을 갱신한다.
        /// 스테이지 컨텍스트에서는 아무 동작도 하지 않는다.
        /// </summary>
        /// <param name="deltaTime">경과 시간</param>
        void Tick(float deltaTime);
    }
}
