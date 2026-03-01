using System.Collections.Generic;
using Geuneda.GoogleSheetImporter;
using GeunedaEditor.GoogleSheetImporter;
using IdleRPG.Hero;

namespace IdleRPG.Editor.Importers
{
    /// <summary>
    /// Google Sheets에서 영웅 기본 스탯 설정을 임포트한다.
    /// </summary>
    public class HeroConfigImporter : GoogleSheetScriptableObjectImportContainer<HeroConfigAsset>
    {
        /// <inheritdoc />
        public override string GoogleSheetUrl => ConfigImporterHelper.BuildSheetUrl("1782579787");

        /// <inheritdoc />
        protected override void OnImport(HeroConfigAsset asset, List<Dictionary<string, string>> data)
        {
            if (data.Count == 0) return;

            var config = CsvParser.DeserializeTo<HeroConfig>(data[0]);
            ConfigImporterHelper.SetPrivateField(asset, "_config", config);
        }
    }
}
