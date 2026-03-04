using System;
using Geuneda.Services;
using IdleRPG.Skill;

namespace IdleRPG.Core
{
    /// <summary>
    /// 스킬 보유/장착 상태의 저장/로드를 담당한다.
    /// </summary>
    public class SkillSaveCollector : ISaveDataCollector
    {
        private readonly SkillModel _skillModel;
        private Action _markDirty;

        public SkillSaveCollector(SkillModel skillModel)
        {
            _skillModel = skillModel;
        }

        /// <inheritdoc />
        public void ExtractTo(GameSaveData saveData)
        {
            var data = new SkillSaveData();
            foreach (var item in _skillModel.GetAllItems())
            {
                if (item.OwnedCount.Value <= 0 && item.Level.Value <= 0) continue;

                data.Items[item.Id] = new SkillItemSaveEntry
                {
                    OwnedCount = item.OwnedCount.Value,
                    Level = item.Level.Value
                };
            }

            for (int i = 0; i < SkillModel.MAX_SLOTS; i++)
            {
                int skillId = _skillModel.GetEquippedSkillId(i);
                if (skillId != SkillModel.UNEQUIPPED)
                {
                    data.Equipped[i] = skillId;
                }
            }

            saveData.Skill = data;
        }

        /// <inheritdoc />
        public void ApplyFrom(GameSaveData saveData)
        {
            var data = saveData.Skill;
            if (data == null) return;

            foreach (var pair in data.Items)
            {
                var state = _skillModel.GetOrCreateItemState(pair.Key);
                state.OwnedCount.Value = pair.Value.OwnedCount;
                state.Level.Value = pair.Value.Level;
            }

            foreach (var pair in data.Equipped)
            {
                if (pair.Key < 0 || pair.Key >= SkillModel.MAX_SLOTS) continue;
                _skillModel.Equip(pair.Key, pair.Value);
            }
        }

        /// <inheritdoc />
        public void SubscribeDirtyEvents(IMessageBrokerService broker, Action markDirty)
        {
            _markDirty = markDirty;
            broker.Subscribe<SkillAcquiredMessage>(OnSkillChanged);
            broker.Subscribe<SkillUpgradedMessage>(OnSkillChanged);
            broker.Subscribe<SkillEquippedMessage>(OnSkillChanged);
        }

        /// <inheritdoc />
        public void UnsubscribeDirtyEvents(IMessageBrokerService broker)
        {
            broker.Unsubscribe<SkillAcquiredMessage>(this);
            broker.Unsubscribe<SkillUpgradedMessage>(this);
            broker.Unsubscribe<SkillEquippedMessage>(this);
            _markDirty = null;
        }

        private void OnSkillChanged<T>(T message) => _markDirty?.Invoke();
    }
}
