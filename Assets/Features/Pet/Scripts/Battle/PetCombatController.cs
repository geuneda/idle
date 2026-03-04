using System.Collections.Generic;
using Geuneda.Services;
using IdleRPG.Battle;
using IdleRPG.Core;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace IdleRPG.Pet
{
    /// <summary>
    /// <see cref="IPetCombatService"/>의 구현체.
    /// 장착된 펫의 공격 타이머를 관리하고 투사체를 발사한다.
    /// 전투 시작 시 Addressables로 애니메이터/PetView를 로드하고, 종료 시 해제한다.
    /// </summary>
    public class PetCombatController : IPetCombatService
    {
        private static readonly Vector3[] SlotOffsets =
        {
            new Vector3(-0.8f, +0.6f, 0f),
            new Vector3(-0.8f, -0.6f, 0f),
            new Vector3(-1.2f, +0.3f, 0f),
            new Vector3(-1.2f, -0.3f, 0f),
            new Vector3(-1.5f, 0.0f, 0f)
        };

        private struct SlotCache
        {
            public int PetId;
            public RuntimeAnimatorController BodyAnimator;
            public RuntimeAnimatorController ProjectileAnimator;
            public AsyncOperationHandle<RuntimeAnimatorController> BodyHandle;
            public AsyncOperationHandle<RuntimeAnimatorController> ProjectileHandle;
            public PetView View;
            public bool IsLoaded;
        }

        private readonly IPetService _petService;
        private readonly IMessageBrokerService _messageBroker;
        private readonly IPoolService _poolService;
        private readonly float[] _attackTimers = new float[PetModel.MAX_SLOTS];
        private readonly SlotCache[] _slotCaches = new SlotCache[PetModel.MAX_SLOTS];
        private bool _battleActive;
        private int _loadSession;

        /// <summary>
        /// <see cref="PetCombatController"/>의 새 인스턴스를 생성한다.
        /// </summary>
        /// <param name="petService">펫 데이터 및 설정 접근 서비스</param>
        /// <param name="messageBroker">이벤트 발행용 메시지 브로커</param>
        /// <param name="poolService">오브젝트 풀 서비스</param>
        public PetCombatController(
            IPetService petService,
            IMessageBrokerService messageBroker,
            IPoolService poolService)
        {
            _petService = petService;
            _messageBroker = messageBroker;
            _poolService = poolService;

            RegisterProjectilePool();
            RegisterPetViewPool();
        }

        /// <summary>
        /// 펫 투사체용 오브젝트 풀을 등록한다.
        /// </summary>
        private void RegisterProjectilePool()
        {
            var templateGo = new GameObject("PetProjectileTemplate");
            templateGo.SetActive(false);
            Object.DontDestroyOnLoad(templateGo);
            templateGo.AddComponent<SpriteRenderer>();
            templateGo.AddComponent<Animator>();
            var template = templateGo.AddComponent<PetProjectileView>();
            var pool = new GameObjectPool<PetProjectileView>(10, template);
            _poolService.AddPool(pool);
        }

        /// <summary>
        /// 펫 뷰용 오브젝트 풀을 등록한다.
        /// </summary>
        private void RegisterPetViewPool()
        {
            var templateGo = new GameObject("PetViewTemplate");
            templateGo.SetActive(false);
            Object.DontDestroyOnLoad(templateGo);
            templateGo.AddComponent<SpriteRenderer>();
            templateGo.AddComponent<Animator>();
            var template = templateGo.AddComponent<PetView>();
            var pool = new GameObjectPool<PetView>(PetModel.MAX_SLOTS, template);
            _poolService.AddPool(pool);
        }

        /// <inheritdoc/>
        public void Tick(float dt, BigNumber heroAttack, Vector3 heroPosition, float heroAttackRange,
            IReadOnlyList<ISkillTarget> targets)
        {
            for (int slot = 0; slot < PetModel.MAX_SLOTS; slot++)
            {
                int petId = _petService.Model.GetEquippedPetId(slot);
                if (petId == PetModel.UNEQUIPPED) continue;

                PetEntry entry = _petService.Config.GetEntry(petId);
                if (entry == null) continue;

                CollectibleItemState state = _petService.Model.GetItemState(petId);
                if (state == null || !state.IsUnlocked) continue;

                Vector3 petPosition = heroPosition + SlotOffsets[slot];

                if (_slotCaches[slot].View != null)
                {
                    _slotCaches[slot].View.SetPosition(petPosition);
                }

                float attackSpeed = entry.AttackSpeed;
                if (attackSpeed <= 0f) continue;

                _attackTimers[slot] += dt;
                float attackInterval = 1f / attackSpeed;

                if (_attackTimers[slot] >= attackInterval)
                {
                    _attackTimers[slot] -= attackInterval;

                    ISkillTarget nearest = FindNearestAliveTarget(petPosition, heroAttackRange, targets);
                    if (nearest == null) continue;

                    double damagePercent = PetFormula.CalcDamagePercent(entry, state.Level.Value);
                    BigNumber damage = PetFormula.CalcPetDamage(heroAttack, damagePercent);

                    var data = new PetProjectileData
                    {
                        Target = nearest as EnemyView,
                        Damage = damage,
                        IsCritical = false,
                        Speed = entry.ProjectileSpeed,
                        AnimatorController = _slotCaches[slot].ProjectileAnimator
                    };

                    if (data.Target == null) continue;

                    var proj = _poolService.Spawn<PetProjectileView, PetProjectileData>(data);
                    proj.Init(_poolService, _messageBroker);
                    proj.transform.position = petPosition;

                    if (_slotCaches[slot].View != null)
                    {
                        _slotCaches[slot].View.PlayAttack();
                    }
                }
            }
        }

        /// <inheritdoc/>
        public void OnBattleStarted()
        {
            ResetTimers();
            _battleActive = true;
            _loadSession++;
            LoadEquippedPetAssetsAsync(_loadSession);
        }

        /// <inheritdoc/>
        public void OnBattleEnded()
        {
            _battleActive = false;
            _loadSession++;
            ResetTimers();
            ReleaseAllSlotCaches();
        }

        /// <summary>
        /// 장착된 펫의 애니메이터 에셋을 비동기로 로드하고 PetView를 스폰한다.
        /// </summary>
        /// <param name="session">로드 세션 ID. 세션 불일치 시 로드를 중단한다.</param>
        private async void LoadEquippedPetAssetsAsync(int session)
        {
            try
            {
                for (int slot = 0; slot < PetModel.MAX_SLOTS; slot++)
                {
                    if (session != _loadSession) return;

                    int petId = _petService.Model.GetEquippedPetId(slot);
                    if (petId == PetModel.UNEQUIPPED) continue;

                    PetEntry entry = _petService.Config.GetEntry(petId);
                    if (entry == null) continue;

                    var cache = new SlotCache { PetId = petId };

                    if (!string.IsNullOrEmpty(entry.AnimatorKey))
                    {
                        cache.BodyHandle = Addressables.LoadAssetAsync<RuntimeAnimatorController>(entry.AnimatorKey);
                        cache.BodyAnimator = await cache.BodyHandle.Task;
                        if (session != _loadSession) { ReleaseHandle(cache.BodyHandle); return; }
                    }

                    if (!string.IsNullOrEmpty(entry.ProjectileAnimatorKey))
                    {
                        cache.ProjectileHandle = Addressables.LoadAssetAsync<RuntimeAnimatorController>(entry.ProjectileAnimatorKey);
                        cache.ProjectileAnimator = await cache.ProjectileHandle.Task;
                        if (session != _loadSession) { ReleaseHandle(cache.BodyHandle); ReleaseHandle(cache.ProjectileHandle); return; }
                    }

                    cache.IsLoaded = true;
                    _slotCaches[slot] = cache;

                    SpawnPetView(slot);
                }
            }
            catch (System.Exception ex)
            {
                DevLog.LogWarning($"[PetCombat] 펫 에셋 로드 실패: {ex.Message}");
            }
        }

        /// <summary>
        /// 지정 슬롯에 PetView를 스폰한다.
        /// </summary>
        /// <param name="slot">슬롯 인덱스</param>
        private void SpawnPetView(int slot)
        {
            var viewData = new PetViewData
            {
                AnimatorController = _slotCaches[slot].BodyAnimator
            };

            var view = _poolService.Spawn<PetView, PetViewData>(viewData);
            var cache = _slotCaches[slot];
            cache.View = view;
            _slotCaches[slot] = cache;
        }

        /// <summary>
        /// 모든 슬롯의 PetView를 디스폰하고 Addressables 핸들을 해제한다.
        /// </summary>
        private void ReleaseAllSlotCaches()
        {
            for (int i = 0; i < _slotCaches.Length; i++)
            {
                ReleaseSlotCache(i);
            }
        }

        /// <summary>
        /// 지정 슬롯의 PetView 디스폰 및 Addressables 핸들 해제.
        /// </summary>
        /// <param name="slot">슬롯 인덱스</param>
        private void ReleaseSlotCache(int slot)
        {
            var cache = _slotCaches[slot];

            if (cache.View != null)
            {
                _poolService.Despawn(cache.View);
            }

            ReleaseHandle(cache.BodyHandle);
            ReleaseHandle(cache.ProjectileHandle);

            _slotCaches[slot] = default;
        }

        /// <summary>
        /// Addressables 핸들이 유효하면 해제한다.
        /// </summary>
        private static void ReleaseHandle(AsyncOperationHandle<RuntimeAnimatorController> handle)
        {
            if (handle.IsValid())
            {
                Addressables.Release(handle);
            }
        }

        /// <summary>
        /// 모든 슬롯의 공격 타이머를 초기화한다.
        /// </summary>
        private void ResetTimers()
        {
            for (int i = 0; i < _attackTimers.Length; i++)
            {
                _attackTimers[i] = 0f;
            }
        }

        /// <summary>
        /// 펫 위치에서 사거리 내 가장 가까운 생존 대상을 찾는다.
        /// </summary>
        /// <param name="petPosition">펫 월드 좌표</param>
        /// <param name="attackRange">공격 사거리</param>
        /// <param name="targets">현재 활성 대상 목록</param>
        /// <returns>가장 가까운 생존 대상. 없으면 null</returns>
        private static ISkillTarget FindNearestAliveTarget(
            Vector3 petPosition, float attackRange, IReadOnlyList<ISkillTarget> targets)
        {
            ISkillTarget nearest = null;
            float nearestDist = float.MaxValue;

            for (int i = 0; i < targets.Count; i++)
            {
                var target = targets[i];
                if (!target.IsAlive) continue;

                float dist = Vector3.Distance(petPosition, target.Position);
                if (dist <= attackRange && dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = target;
                }
            }

            return nearest;
        }
    }
}
