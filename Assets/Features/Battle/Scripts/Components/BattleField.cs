using System.Collections.Generic;
using Geuneda.Services;
using IdleRPG.Core;
using IdleRPG.Hero;
using UnityEngine;

namespace IdleRPG.Battle
{
    /// <summary>
    /// 전투 필드를 관리하는 MonoBehaviour. 적 스폰, 영웅 공격/회복, 투사체 발사 등
    /// 전투 시각 로직을 처리한다.
    /// </summary>
    /// <remarks>
    /// <see cref="IBattleService"/>와 <see cref="IPoolService"/>를 사용하여
    /// 전투 로직과 오브젝트 풀링을 연동한다.
    /// </remarks>
    public class BattleField : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private HeroView _heroView;
        [SerializeField] private EnemyView _enemyPrefab;
        [SerializeField] private ProjectileView _projectilePrefab;

        [Header("Spawn Settings")]
        [SerializeField] private Transform _enemySpawnPoint;
        [SerializeField] private float _enemySpawnSpacing = 1.5f;
        [SerializeField] private float _deathDuration = 0.5f;

        private IBattleService _battleService;
        private IPoolService _poolService;
        private IMessageBrokerService _messageBroker;
        private ISkillExecutionService _skillExecutionService;

        private readonly List<EnemyView> _activeEnemies = new List<EnemyView>();
        private readonly List<EnemyView> _dyingEnemies = new List<EnemyView>();
        private readonly Dictionary<EnemyView, float> _deathTimers = new Dictionary<EnemyView, float>();
        private readonly List<ISkillTarget> _skillTargetCache = new List<ISkillTarget>();

        private float _heroAttackTimer;
        private float _restartTimer = -1f;
        private const float RestartDelay = 2f;
        private bool _initialized;

        private void Start()
        {
            _poolService = MainInstaller.Resolve<IPoolService>();
            _messageBroker = MainInstaller.Resolve<IMessageBrokerService>();

            SetupPools();

            _messageBroker.Subscribe<BattleStartedMessage>(OnBattleStarted);
            _messageBroker.Subscribe<AppFlowReadyMessage>(OnAppFlowReady);
        }

        private void OnAppFlowReady(AppFlowReadyMessage msg)
        {
            _battleService = MainInstaller.Resolve<IBattleService>();
            _skillExecutionService = MainInstaller.Resolve<ISkillExecutionService>();
            _initialized = true;
            _battleService.StartBattle();
        }

        private void OnBattleStarted(BattleStartedMessage msg)
        {
            SpawnEnemies();
        }

        /// <summary>
        /// 적/투사체 오브젝트 풀을 생성하고 <see cref="IPoolService"/>에 등록한다.
        /// </summary>
        private void SetupPools()
        {
            var enemyPool = new GameObjectPool<EnemyView>(10, _enemyPrefab);
            _poolService.AddPool(enemyPool);

            var projPool = new GameObjectPool<ProjectileView>(20, _projectilePrefab);
            _poolService.AddPool(projPool);
        }

        private void Update()
        {
            if (!_initialized) return;

            float dt = Time.deltaTime;

            if (_restartTimer > 0f)
            {
                _restartTimer -= dt;
                UpdateDyingEnemies(dt);
                if (_restartTimer <= 0f)
                {
                    _restartTimer = -1f;
                    _battleService.StartBattle();
                }
                return;
            }

            if (!_battleService.Model.IsBattleActive.Value) return;

            UpdateHeroAttack(dt);
            UpdateHeroRegen(dt);
            UpdateSkillExecution(dt);
            UpdateEnemies(dt);
            UpdateDyingEnemies(dt);
        }

