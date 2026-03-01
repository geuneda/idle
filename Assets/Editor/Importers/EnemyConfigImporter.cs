using System.Collections.Generic;
using Geuneda.GoogleSheetImporter;
using GeunedaEditor.GoogleSheetImporter;
using IdleRPG.Battle;

namespace IdleRPG.Editor.Importers
{
    /// <summary>
    /// Google Sheets에서 적 설정 목록을 임포트한다.
    /// </summary>
    public class EnemyConfigImporter : GoogleSheetScriptableObjectImportContainer<EnemyConfigsAsset>
    {
        /// <inheritdoc />
        public override string GoogleSheetUrl => ConfigImporterHelper.BuildSheetUrl("213772244");

        /// <inheritdoc />
        protected override void OnImport(EnemyConfigsAsset asset, List<Dictionary<string, string>> data)
        {
            var configs = new List<EnemyConfig>();
            foreach (var row in data)
            {
                configs.Add(CsvParser.DeserializeTo<EnemyConfig>(row));
            }

            ConfigImporterHelper.SetPrivateField(asset, "_configs", configs);
        }
    }
}
