using System.Collections.Generic;
using Geuneda.Services;
using IdleRPG.Core;
using UnityEngine;

namespace IdleRPG.Skill
{
    /// <summary>
    /// <see cref="ISkillExecutionService"/>의 구현체.
    /// 장착된 스킬의 쿨타임 관리, 오토캐스트/수동 발동, 지속 효과 갱신을 처리한다.
    /// </summary>
    /// <remarks>
    /// 스킬 ID별로 <see cref="ISkillEffectExecutor"/>를 등록하여
    /// 21개 스킬 각각의 고유 효과를 처리한다.
    /// </remarks>
    public class SkillExecutionService : ISkillExecutionService
    {
        private readonly ISkillService _skillService;
        private readonly IMessageBrokerService _messageBroker;
        private readonly Dictionary<int, ISkillEffectExecutor> _executors = new();
        private readonly float[] _cooldownTimers = new float[SkillModel.MAX_SLOTS];
        private readonly SkillExecutionContext _context = new();
        private readonly List<ISustainedEffect> _activeSustainedEffects = new();

        /// <summary>
        /// <see cref="SkillExecutionService"/>의 새 인스턴스를 생성한다.
        /// </summary>
        /// <param name="skillService">스킬 데이터 및 설정 접근 서비스</param>
        /// <param name="messageBroker">이벤트 발행용 메시지 브로커</param>
        public SkillExecutionService(
            ISkillService skillService,
            IMessageBrokerService messageBroker)
        {
            _skillService = skillService;
            _messageBroker = messageBroker;

            _context.MessageBroker = _messageBroker;
            _context.RegisterSustainedEffect = RegisterSustainedEffect;
            _context.ReduceLongestCooldown = ReduceLongestCooldown;

            RegisterExecutors();
        }

        /// <inheritdoc/>
        public void Tick(float dt, BigNumber heroAttack, Vector3 heroPosition, IReadOnlyList<ISkillTarget> targets)
        {
            for (int i = 0; i < SkillModel.MAX_SLOTS; i++)
            {
                if (_cooldownTimers[i] > 0f)
                    _cooldownTimers[i] -= dt;
            }

            _context.HeroAttack = heroAttack;
            _context.HeroPosition = heroPosition;
            _context.Targets = targets;

            TickSustainedEffects(dt);

            for (int i = 0; i < SkillModel.MAX_SLOTS; i++)
            {
                if (TryActivateSlot(i, heroAttack, heroPosition, targets))
                    break;
            }
        }

        /// <inheritdoc/>
        public bool TryManualActivate(int slotIndex, BigNumber heroAttack, Vector3 heroPosition, IReadOnlyList<ISkillTarget> targets)
        {
            return TryActivateSlot(slotIndex, heroAttack, heroPosition, targets);
        }

        /// <inheritdoc/>
        public float GetCooldownRemaining(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= SkillModel.MAX_SLOTS) return 0f;
            return _cooldownTimers[slotIndex] > 0f ? _cooldownTimers[slotIndex] : 0f;
        }

        /// <inheritdoc/>
        public float GetCooldownTotal(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= SkillModel.MAX_SLOTS) return 0f;
            int skillId = _skillService.Model.GetEquippedSkillId(slotIndex);
            if (skillId == SkillModel.UNEQUIPPED) return 0f;