        /// <summary>
        /// 현재 웨이브의 적을 스폰한다. 기존 활성 적을 제거한 뒤 새로 생성한다.
        /// </summary>
        public void SpawnEnemies()
        {
            ClearActiveEnemies();

            int count = _battleService.GetCurrentWaveEnemyCount();
            bool isBossWave = MainInstaller.Resolve<IdleRPG.Stage.IStageService>().IsBossWave();

            BigNumber hpMul = _battleService.GetCurrentHpMultiplier();
            BigNumber atkMul = _battleService.GetCurrentAttackMultiplier();

            for (int i = 0; i < count; i++)
            {
                EnemyConfig config = isBossWave
                    ? _battleService.BossEnemyConfig
                    : _battleService.NormalEnemyConfig;

                var model = new EnemyModel(config, hpMul, atkMul);

                var enemy = _poolService.Spawn<EnemyView, EnemyModel>(model);
                enemy.Index = i;

                float spawnX = _enemySpawnPoint.position.x + i * _enemySpawnSpacing;
                float spawnY = _heroView.transform.position.y;
                enemy.transform.position = new Vector3(spawnX, spawnY, 0f);

                _activeEnemies.Add(enemy);

                _messageBroker.Publish(new EnemySpawnedMessage
                {
                    EnemyIndex = i,
                    IsBoss = isBossWave
                });
            }
        }

        /// <summary>
        /// 영웅의 공격 타이머를 갱신하고 공격 간격에 도달하면 투사체를 발사한다.
        /// </summary>
        /// <param name="dt">프레임 경과 시간</param>
        private void UpdateHeroAttack(float dt)
        {
            if (_heroView.CurrentState == HeroState.Death) return;

            EnemyView nearestInRange = FindNearestAliveEnemyInRange();
            if (nearestInRange == null)
            {
                _heroView.SetState(HeroState.Idle);
                _heroAttackTimer = 0f;
                return;
            }

            _heroView.SetState(HeroState.Attack);
            float attackSpeed = _battleService.HeroModel.AttackSpeed.Value;
            if (attackSpeed <= 0f) return;

            _heroAttackTimer += dt;
            float attackInterval = 1f / attackSpeed;

            if (_heroAttackTimer >= attackInterval)
            {
                _heroAttackTimer -= attackInterval;
                FireProjectile();
                _heroView.PlayAttack();
            }
        }

        /// <summary>
        /// 영웅의 체력을 초당 재생량에 따라 회복시킨다.
        /// </summary>
        /// <param name="dt">프레임 경과 시간</param>
        private void UpdateHeroRegen(float dt)
        {
            var hero = _battleService.HeroModel;
            if (!hero.IsAlive) return;

            BigNumber regen = hero.HpRegen.Value;
            if (regen.IsZero) return;

            BigNumber healAmount = regen * (double)dt;
            hero.Heal(healAmount);
        }

        /// <summary>
        /// 스킬 실행 시스템에 현재 전투 상태를 전달하여 오토캐스트를 처리한다.
        /// </summary>
        /// <param name="dt">프레임 경과 시간</param>
        private void UpdateSkillExecution(float dt)
        {
            if (_skillExecutionService == null) return;
            if (_heroView.CurrentState == HeroState.Death) return;

            BuildSkillTargetCache();
            if (_skillTargetCache.Count == 0) return;

            BigNumber heroAttack = _battleService.HeroModel.Attack.Value;
            Vector3 heroPosition = _heroView.transform.position;

            _skillExecutionService.Tick(dt, heroAttack, heroPosition, _skillTargetCache);
        }

        /// <summary>
        /// 활성 적 목록을 <see cref="ISkillTarget"/> 캐시로 변환한다.
        /// 매 프레임 재사용하여 GC 할당을 방지한다.
        /// </summary>
        private void BuildSkillTargetCache()
        {
            _skillTargetCache.Clear();
            for (int i = 0; i < _activeEnemies.Count; i++)
            {
                var enemy = _activeEnemies[i];
                if (enemy.Model != null && enemy.Model.IsAlive)
                    _skillTargetCache.Add(enemy);
            }
        }

