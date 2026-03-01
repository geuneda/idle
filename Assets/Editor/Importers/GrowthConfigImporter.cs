using System.Collections.Generic;
using Geuneda.GoogleSheetImporter;
using GeunedaEditor.GoogleSheetImporter;
using IdleRPG.Growth;

namespace IdleRPG.Editor.Importers
{
    /// <summary>
    /// Google Sheets에서 영웅 성장 설정(스탯별 성장 곡선)을 임포트한다.
    /// </summary>
    public class GrowthConfigImporter : GoogleSheetScriptableObjectImportContainer<GrowthConfigAsset>
    {
        /// <inheritdoc />
        public override string GoogleSheetUrl => ConfigImporterHelper.BuildSheetUrl("175023291");

        /// <inheritdoc />
        protected override void OnImport(GrowthConfigAsset asset, List<Dictionary<string, string>> data)
        {
            var entries = new List<StatGrowthEntry>();
            foreach (var row in data)
            {
                ConfigImporterHelper.RemoveEmptyEnumFields(row, "PrerequisiteStat");
                entries.Add(CsvParser.DeserializeTo<StatGrowthEntry>(row));
            }

            var config = new GrowthConfig { Entries = entries };
            config.BuildLookup();
            ConfigImporterHelper.SetPrivateField(asset, "_config", config);
        }
    }
}
