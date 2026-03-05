using System;
using Geuneda.Services;
using IdleRPG.Core;
using IdleRPG.Hero;

namespace IdleRPG.Battle
{
    /// <summary>
    /// <see cref="IBattleService"/>의 구현체. 전투 시작/종료, 데미지 계산,
    /// 적 처치 처리, 웨이브 진행을 관리한다.
    /// </summary>
    public class BattleService : IBattleService
    {
        private IBattleContext _context;
        private readonly IMessageBrokerService _messageBroker;
        private readonly Random _random = new Random();

        /// <inheritdoc />
        public BattleModel Model { get; }

        /// <inheritdoc />
        public HeroModel HeroModel { get; }

        /// <inheritdoc />
        public EnemyConfig NormalEnemyConfig { get; }

        /// <inheritdoc />
        public EnemyConfig BossEnemyConfig { get; }

        /// <inheritdoc />
        public bool ShouldAutoRestart => _context.ShouldAutoRestart;

        /// <summary>
        /// <see cref="BattleService"/>를 생성한다.
        /// </summary>
        /// <param name="context">초기 전투 컨텍스트 (스테이지)</param>
        /// <param name="heroModel">영웅 상태 모델</param>
        /// <param name="normalEnemyConfig">일반 적 설정</param>
        /// <param name="bossEnemyConfig">보스 적 설정</param>
        /// <param name="messageBroker">이벤트 발행용 메시지 브로커</param>
        public BattleService(
            IBattleContext context,
            HeroModel heroModel,
            EnemyConfig normalEnemyConfig,
            EnemyConfig bossEnemyConfig,
            IMessageBrokerService messageBroker)
        {
            _context = context;
            _messageBroker = messageBroker;
            HeroModel = heroModel;
            NormalEnemyConfig = normalEnemyConfig;
            BossEnemyConfig = bossEnemyConfig;
            Model = new BattleModel();
        }

        /// <inheritdoc />
        public void SetBattleContext(IBattleContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        public bool IsBossWave()
        {
            return _context.IsBossWave();
        }

        /// <inheritdoc />
        public void StartBattle()
        {
            HeroModel.FullHeal();
            _context.OnWaveStarted();

            int enemyCount = GetCurrentWaveEnemyCount();
            Model.CurrentWaveEnemyCount = enemyCount;
            Model.AliveEnemyCount = enemyCount;
            Model.IsBattleActive.Value = true;

            _messageBroker.Publish(new BattleStartedMessage());
        }

        /// <inheritdoc />
        public void OnEnemyDied(int enemyIndex, bool isBoss)
        {
            _messageBroker.Publish(new EnemyDiedMessage
            {
                EnemyIndex = enemyIndex,
                IsBoss = isBoss
            });

            Model.AliveEnemyCount--;

            if (Model.AliveEnemyCount <= 0)
            {
                OnAllEnemiesCleared();
            }
        }

        /// <inheritdoc />
        public void OnHeroDied()
        {
            Model.IsBattleActive.Value = false;
            _messageBroker.Publish(new HeroDiedMessage());
            _context.OnHeroDied();
        }

        /// <inheritdoc />
        public BigNumber CalculateHeroDamage(out bool isCritical)
        {
            BigNumber baseDamage = HeroModel.Attack.Value;
            float critRate = HeroModel.CritRate.Value;
            float critDamage = HeroModel.CritDamage.Value;

            isCritical = _random.NextDouble() < critRate;
            if (isCritical)
            {
                baseDamage = baseDamage * (double)critDamage;
            }

            float advancedBonus = HeroModel.AdvancedAttackBonus.Value;
            if (advancedBonus > 0f)
            {
                baseDamage = baseDamage * (1.0 + advancedBonus);
            }

            float enemyBonus = HeroModel.EnemyBonusDamage.Value;
            if (enemyBonus > 0f)
            {
                baseDamage = baseDamage * (1.0 + enemyBonus);
            }

            return baseDamage;
        }

        /// <inheritdoc />
        public int RollExtraShots()
        {
            int extra = 0;

            float tripleRate = HeroModel.TripleShotRate.Value;
            if (tripleRate > 0f && _random.NextDouble() < tripleRate)
            {
                extra = 2;
                return extra;
            }

            float doubleRate = HeroModel.DoubleShotRate.Value;
            if (doubleRate > 0f && _random.NextDouble() < doubleRate)
            {
                extra = 1;
            }

            return extra;
        }

        /// <inheritdoc />
        public BigNumber GetCurrentHpMultiplier()
        {
            return _context.GetHpMultiplier();
        }

        /// <inheritdoc />
        public BigNumber GetCurrentAttackMultiplier()
        {
            return _context.GetAttackMultiplier();
        }

        /// <inheritdoc />
        public int GetCurrentWaveEnemyCount()
        {
            return _context.GetWaveEnemyCount();
        }

        /// <inheritdoc />
        public void Tick(float deltaTime)
        {
            _context.Tick(deltaTime);
        }

        /// <summary>
        /// 현재 웨이브의 모든 적이 처치되었을 때 호출된다.
        /// 컨텍스트에 위임하고 반환값에 따라 다음 전투를 시작하거나 전투를 종료한다.
        /// </summary>
        private void OnAllEnemiesCleared()
        {
            bool continueNext = _context.OnAllEnemiesCleared();

            if (continueNext)
            {
                StartBattle();
            }
            else
            {
                Model.IsBattleActive.Value = false;
            }
        }
    }
}
