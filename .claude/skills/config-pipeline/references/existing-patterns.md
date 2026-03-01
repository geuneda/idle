# Config Pipeline 기존 패턴 레퍼런스

## 프로젝트 정보

- Google Sheets 스프레드시트 ID: `1f1kK8ul7E8Y-B_Ipr0MQDZ1RqzmbL9tVnHLoFZsel2s`
- ConfigImporterHelper 위치: `Assets/Editor/Importers/ConfigImporterHelper.cs`
- Importers asmdef: `Assets/Editor/Importers/IdleRPG.Importers.asmdef`

## 1. Config POCO 클래스 패턴

위치: `Assets/Features/{Feature}/Scripts/Configs/{ConfigName}.cs`

```csharp
using System;
// 필요 시 using System.Collections.Generic;

namespace IdleRPG.{Feature}
{
    /// <summary>{Config 설명}</summary>
    [Serializable]
    public class {ConfigName}
    {
        /// <summary>필드 설명</summary>
        public {Type} {FieldName} = {DefaultValue};
    }
}
```

핵심 규칙:
- `[Serializable]` 어트리뷰트 필수
- `class` 사용 (struct 아님 - CsvParser.DeserializeTo<T>가 Activator.CreateInstance 사용)
- 모든 필드는 `public` (CsvParser 리플렉션이 public 필드만 처리)
- 기본값 할당 필수
- 비즈니스 로직 메서드 포함 가능 (CalculateX 등)

## 2. ConfigAsset ScriptableObject 래퍼 패턴

위치: `Assets/Features/{Feature}/Scripts/Configs/{ConfigName}Asset.cs`

### 단일 Config 래퍼

```csharp
using UnityEngine;

namespace IdleRPG.{Feature}
{
    /// <summary>{Config} ScriptableObject 래퍼</summary>
    [CreateAssetMenu(fileName = "{ConfigName}Asset", menuName = "IdleRPG/Configs/{Display Name}")]
    public class {ConfigName}Asset : ScriptableObject
    {
        /// <summary>설정 데이터</summary>
        [SerializeField] private {ConfigName} _config = new {ConfigName}();

        /// <summary>직렬화된 설정 데이터를 반환한다.</summary>
        public {ConfigName} Config => _config;
    }
}
```

### 다중 Config 래퍼 (List)

```csharp
using System.Collections.Generic;
using UnityEngine;

namespace IdleRPG.{Feature}
{
    /// <summary>{Config} 목록 ScriptableObject 래퍼</summary>
    [CreateAssetMenu(fileName = "{ConfigName}sAsset", menuName = "IdleRPG/Configs/{Display Name}")]
    public class {ConfigName}sAsset : ScriptableObject
    {
        /// <summary>설정 목록</summary>
        [SerializeField] private List<{ConfigName}> _configs = new List<{ConfigName}>();

        /// <summary>직렬화된 설정 목록을 반환한다.</summary>
        public IList<{ConfigName}> Configs => _configs;
    }
}
```

### ISerializationCallbackReceiver 패턴 (역직렬화 후 처리 필요 시)

GrowthConfigAsset처럼 역직렬화 후 룩업 딕셔너리를 빌드해야 하는 경우:

```csharp
public class {ConfigName}Asset : ScriptableObject, ISerializationCallbackReceiver
{
    [SerializeField] private {ConfigName} _config = new {ConfigName}();
    public {ConfigName} Config => _config;

    public void OnBeforeSerialize() { }
    public void OnAfterDeserialize()
    {
        _config.BuildLookup(); // 또는 기타 초기화 로직
    }
}
```

## 3. Google Sheets Importer 패턴

위치: `Assets/Editor/Importers/{ConfigName}Importer.cs`

### 단일 행 Config (Hero, Stage, Reward)

```csharp
using System.Collections.Generic;
using Geuneda.GoogleSheetImporter;
using GeunedaEditor.GoogleSheetImporter;
using IdleRPG.{Feature};

namespace IdleRPG.Editor.Importers
{
    /// <summary>Google Sheets에서 {Config} 설정을 임포트한다.</summary>
    public class {ConfigName}Importer : GoogleSheetScriptableObjectImportContainer<{ConfigName}Asset>
    {
        /// <inheritdoc />
        public override string GoogleSheetUrl => ConfigImporterHelper.BuildSheetUrl("{gid}");

        /// <inheritdoc />
        protected override void OnImport({ConfigName}Asset asset, List<Dictionary<string, string>> data)
        {
            if (data.Count == 0) return;

            var config = CsvParser.DeserializeTo<{ConfigName}>(data[0]);
            ConfigImporterHelper.SetPrivateField(asset, "_config", config);
        }
    }
}
```

