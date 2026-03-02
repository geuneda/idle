using System.Collections.Generic;
using Geuneda.GoogleSheetImporter;
using GeunedaEditor.GoogleSheetImporter;
using IdleRPG.Skill;

namespace IdleRPG.Editor.Importers
{
    /// <summary>
    /// Google Sheets에서 스킬 슬롯 해금 설정을 임포트한다.
    /// 각 행을 <see cref="SkillSlotConfig"/>로 변환하여 <see cref="SkillConfigAsset"/>의 SlotConfigs에 적용한다.
    /// 기존 Entries와 UpgradeConfig 데이터를 보존한다.
    /// </summary>
    public class SkillSlotConfigImporter : GoogleSheetScriptableObjectImportContainer<SkillConfigAsset>
    {
        /// <inheritdoc />
        public override string GoogleSheetUrl => ConfigImporterHelper.BuildSheetUrl("1289559878");

        /// <inheritdoc />
        protected override void OnImport(SkillConfigAsset asset, List<Dictionary<string, string>> data)
        {
            if (data.Count == 0) return;

            var slotConfigs = new List<SkillSlotConfig>();
            foreach (var row in data)
            {
                var slot = CsvParser.DeserializeTo<SkillSlotConfig>(row);
                slotConfigs.Add(slot);
            }

            var existingEntries = asset.Config.Entries;
            var existingUpgradeConfig = asset.Config.UpgradeConfig;

            var config = new SkillConfigCollection
            {
                Entries = existingEntries ?? new List<SkillEntry>(),
                UpgradeConfig = existingUpgradeConfig ?? new SkillUpgradeConfig(),
                SlotConfigs = slotConfigs
            };
            config.BuildLookup();

            ConfigImporterHelper.SetPrivateField(asset, "_config", config);
        }
    }
}