            var entry = _skillService.Config.GetEntry(skillId);
            return entry?.Cooldown ?? 0f;
        }

        /// <inheritdoc/>
        public void ResetState()
        {
            _activeSustainedEffects.Clear();
            for (int i = 0; i < _cooldownTimers.Length; i++)
                _cooldownTimers[i] = 0f;
        }

        /// <summary>
        /// 지정 슬롯의 스킬 발동을 시도한다.
        /// </summary>
        private bool TryActivateSlot(int slotIndex, BigNumber heroAttack, Vector3 heroPosition, IReadOnlyList<ISkillTarget> targets)
        {
            if (slotIndex < 0 || slotIndex >= SkillModel.MAX_SLOTS) return false;
            if (_cooldownTimers[slotIndex] > 0f) return false;

            int skillId = _skillService.Model.GetEquippedSkillId(slotIndex);
            if (skillId == SkillModel.UNEQUIPPED) return false;

            var entry = _skillService.Config.GetEntry(skillId);
            if (entry == null) return false;

            if (!_executors.TryGetValue(entry.Id, out var executor)) return false;

            if (targets == null || targets.Count == 0) return false;

            var state = _skillService.Model.GetItemState(skillId);
            int level = state?.Level.Value ?? 1;
            double damagePercent = SkillFormula.CalcDamagePercent(entry, level);

            _context.HeroAttack = heroAttack;
            _context.HeroPosition = heroPosition;
            _context.Targets = targets;

            executor.Execute(_context, entry, damagePercent);

            _cooldownTimers[slotIndex] = entry.Cooldown;

            return true;
        }

        /// <summary>
        /// 활성 지속 효과를 매 프레임 갱신하고 만료된 효과를 제거한다.
        /// </summary>
        private void TickSustainedEffects(float dt)
        {
            for (int i = _activeSustainedEffects.Count - 1; i >= 0; i--)
            {
                _activeSustainedEffects[i].Tick(dt, _context);

                if (_activeSustainedEffects[i].IsExpired)
                    _activeSustainedEffects.RemoveAt(i);
            }
        }

        /// <summary>
        /// 지속 효과를 활성 목록에 등록한다.
        /// </summary>
        private void RegisterSustainedEffect(ISustainedEffect effect)
        {
            _activeSustainedEffects.Add(effect);
        }

        /// <summary>
        /// 장착 슬롯 중 가장 긴 남은 쿨타임을 비율만큼 감소시킨다.
        /// </summary>
        /// <param name="reductionPercent">감소 비율 (%)</param>
        private void ReduceLongestCooldown(float reductionPercent)
        {
            int longestSlot = -1;
            float longestTime = 0f;

            for (int i = 0; i < SkillModel.MAX_SLOTS; i++)
            {
                if (_cooldownTimers[i] > longestTime)
                {
                    longestTime = _cooldownTimers[i];
                    longestSlot = i;
                }
            }

            if (longestSlot < 0) return;

            float reduction = longestTime * reductionPercent / 100f;
            _cooldownTimers[longestSlot] -= reduction;
            if (_cooldownTimers[longestSlot] < 0f)
                _cooldownTimers[longestSlot] = 0f;

            _messageBroker.Publish(new SkillCooldownReducedMessage
            {
                SlotIndex = longestSlot,
                ReductionPercent = reductionPercent
            });
        }

        /// <summary>
        /// 21개 스킬별 실행기를 등록한다.
        /// </summary>
        private void RegisterExecutors()
        {
            // Grade 0 - 일반
            Register(new FairySummonExecutor());
            Register(new SwordCreationExecutor());
            Register(new GiantRockExecutor());

            // Grade 1 - 고급
            Register(new AttackSpeedBuffExecutor());
            Register(new IceShardExecutor());
            Register(new LightningExecutor());

            // Grade 2 - 희귀
            Register(new PhoenixExecutor());
            Register(new BlizzardExecutor());
            Register(new AnimalPackExecutor());

            // Grade 3 - 에픽
            Register(new RockRainExecutor());
            Register(new MeteorExecutor());
            Register(new BarrelTossExecutor());

            // Grade 4 - 전설
            Register(new WindOrbExecutor());
            Register(new DragonExecutor());
            Register(new GreatSwordExecutor());

            // Grade 5 - 신화
            Register(new MagicCircleExecutor());
            Register(new EntitySummonExecutor());
            Register(new GodSummonExecutor());

            // Grade 6 - 특별
            Register(new DarkOrbExecutor());
            Register(new DemonExecutor());
            Register(new EnergyBurstExecutor());
        }

        private void Register(ISkillEffectExecutor executor)
        {
            _executors[executor.SkillId] = executor;
        }
    }
}