### 다중 행 Config (Enemy)

```csharp
protected override void OnImport({ConfigName}sAsset asset, List<Dictionary<string, string>> data)
{
    var configs = new List<{ConfigName}>();
    foreach (var row in data)
    {
        configs.Add(CsvParser.DeserializeTo<{ConfigName}>(row));
    }

    ConfigImporterHelper.SetPrivateField(asset, "_configs", configs);
}
```

### enum 빈 값 처리 (선택적 enum 필드가 있는 경우)

CsvParser.Parse가 빈 문자열에 대해 Enum.Parse를 먼저 호출하여 실패하므로,
빈 값인 enum 필드의 키를 딕셔너리에서 제거하여 기본값을 유지한다.

```csharp
foreach (var row in data)
{
    ConfigImporterHelper.RemoveEmptyEnumFields(row, "OptionalEnumField1", "OptionalEnumField2");
    entries.Add(CsvParser.DeserializeTo<{EntryType}>(row));
}
```

## 4. GameInstaller 등록 패턴

위치: `Assets/Core/Scripts/Bootstrap/GameInstaller.cs`

### Addressables 비동기 로드 (LoadConfigAssetsAsync)

```csharp
var op = Addressables.LoadAssetAsync<{ConfigName}Asset>("{ConfigName}Asset");
await op.Task;
_{configName}Asset = op.Result;
```

### ConfigsProvider 초기화 (InitializeConfigsProvider)

단일 Config 등록:
```csharp
provider.AddSingletonConfig(_{configName}Asset.Config);
```

다중 Config 등록 (Id 기반 조회):
```csharp
provider.AddConfigs(config => config.Id, _{configName}sAsset.Configs);
```

### 서비스에서 Config 사용 (InitializeGameServices)

```csharp
var config = configsProvider.GetConfig<{ConfigName}>();       // 단일
var config = configsProvider.GetConfig<{ConfigName}>(id);     // Id로 조회
```

## 5. Google Sheets 탭 구조

### 단일 행 Config 시트 (가로 레이아웃)
```
| Field1 | Field2 | Field3 |
|--------|--------|--------|
| value1 | value2 | value3 |
```

### 다중 행 Config 시트 (ID별)
```
| Id  | Field1 | Field2 |
|-----|--------|--------|
| 1   | val1a  | val2a  |
| 2   | val1b  | val2b  |
```

CSV 컬럼 헤더 = Config POCO 클래스의 public 필드 이름 (정확히 일치해야 함).

## 6. Addressables 등록

- Asset 파일 위치: `Assets/_Project/Configs/{ConfigName}Asset.asset`
- Addressables 그룹: `Configs`
- Address 이름: `{ConfigName}Asset`

## 7. asmdef 참조

`Assets/Editor/Importers/IdleRPG.Importers.asmdef`에 새 Feature asmdef 참조 추가:

```json
{
    "references": [
        "IdleRPG.Core",
        "IdleRPG.Hero",
        "IdleRPG.Battle",
        "IdleRPG.Stage",
        "IdleRPG.Growth",
        "IdleRPG.Reward",
        "IdleRPG.{NewFeature}",
        "Geuneda.GoogleSheetImporter",
        "Geuneda.GoogleSheetImporter.Editor",
        "Geuneda.GameData"
    ]
}
```

## 8. 기존 Config 목록

| Config | 타입 | Asset | gid | Feature |
|--------|------|-------|-----|---------|
| HeroConfig | 단일 | HeroConfigAsset | 1782579787 | Hero |
| EnemyConfig | 다중(Id) | EnemyConfigsAsset | 213772244 | Battle |
| StageConfig | 단일 | StageConfigAsset | 1549889204 | Stage |
| GrowthConfig | 서브리스트 | GrowthConfigAsset | 175023291 | Growth |
| RewardConfig | 단일 | RewardConfigAsset | 1998395860 | Reward |