        /// <summary>
        /// 생존 적을 대상으로 투사체를 발사한다.
        /// 이중/삼중 사격 확률에 따라 추가 타겟에 투사체를 발사할 수 있다.
        /// </summary>
        private void FireProjectile()
        {
            EnemyView target = FindNearestAliveEnemy();
            if (target == null) return;

            BigNumber damage = _battleService.CalculateHeroDamage(out bool isCritical);
            float speed = _battleService.HeroModel.ProjectileSpeed;

            Vector3 spawnPos = _heroView.ProjectileSpawnPoint != null
                ? _heroView.ProjectileSpawnPoint.position
                : _heroView.transform.position;

            SpawnProjectile(target, damage, isCritical, speed, spawnPos);

            int extraShots = _battleService.RollExtraShots();
            if (extraShots > 0)
            {
                FireExtraProjectiles(extraShots, target, speed, spawnPos);
            }
        }

        /// <summary>
        /// 추가 타겟에 투사체를 발사한다. 이미 공격 중인 대상을 제외하고 선택한다.
        /// </summary>
        /// <param name="count">추가 투사체 수</param>
        /// <param name="excludeTarget">제외할 주 타겟</param>
        /// <param name="speed">투사체 속도</param>
        /// <param name="spawnPos">투사체 발사 위치</param>
        private void FireExtraProjectiles(int count, EnemyView excludeTarget, float speed, Vector3 spawnPos)
        {
            int fired = 0;
            Vector3 heroPos = _heroView.transform.position;
            var candidates = new List<EnemyView>();

            foreach (var enemy in _activeEnemies)
            {
                if (enemy == excludeTarget) continue;
                if (enemy.Model == null || !enemy.Model.IsAlive) continue;
                candidates.Add(enemy);
            }

            candidates.Sort((a, b) =>
            {
                float dA = Vector3.Distance(heroPos, a.transform.position);
                float dB = Vector3.Distance(heroPos, b.transform.position);
                return dA.CompareTo(dB);
            });

            for (int i = 0; i < candidates.Count && fired < count; i++)
            {
                BigNumber extraDamage = _battleService.CalculateHeroDamage(out bool extraCrit);
                SpawnProjectile(candidates[i], extraDamage, extraCrit, speed, spawnPos);
                fired++;
            }

            if (fired < count)
            {
                for (int i = fired; i < count; i++)
                {
                    BigNumber extraDamage = _battleService.CalculateHeroDamage(out bool extraCrit);
                    SpawnProjectile(excludeTarget, extraDamage, extraCrit, speed, spawnPos);
                }
            }
        }

        /// <summary>
        /// 단일 투사체를 생성하여 대상으로 발사한다.
        /// </summary>
        private void SpawnProjectile(EnemyView target, BigNumber damage, bool isCritical, float speed, Vector3 spawnPos)
        {
            var data = new ProjectileData
            {
                Target = target,
                Damage = damage,
                IsCritical = isCritical,
                Speed = speed
            };

            var proj = _poolService.Spawn<ProjectileView, ProjectileData>(data);
            proj.Init(_poolService, _messageBroker);
            proj.transform.position = spawnPos;
        }

        /// <summary>
        /// 활성 적의 이동과 공격을 갱신한다. 사망한 적은 <see cref="HandleEnemyDeath"/>로 처리한다.
        /// </summary>
        /// <param name="dt">프레임 경과 시간</param>
        private void UpdateEnemies(float dt)
        {
            float heroX = _heroView.transform.position.x;

            for (int i = _activeEnemies.Count - 1; i >= 0; i--)
            {
                var enemy = _activeEnemies[i];

                if (enemy.Model == null || !enemy.Model.IsAlive)
                {
                    HandleEnemyDeath(enemy, i);
                    continue;
                }

                enemy.UpdateMovement(dt, heroX);

                if (enemy.TryAttack(dt))
                {
                    ApplyEnemyAttack(enemy);
                }
            }
        }

