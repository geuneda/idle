using System;
using System.Collections.Generic;
using Geuneda.Services;
using IdleRPG.Dungeon;

namespace IdleRPG.Core
{
    /// <summary>
    /// 던전 진행 상태의 저장/로드를 담당한다.
    /// </summary>
    public class DungeonSaveCollector : ISaveDataCollector
    {
        private readonly DungeonModel _dungeonModel;
        private Action _markDirty;

        /// <summary>
        /// <see cref="DungeonSaveCollector"/>를 생성한다.
        /// </summary>
        /// <param name="dungeonModel">던전 진행 상태 모델</param>
        public DungeonSaveCollector(DungeonModel dungeonModel)
        {
            _dungeonModel = dungeonModel;
        }

        /// <inheritdoc />
        public void ExtractTo(GameSaveData saveData)
        {
            saveData.Dungeon = new DungeonSaveData
            {
                ClearedLevels = new Dictionary<int, int>(_dungeonModel.ClearedLevels),
                DailyUsedEntries = new Dictionary<int, int>(_dungeonModel.DailyUsedEntries),
                LastDailyResetTimestamp = _dungeonModel.LastDailyResetTimestamp
            };
        }

        /// <inheritdoc />
        public void ApplyFrom(GameSaveData saveData)
        {
            var data = saveData.Dungeon;
            if (data == null) return;

            _dungeonModel.ClearedLevels = new Dictionary<int, int>(data.ClearedLevels);
            _dungeonModel.DailyUsedEntries = new Dictionary<int, int>(data.DailyUsedEntries);
            _dungeonModel.LastDailyResetTimestamp = data.LastDailyResetTimestamp;
        }

        /// <inheritdoc />
        public void SubscribeDirtyEvents(IMessageBrokerService broker, Action markDirty)
        {
            _markDirty = markDirty;
            broker.Subscribe<DungeonCompletedMessage>(OnDungeonCompleted);
            broker.Subscribe<DungeonSweptMessage>(OnDungeonSwept);
            broker.Subscribe<DungeonDailyResetMessage>(OnDailyReset);
        }

        /// <inheritdoc />
        public void UnsubscribeDirtyEvents(IMessageBrokerService broker)
        {
            broker.Unsubscribe<DungeonCompletedMessage>(this);
            broker.Unsubscribe<DungeonSweptMessage>(this);
            broker.Unsubscribe<DungeonDailyResetMessage>(this);
            _markDirty = null;
        }

        private void OnDungeonCompleted(DungeonCompletedMessage msg) => _markDirty?.Invoke();
        private void OnDungeonSwept(DungeonSweptMessage msg) => _markDirty?.Invoke();
        private void OnDailyReset(DungeonDailyResetMessage msg) => _markDirty?.Invoke();
    }
}
