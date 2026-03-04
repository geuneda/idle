using System;
using Geuneda.Services;
using IdleRPG.Pet;

namespace IdleRPG.Core
{
    /// <summary>
    /// 펫 보유/장착 상태의 저장/로드를 담당한다.
    /// </summary>
    public class PetSaveCollector : ISaveDataCollector
    {
        private readonly PetModel _petModel;
        private Action _markDirty;

        public PetSaveCollector(PetModel petModel)
        {
            _petModel = petModel;
        }

        /// <inheritdoc />
        public void ExtractTo(GameSaveData saveData)
        {
            var data = new PetSaveData();
            foreach (var item in _petModel.GetAllItems())
            {
                if (item.OwnedCount.Value <= 0 && item.Level.Value <= 0) continue;

                data.Items[item.Id] = new PetItemSaveEntry
                {
                    OwnedCount = item.OwnedCount.Value,
                    Level = item.Level.Value
                };
            }

            for (int i = 0; i < PetModel.MAX_SLOTS; i++)
            {
                int petId = _petModel.GetEquippedPetId(i);
                if (petId != PetModel.UNEQUIPPED)
                {
                    data.Equipped[i] = petId;
                }
            }

            saveData.Pet = data;
        }

        /// <inheritdoc />
        public void ApplyFrom(GameSaveData saveData)
        {
            var data = saveData.Pet;
            if (data == null) return;

            foreach (var pair in data.Items)
            {
                var state = _petModel.GetOrCreateItemState(pair.Key);
                state.OwnedCount.Value = pair.Value.OwnedCount;
                state.Level.Value = pair.Value.Level;
            }

            foreach (var pair in data.Equipped)
            {
                if (pair.Key < 0 || pair.Key >= PetModel.MAX_SLOTS) continue;
                _petModel.Equip(pair.Key, pair.Value);
            }
        }

        /// <inheritdoc />
        public void SubscribeDirtyEvents(IMessageBrokerService broker, Action markDirty)
        {
            _markDirty = markDirty;
            broker.Subscribe<PetAcquiredMessage>(OnPetChanged);
            broker.Subscribe<PetUpgradedMessage>(OnPetChanged);
            broker.Subscribe<PetEquippedMessage>(OnPetChanged);
        }

        /// <inheritdoc />
        public void UnsubscribeDirtyEvents(IMessageBrokerService broker)
        {
            broker.Unsubscribe<PetAcquiredMessage>(this);
            broker.Unsubscribe<PetUpgradedMessage>(this);
            broker.Unsubscribe<PetEquippedMessage>(this);
            _markDirty = null;
        }

        private void OnPetChanged<T>(T message) => _markDirty?.Invoke();
    }
}
