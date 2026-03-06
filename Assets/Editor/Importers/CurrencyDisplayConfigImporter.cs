using System.Collections.Generic;
using Geuneda.GoogleSheetImporter;
using GeunedaEditor.GoogleSheetImporter;
using IdleRPG.Economy;

namespace IdleRPG.Editor.Importers
{
    /// <summary>
    /// Google Sheets에서 재화 표시 설정을 임포트하는 에디터 도구.
    /// </summary>
    public class CurrencyDisplayConfigImporter : GoogleSheetScriptableObjectImportContainer<CurrencyDisplayConfigAsset>
    {
        /// <inheritdoc />
        public override string GoogleSheetUrl => ConfigImporterHelper.BuildSheetUrl("1784453796");

        /// <inheritdoc />
        protected override void OnImport(CurrencyDisplayConfigAsset asset, List<Dictionary<string, string>> data)
        {
            if (data.Count == 0) return;

            var entries = new List<CurrencyDisplayConfig>();
            foreach (var row in data)
            {
                var entry = CsvParser.DeserializeTo<CurrencyDisplayConfig>(row);
                entries.Add(entry);
            }

            var config = new CurrencyDisplayConfigCollection
            {
                Entries = entries
            };
            config.BuildLookup();

            ConfigImporterHelper.SetPrivateField(asset, "_config", config);
        }
    }
}
