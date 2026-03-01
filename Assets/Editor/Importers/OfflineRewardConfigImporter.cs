using System;
using System.Collections.Generic;
using Geuneda.GoogleSheetImporter;
using GeunedaEditor.GoogleSheetImporter;
using IdleRPG.OfflineReward;

namespace IdleRPG.Editor.Importers
{
    /// <summary>
    /// Google Sheets에서 오프라인 보상 기본 설정을 임포트한다.
    /// 기존 DropTables 데이터를 보존하며 기본 설정(골드 배율, 최대 시간 등)만 갱신한다.
    /// </summary>
    public class OfflineRewardConfigImporter : GoogleSheetScriptableObjectImportContainer<OfflineRewardConfigAsset>
    {
        /// <inheritdoc />
        public override string GoogleSheetUrl => ConfigImporterHelper.BuildSheetUrl("660414996");

        /// <inheritdoc />
        protected override void OnImport(OfflineRewardConfigAsset asset, List<Dictionary<string, string>> data)
        {
            if (data.Count == 0) return;

            var row = CsvParser.DeserializeTo<OfflineRewardConfigRow>(data[0]);

            var existingDropTables = asset.Config.DropTables;

            var config = new OfflineRewardConfig
            {
                GoldPerMinuteMultiplier = row.GoldPerMinuteMultiplier,
                DefaultMaxOfflineHours = row.DefaultMaxOfflineHours,
                MinOfflineMinutes = row.MinOfflineMinutes,
                AdMultiplier = row.AdMultiplier,
                DropTables = existingDropTables ?? new List<OfflineDropTableEntry>()
            };

            ConfigImporterHelper.SetPrivateField(asset, "_config", config);
        }

        /// <summary>
        /// CSV 역직렬화용 플랫 DTO. OfflineRewardConfig의 기본 설정 필드만 포함한다.
        /// DropTables 등 복합 필드를 제외하여 CsvParser 호환성을 보장한다.
        /// </summary>
        [Serializable]
        internal class OfflineRewardConfigRow
        {
            public float GoldPerMinuteMultiplier;
            public float DefaultMaxOfflineHours;
            public float MinOfflineMinutes;
            public float AdMultiplier;
        }
    }
}
