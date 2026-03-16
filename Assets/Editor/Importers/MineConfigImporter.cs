using System.Collections.Generic;
using Geuneda.GoogleSheetImporter;
using GeunedaEditor.GoogleSheetImporter;
using IdleRPG.Core;
using IdleRPG.Mine;

namespace IdleRPG.Editor.Importers
{
    /// <summary>
    /// Google Sheets에서 광산 블록 가중치 설정을 임포트하는 에디터 도구.
    /// </summary>
    public class MineBlockWeightImporter : GoogleSheetScriptableObjectImportContainer<MineConfigAsset>
    {
        /// <inheritdoc />
        public override string GoogleSheetUrl => ConfigImporterHelper.BuildSheetUrl("1269824441");

        /// <inheritdoc />
        protected override void OnImport(MineConfigAsset asset, List<Dictionary<string, string>> data)
        {
            if (data.Count == 0) return;

            var entries = new List<MineBlockWeightConfig>();
            foreach (var row in data)
            {
                var entry = CsvParser.DeserializeTo<MineBlockWeightConfig>(row);
                entries.Add(entry);
            }

            var config = asset.Config;
            config.BlockWeightEntries = entries;
            config.BuildLookup();

            ConfigImporterHelper.SetPrivateField(asset, "_config", config);
        }
    }

    /// <summary>
    /// Google Sheets에서 광산 상자 보상 설정을 임포트하는 에디터 도구.
    /// </summary>
    public class MineChestRewardImporter : GoogleSheetScriptableObjectImportContainer<MineConfigAsset>
    {
        /// <inheritdoc />
        public override string GoogleSheetUrl => ConfigImporterHelper.BuildSheetUrl("323013103");

        /// <inheritdoc />
        protected override void OnImport(MineConfigAsset asset, List<Dictionary<string, string>> data)
        {
            if (data.Count == 0) return;

            var entries = new List<MineChestRewardConfig>();
            foreach (var row in data)
            {
                ConfigImporterHelper.RemoveEmptyEnumFields(row, "Grade");
                var entry = CsvParser.DeserializeTo<MineChestRewardConfig>(row);
                entries.Add(entry);
            }

            var config = asset.Config;
            config.ChestRewardEntries = entries;
            config.BuildLookup();

            ConfigImporterHelper.SetPrivateField(asset, "_config", config);
        }
    }

    /// <summary>
    /// Google Sheets에서 광산 진행도 보상 설정을 임포트하는 에디터 도구.
    /// </summary>
    public class MineProgressRewardImporter : GoogleSheetScriptableObjectImportContainer<MineConfigAsset>
    {
        /// <inheritdoc />
        public override string GoogleSheetUrl => ConfigImporterHelper.BuildSheetUrl("465038327");

        /// <inheritdoc />
        protected override void OnImport(MineConfigAsset asset, List<Dictionary<string, string>> data)
        {
            if (data.Count == 0) return;

            var entries = new List<MineProgressRewardConfig>();
            foreach (var row in data)
            {
                ConfigImporterHelper.RemoveEmptyEnumFields(row, "RewardType");
                var entry = CsvParser.DeserializeTo<MineProgressRewardConfig>(row);
                entries.Add(entry);
            }

            var config = asset.Config;
            config.ProgressRewardEntries = entries;
            config.BuildLookup();

            ConfigImporterHelper.SetPrivateField(asset, "_config", config);
        }
    }

    /// <summary>
    /// Google Sheets에서 광산 글로벌 설정을 임포트하는 에디터 도구.
    /// </summary>
    public class MineSettingsImporter : GoogleSheetScriptableObjectImportContainer<MineConfigAsset>
    {
        /// <inheritdoc />
        public override string GoogleSheetUrl => ConfigImporterHelper.BuildSheetUrl("2126098338");

        /// <inheritdoc />
        protected override void OnImport(MineConfigAsset asset, List<Dictionary<string, string>> data)
        {
            if (data.Count == 0) return;

            var settings = CsvParser.DeserializeTo<MineSettingsConfig>(data[0]);

            var config = asset.Config;
            config.Settings = settings;

            ConfigImporterHelper.SetPrivateField(asset, "_config", config);
        }
    }
}
