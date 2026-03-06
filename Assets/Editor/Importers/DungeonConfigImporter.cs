using System.Collections.Generic;
using Geuneda.GoogleSheetImporter;
using GeunedaEditor.GoogleSheetImporter;
using IdleRPG.Dungeon;

namespace IdleRPG.Editor.Importers
{
    /// <summary>
    /// Google Sheets에서 던전 레벨 설정을 임포트하는 에디터 도구.
    /// </summary>
    public class DungeonConfigImporter : GoogleSheetScriptableObjectImportContainer<DungeonConfigAsset>
    {
        /// <inheritdoc />
        public override string GoogleSheetUrl => ConfigImporterHelper.BuildSheetUrl("1118804460");

        /// <inheritdoc />
        protected override void OnImport(DungeonConfigAsset asset, List<Dictionary<string, string>> data)
        {
            if (data.Count == 0) return;

            var entries = new List<DungeonLevelConfig>();
            foreach (var row in data)
            {
                var entry = CsvParser.DeserializeTo<DungeonLevelConfig>(row);
                entries.Add(entry);
            }

            var config = new DungeonConfigCollection
            {
                Entries = entries
            };
            config.BuildLookup();

            ConfigImporterHelper.SetPrivateField(asset, "_config", config);
        }
    }
}
