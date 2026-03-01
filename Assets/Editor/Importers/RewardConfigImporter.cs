using System.Collections.Generic;
using Geuneda.GoogleSheetImporter;
using GeunedaEditor.GoogleSheetImporter;
using IdleRPG.Reward;

namespace IdleRPG.Editor.Importers
{
    /// <summary>
    /// Google Sheets에서 적 처치 보상 설정을 임포트한다.
    /// </summary>
    public class RewardConfigImporter : GoogleSheetScriptableObjectImportContainer<RewardConfigAsset>
    {
        /// <inheritdoc />
        public override string GoogleSheetUrl => ConfigImporterHelper.BuildSheetUrl("1998395860");

        /// <inheritdoc />
        protected override void OnImport(RewardConfigAsset asset, List<Dictionary<string, string>> data)
        {
            if (data.Count == 0) return;

            var config = CsvParser.DeserializeTo<RewardConfig>(data[0]);
            ConfigImporterHelper.SetPrivateField(asset, "_config", config);
        }
    }
}
