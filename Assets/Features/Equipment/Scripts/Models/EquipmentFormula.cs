using IdleRPG.Core;

namespace IdleRPG.Equipment
{
    /// <summary>
    /// 장비 관련 순수 함수 모음. 보유효과, 장착효과, 강화 비용 등을 계산한다.
    /// </summary>
    public static class EquipmentFormula
    {
        /// <summary>
        /// 지정 레벨에서의 보유효과 수치를 계산한다.
        /// 공식: BasePossessionEffect + PossessionEffectPerLevel * (level - 1)
        /// </summary>
        /// <param name="entry">장비 설정 항목</param>
        /// <param name="level">현재 레벨 (1 이상)</param>
        /// <returns>보유효과 수치 (%)</returns>
        public static BigNumber CalcPossessionEffect(EquipmentEntry entry, int level)
        {
            return CollectibleFormula.CalcPossessionEffect(entry, level);
        }

        /// <summary>
        /// 지정 레벨에서의 장착효과 수치를 계산한다.
        /// 공식: BaseEquipEffect + EquipEffectPerLevel * (level - 1)
        /// </summary>
        /// <param name="entry">장비 설정 항목</param>
        /// <param name="level">현재 레벨 (1 이상)</param>
        /// <returns>장착효과 수치 (%)</returns>
        public static BigNumber CalcEquipEffect(EquipmentEntry entry, int level)
        {
            if (level <= 0) return BigNumber.Zero;

            double value = entry.BaseEquipEffect + entry.EquipEffectPerLevel * (level - 1);
            return new BigNumber(value, 0);
        }

        /// <summary>
        /// 현재 레벨에서 다음 레벨로 강화하는 데 필요한 같은 장비 개수를 반환한다.
        /// </summary>
        /// <param name="config">강화 비용 설정</param>
        /// <param name="currentLevel">현재 레벨</param>
        /// <returns>필요한 소재 개수</returns>
        public static int GetRequiredCount(UpgradeConfig config, int currentLevel)
        {
            return CollectibleFormula.GetRequiredCount(config, currentLevel);
        }

        /// <summary>
        /// 지정 레벨이 최대 레벨인지 확인한다.
        /// </summary>
        /// <param name="entry">장비 설정 항목</param>
        /// <param name="currentLevel">현재 레벨</param>
        /// <returns>최대 레벨이면 true</returns>
        public static bool IsMaxLevel(EquipmentEntry entry, int currentLevel)
        {
            return CollectibleFormula.IsMaxLevel(entry, currentLevel);
        }

        /// <summary>
        /// 장착효과가 가장 높은 해금된 장비를 찾는다.
        /// </summary>
        /// <param name="config">장비 설정 컬렉션</param>
        /// <param name="model">장비 모델</param>
        /// <param name="type">장비 슬롯 타입</param>
        /// <returns>최적 장비 ID. 해금된 장비가 없으면 null</returns>
        public static int? FindBestEquipmentId(
            EquipmentConfigCollection config,
            EquipmentModel model,
            EquipmentType type)
        {
            var entries = config.GetEntriesByType(type);
            BigNumber bestEffect = BigNumber.Zero;
            int? bestId = null;

            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                var state = model.GetItemState(entry.Id);
                if (state == null || !state.IsUnlocked) continue;

                var effect = CalcEquipEffect(entry, state.Level.Value);
                if (effect > bestEffect)
                {
                    bestEffect = effect;
                    bestId = entry.Id;
                }
            }

            return bestId;
        }
    }
}
