using IdleRPG.Core;
using IdleRPG.Hero;

namespace IdleRPG.Battle
{
    /// <summary>
    /// 전투 로직을 제공하는 서비스 인터페이스.
    /// 전투 시작/종료, 데미지 계산, 적 처치 처리, 스테이지 배율 조회를 담당한다.
    /// </summary>
    /// <remarks>
    /// <see cref="Geuneda.Services.MainInstaller"/>에 바인딩하여 사용한다.
    /// </remarks>
    public interface IBattleService
    {
        /// <summary>전투 런타임 상태 모델</summary>
        BattleModel Model { get; }

        /// <summary>영웅 런타임 상태 모델</summary>
        HeroModel HeroModel { get; }

        /// <summary>일반 적 설정 데이터</summary>
        EnemyConfig NormalEnemyConfig { get; }

        /// <summary>보스 적 설정 데이터</summary>
        EnemyConfig BossEnemyConfig { get; }

        /// <summary>
        /// 전투를 시작한다. 영웅을 완전 회복하고 웨이브를 개시한다.
        /// </summary>
        void StartBattle();

        /// <summary>
        /// 적이 사망했을 때 호출한다. 모든 적이 처치되면 다음 웨이브를 진행한다.
        /// </summary>
        /// <param name="enemyIndex">사망한 적의 인덱스</param>
        /// <param name="isBoss">보스 여부</param>
        void OnEnemyDied(int enemyIndex, bool isBoss);

        /// <summary>
        /// 영웅이 사망했을 때 호출한다. 전투를 종료하고 보스 웨이브 실패를 처리한다.
        /// </summary>
        void OnHeroDied();

        /// <summary>
        /// 영웅의 공격 데미지를 계산한다. 치명타 확률에 따라 추가 피해가 적용된다.
        /// </summary>
        /// <param name="isCritical">치명타 발생 여부</param>
        /// <returns>계산된 데미지</returns>
        BigNumber CalculateHeroDamage(out bool isCritical);

        /// <summary>
        /// 현재 스테이지의 적 체력 배율을 반환한다.
        /// </summary>
        /// <returns>체력 배율</returns>
        BigNumber GetCurrentHpMultiplier();

        /// <summary>
        /// 현재 스테이지의 적 공격력 배율을 반환한다.
        /// </summary>
        /// <returns>공격력 배율</returns>
        BigNumber GetCurrentAttackMultiplier();

        /// <summary>
        /// 현재 웨이브의 적 출현 수를 반환한다.
        /// </summary>
        /// <returns>적 출현 수</returns>
        int GetCurrentWaveEnemyCount();

        /// <summary>
        /// 이중/삼중 사격 확률 판정을 수행하여 추가 투사체 수를 반환한다.
        /// </summary>
        /// <returns>추가로 발사할 투사체 수 (0, 1, 또는 2)</returns>
        int RollExtraShots();

        /// <summary>
        /// 전투 컨텍스트를 교체한다. 던전 진입/퇴장 시 사용한다.
        /// </summary>
        /// <param name="context">새 전투 컨텍스트</param>
        void SetBattleContext(IBattleContext context);

        /// <summary>
        /// 현재 웨이브가 보스 웨이브인지 확인한다.
        /// </summary>
        /// <returns>보스 웨이브이면 true</returns>
        bool IsBossWave();

        /// <summary>
        /// 영웅 사망 후 자동으로 전투를 재시작할지 여부를 반환한다.
        /// </summary>
        bool ShouldAutoRestart { get; }

        /// <summary>
        /// 매 프레임 호출되어 컨텍스트 타이머를 갱신한다.
        /// </summary>
        /// <param name="deltaTime">경과 시간</param>
        void Tick(float deltaTime);
    }
}
