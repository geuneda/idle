using System;
using System.Collections.Generic;
using Geuneda.GoogleSheetImporter;
using GeunedaEditor.GoogleSheetImporter;
using IdleRPG.OfflineReward;

namespace IdleRPG.Editor.Importers
{
    /// <summary>
    /// Google Sheets에서 오프라인 드롭 테이블을 임포트한다.
    /// 플랫한 행 데이터를 스테이지 범위별로 그룹화하여 중첩 구조로 변환한다.
    /// </summary>
    public class OfflineDropTableImporter : GoogleSheetScriptableObjectImportContainer<OfflineRewardConfigAsset>
    {
        /// <inheritdoc />
        public override string GoogleSheetUrl => ConfigImporterHelper.BuildSheetUrl("203633402");

        /// <inheritdoc />
        protected override void OnImport(OfflineRewardConfigAsset asset, List<Dictionary<string, string>> data)
        {
            var dropTables = BuildDropTables(data);

            var config = asset.Config;
            config.DropTables = dropTables;
            ConfigImporterHelper.SetPrivateField(asset, "_config", config);
        }

        /// <summary>
        /// 플랫한 CSV 행 목록을 스테이지 범위별 드롭 테이블로 그룹화한다.
        /// </summary>
        private static List<OfflineDropTableEntry> BuildDropTables(List<Dictionary<string, string>> data)
        {
            var result = new List<OfflineDropTableEntry>();

            foreach (var row in data)
            {
                if (!row.ContainsKey("MinGlobalStageIndex") ||
                    string.IsNullOrWhiteSpace(row["MinGlobalStageIndex"]))
                {
                    continue;
                }

                ConfigImporterHelper.RemoveEmptyEnumFields(row, "DropType", "Grade");

                var flatRow = CsvParser.DeserializeTo<OfflineDropTableRow>(row);

                var tableEntry = result.Find(e =>
                    e.MinGlobalStageIndex == flatRow.MinGlobalStageIndex &&
                    e.MaxGlobalStageIndex == flatRow.MaxGlobalStageIndex);

                if (tableEntry == null)
                {
                    tableEntry = new OfflineDropTableEntry
                    {
                        MinGlobalStageIndex = flatRow.MinGlobalStageIndex,
                        MaxGlobalStageIndex = flatRow.MaxGlobalStageIndex
                    };
                    result.Add(tableEntry);
                }

                tableEntry.DropEntries.Add(new OfflineDropEntry
                {
                    DropType = flatRow.DropType,
                    Grade = flatRow.Grade,
                    DropChancePerMinute = flatRow.DropChancePerMinute
                });
            }

            return result;
        }

        /// <summary>
        /// CSV 역직렬화용 플랫 DTO. 스테이지 범위 + 드롭 엔트리를 한 행으로 표현한다.
        /// </summary>
        [Serializable]
        internal class OfflineDropTableRow
        {
            public int MinGlobalStageIndex;
            public int MaxGlobalStageIndex;
            public OfflineDropType DropType;
            public ItemGrade Grade;
            public float DropChancePerMinute;
        }
    }
}
