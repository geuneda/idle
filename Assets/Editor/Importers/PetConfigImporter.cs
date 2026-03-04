using System.Collections.Generic;
using Geuneda.GoogleSheetImporter;
using GeunedaEditor.GoogleSheetImporter;
using IdleRPG.Pet;

namespace IdleRPG.Editor.Importers
{
    /// <summary>
    /// Google Sheets에서 펫 설정 데이터를 임포트한다.
    /// 각 행을 <see cref="PetEntry"/>로 변환하여 <see cref="PetConfigAsset"/>에 적용한다.
    /// UpgradeConfig와 SlotConfigs는 Editor에서 수동 관리한다.
    /// </summary>
    public class PetConfigImporter : GoogleSheetScriptableObjectImportContainer<PetConfigAsset>
    {
        /// <inheritdoc />
        public override string GoogleSheetUrl => ConfigImporterHelper.BuildSheetUrl("1344681108");

        /// <inheritdoc />
        protected override void OnImport(PetConfigAsset asset, List<Dictionary<string, string>> data)
        {
            if (data.Count == 0) return;

            var entries = new List<PetEntry>();
            foreach (var row in data)
            {
                ConfigImporterHelper.RemoveEmptyEnumFields(row, "Grade");
                var entry = CsvParser.DeserializeTo<PetEntry>(row);
                entries.Add(entry);
            }

            var existingUpgradeConfig = asset.Config.UpgradeConfig;
            var existingSlotConfigs = asset.Config.SlotConfigs;

            var config = new PetConfigCollection
            {
                Entries = entries,
                UpgradeConfig = existingUpgradeConfig ?? new PetUpgradeConfig(),
                SlotConfigs = existingSlotConfigs ?? new List<PetSlotConfig>()
            };
            config.BuildLookup();

            ConfigImporterHelper.SetPrivateField(asset, "_config", config);
        }
    }
}
