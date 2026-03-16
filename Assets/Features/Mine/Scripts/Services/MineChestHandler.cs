using System;
using IdleRPG.Core;

namespace IdleRPG.Mine
{
    /// <summary>
    /// 보물상자 강화 로직을 담당한다.
    /// </summary>
    internal class MineChestHandler
    {
        private readonly Random _random;
        private readonly float _upgradeChance;
        private readonly int _upgradeSlots;

        /// <summary>
        /// <see cref="MineChestHandler"/>를 생성한다.
        /// </summary>
        /// <param name="random">공유 난수 생성기</param>
        /// <param name="settings">광산 설정</param>
        internal MineChestHandler(Random random, MineSettingsConfig settings)
        {
            _random = random;
            _upgradeChance = settings.ChestUpgradeChance;
            _upgradeSlots = settings.ChestUpgradeSlots;
        }

        /// <summary>강화 슬롯 수</summary>
        internal int UpgradeSlots => _upgradeSlots;

        /// <summary>보물상자 현재 등급을 반환한다.</summary>
        /// <param name="model">광산 모델</param>
        /// <returns>현재 등급</returns>
        internal ItemGrade GetCurrentGrade(MineModel model) => (ItemGrade)model.ChestGrade;

        /// <summary>
        /// 보물상자 강화 1회를 수행한다.
        /// </summary>
        /// <param name="model">광산 모델</param>
        /// <param name="slotIndex">강화 슬롯 인덱스</param>
        /// <returns>강화 성공 여부</returns>
        internal bool PerformUpgrade(MineModel model, int slotIndex)
        {
            bool success = _random.NextDouble() < _upgradeChance;
            if (success)
            {
                model.SetChestGrade(Math.Min(model.ChestGrade + 1, (int)ItemGrade.Myth));
            }
            model.SetChestUpgradeResult(slotIndex, success);
            model.SetChestUpgradesDone(model.ChestUpgradesDone + 1);
            if (model.ChestUpgradesDone >= _upgradeSlots)
            {
                model.SetChestCompleted(true);
            }
            return success;
        }
    }
}
