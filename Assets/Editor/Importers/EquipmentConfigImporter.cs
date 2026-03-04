using System.Collections.Generic;
using Geuneda.GoogleSheetImporter;
using GeunedaEditor.GoogleSheetImporter;
using IdleRPG.Core;
using IdleRPG.Equipment;

namespace IdleRPG.Editor.Importers
{
    /// <summary>
    /// Google Sheets에서 장비 설정 데이터를 임포트한다.
    /// 각 행을 <see cref="EquipmentEntry"/>로 변환하여 <see cref="EquipmentConfigAsset"/>에 적용한다.
    /// UpgradeConfig는 Editor에서 수동 관리한다.
    /// </summary>
    public class EquipmentConfigImporter : GoogleSheetScriptableObjectImportContainer<EquipmentConfigAsset>
    {
        /// <inheritdoc />
        public override string GoogleSheetUrl => ConfigImporterHelper.BuildSheetUrl("1963620536");

        /// <inheritdoc />
        protected override void OnImport(EquipmentConfigAsset asset, List<Dictionary<string, string>> data)
        {
            if (data.Count == 0) return;

            var entries = new List<EquipmentEntry>();
            foreach (var row in data)
            {
                ConfigImporterHelper.RemoveEmptyEnumFields(row, "Type", "Grade");
                var entry = CsvParser.DeserializeTo<EquipmentEntry>(row);
                entries.Add(entry);
            }

            var existingUpgradeConfig = asset.Config.UpgradeConfig;

            var config = new EquipmentConfigCollection
            {
                Entries = entries,
                UpgradeConfig = existingUpgradeConfig ?? new UpgradeConfig()
            };
            config.BuildLookup();

            ConfigImporterHelper.SetPrivateField(asset, "_config", config);
        }
    }
}
