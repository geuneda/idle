using System;
using Geuneda.Services;
using IdleRPG.Growth;
using IdleRPG.Hero;

namespace IdleRPG.Core
{
    /// <summary>
    /// 영웅 성장 레벨의 저장/로드를 담당한다.
    /// </summary>
    public class GrowthSaveCollector : ISaveDataCollector
    {
        private static readonly HeroStatType[] CachedStatTypes =
            (HeroStatType[])Enum.GetValues(typeof(HeroStatType));

        private readonly HeroGrowthModel _growthModel;
        private Action _markDirty;

        public GrowthSaveCollector(HeroGrowthModel growthModel)
        {
            _growthModel = growthModel;
        }

        /// <inheritdoc />
        public void ExtractTo(GameSaveData saveData)
        {
            var data = new GrowthSaveData();
            foreach (var type in CachedStatTypes)
            {
                data.StatLevels[(int)type] = _growthModel.GetLevel(type);
            }
            saveData.Growth = data;
        }

        /// <inheritdoc />
        public void ApplyFrom(GameSaveData saveData)
        {
            var data = saveData.Growth;
            if (data == null) return;

            foreach (var pair in data.StatLevels)
            {
                if (!Enum.IsDefined(typeof(HeroStatType), pair.Key)) continue;

                var statType = (HeroStatType)pair.Key;
                _growthModel.SetLevel(statType, pair.Value);
            }
        }

        /// <inheritdoc />
        public void SubscribeDirtyEvents(IMessageBrokerService broker, Action markDirty)
        {
            _markDirty = markDirty;
            broker.Subscribe<StatLevelUpMessage>(OnStatLevelUp);
        }

        /// <inheritdoc />
        public void UnsubscribeDirtyEvents(IMessageBrokerService broker)
        {
            broker.Unsubscribe<StatLevelUpMessage>(this);
            _markDirty = null;
        }

        private void OnStatLevelUp(StatLevelUpMessage message) => _markDirty?.Invoke();
    }
}
