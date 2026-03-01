using System.Collections.Generic;
using Geuneda.GoogleSheetImporter;
using GeunedaEditor.GoogleSheetImporter;
using IdleRPG.Stage;

namespace IdleRPG.Editor.Importers
{
    /// <summary>
    /// Google Sheets에서 스테이지 구조 설정을 임포트한다.
    /// </summary>
    public class StageConfigImporter : GoogleSheetScriptableObjectImportContainer<StageConfigAsset>
    {
        /// <inheritdoc />
        public override string GoogleSheetUrl => ConfigImporterHelper.BuildSheetUrl("1549889204");

        /// <inheritdoc />
        protected override void OnImport(StageConfigAsset asset, List<Dictionary<string, string>> data)
        {
            if (data.Count == 0) return;

            var config = CsvParser.DeserializeTo<StageConfig>(data[0]);
            ConfigImporterHelper.SetPrivateField(asset, "_config", config);
        }
    }
}
