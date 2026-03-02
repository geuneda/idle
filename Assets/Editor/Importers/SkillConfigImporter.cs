using System.Collections.Generic;
using Geuneda.GoogleSheetImporter;
using GeunedaEditor.GoogleSheetImporter;
using IdleRPG.Skill;

namespace IdleRPG.Editor.Importers
{
    /// <summary>
    /// Google Sheets에서 스킬 설정 데이터를 임포트한다.
    /// 각 행을 <see cref="SkillEntry"/>로 변환하여 <see cref="SkillConfigAsset"/>에 적용한다.
    /// UpgradeConfig는 Editor에서 수동 관리한다.
    /// </summary>
    public class SkillConfigImporter : GoogleSheetScriptableObjectImportContainer<SkillConfigAsset>
    {
        /// <inheritdoc />
        public override string GoogleSheetUrl => ConfigImporterHelper.BuildSheetUrl("1940140305");

        /// <inheritdoc />
        protected override void OnImport(SkillConfigAsset asset, List<Dictionary<string, string>> data)
        {
            if (data.Count == 0) return;

            var entries = new List<SkillEntry>();
            foreach (var row in data)
            {
                ConfigImporterHelper.RemoveEmptyEnumFields(row, "Grade", "SkillType");
                var entry = CsvParser.DeserializeTo<SkillEntry>(row);
                entries.Add(entry);
            }

            var existingUpgradeConfig = asset.Config.UpgradeConfig;
            var existingSlotConfigs = asset.Config.SlotConfigs;

            var config = new SkillConfigCollection
            {
                Entries = entries,
                UpgradeConfig = existingUpgradeConfig ?? new SkillUpgradeConfig(),
                SlotConfigs = existingSlotConfigs ?? new List<SkillSlotConfig>()
            };
            config.BuildLookup();

            ConfigImporterHelper.SetPrivateField(asset, "_config", config);
        }
    }
}