        /// <summary>
        /// 적의 공격을 영웅에게 적용하고, 영웅 사망 시 전투 종료를 처리한다.
        /// </summary>
        /// <param name="enemy">공격을 수행한 적 뷰</param>
        private void ApplyEnemyAttack(EnemyView enemy)
        {
            var hero = _battleService.HeroModel;
            if (!hero.IsAlive) return;

            hero.TakeDamage(enemy.Model.Attack);

            _messageBroker.Publish(new HeroDamagedMessage
            {
                Damage = enemy.Model.Attack,
                RemainingHp = hero.Hp.Value
            });

            if (!hero.IsAlive)
            {
                _heroView.SetState(HeroState.Death);
                _battleService.OnHeroDied();
                _restartTimer = RestartDelay;
            }
        }

        /// <summary>
        /// 사망한 적을 활성 목록에서 제거하고 사망 연출 목록으로 이동시킨다.
        /// </summary>
        /// <param name="enemy">사망한 적 뷰</param>
        /// <param name="activeIndex">활성 목록 내 인덱스</param>
        private void HandleEnemyDeath(EnemyView enemy, int activeIndex)
        {
            enemy.Die();
            _activeEnemies.RemoveAt(activeIndex);
            _dyingEnemies.Add(enemy);
            _deathTimers[enemy] = _deathDuration;

            _battleService.OnEnemyDied(enemy.Index, enemy.Model != null && enemy.Model.IsBoss);
        }

        /// <summary>
        /// 사망 연출 중인 적의 타이머를 갱신하고 완료 시 풀로 반환한다.
        /// </summary>
        /// <param name="dt">프레임 경과 시간</param>
        private void UpdateDyingEnemies(float dt)
        {
            for (int i = _dyingEnemies.Count - 1; i >= 0; i--)
            {
                var enemy = _dyingEnemies[i];

                if (_deathTimers.TryGetValue(enemy, out float remaining))
                {
                    remaining -= dt;
                    if (remaining <= 0f)
                    {
                        _dyingEnemies.RemoveAt(i);
                        _deathTimers.Remove(enemy);
                        _poolService.Despawn(enemy);
                    }
                    else
                    {
                        _deathTimers[enemy] = remaining;
                    }
                }
            }
        }

        private EnemyView FindNearestAliveEnemyInRange()
        {
            float range = _battleService.HeroModel.AttackRange;
            EnemyView nearest = null;
            float nearestDist = float.MaxValue;
            Vector3 heroPos = _heroView.transform.position;

            foreach (var enemy in _activeEnemies)
            {
                if (enemy.Model == null || !enemy.Model.IsAlive) continue;

                float dist = Vector3.Distance(heroPos, enemy.transform.position);
                if (dist <= range && dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = enemy;
                }
            }

            return nearest;
        }

        private EnemyView FindNearestAliveEnemy()
        {
            EnemyView nearest = null;
            float nearestDist = float.MaxValue;
            Vector3 heroPos = _heroView.transform.position;

            foreach (var enemy in _activeEnemies)
            {
                if (enemy.Model == null || !enemy.Model.IsAlive) continue;

                float dist = Vector3.Distance(heroPos, enemy.transform.position);
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = enemy;
                }
            }

            return nearest;
        }

        /// <summary>
        /// 모든 활성/사망 연출 중 적을 풀로 반환하고 목록을 초기화한다.
        /// </summary>
        private void ClearActiveEnemies()
        {
            foreach (var enemy in _activeEnemies)
            {
                _poolService.Despawn(enemy);
            }
            _activeEnemies.Clear();

            foreach (var enemy in _dyingEnemies)
            {
                _poolService.Despawn(enemy);
            }
            _dyingEnemies.Clear();
            _deathTimers.Clear();
        }

        private void OnDestroy()
        {
            _messageBroker?.UnsubscribeAll(this);
        }
    }
}
