---
name: config-pipeline
description: Google Sheets에서 Unity ScriptableObject까지의 Config 데이터 파이프라인. TRIGGER when: 새로운 Config POCO 클래스 생성, ConfigAsset ScriptableObject 래퍼 작성, Importer 클래스 작성, ConfigsProvider에 Config 등록, GameInstaller에서 Config 로드 코드 추가, Google Sheets 임포트 관련 작업 시. 새 Feature의 설정 데이터 파이프라인 구축 시 반드시 invoke한다.
---

# Config Pipeline

## Overview

Google Sheets 스프레드시트의 게임 설정 데이터를 Unity ScriptableObject로 변환하는 전체 파이프라인을 구현한다. Config POCO 클래스 정의부터 Google Sheets Importer 작성, Addressables 등록, ConfigsProvider를 통한 런타임 접근까지 7단계 워크플로우를 따른다.

## 활성화 시점

- 새로운 Config(설정 데이터) 추가 요청
- Google Sheets에서 데이터를 임포트하는 코드 작성
- ConfigAsset ScriptableObject 래퍼 작성
- Importer 클래스 작성
- GameInstaller에 Config 등록
- 기존 Config 수정 또는 필드 추가

## Pipeline 워크플로우 (7단계)

새로운 Config를 추가할 때 아래 순서를 따른다. 기존 패턴의 코드 예제는 `references/existing-patterns.md`를 참조한다.

### Step 1: Config POCO 클래스 생성

위치: `Assets/Features/{Feature}/Scripts/Configs/{ConfigName}.cs`

필수 규칙:
- `[Serializable]` 어트리뷰트 필수 (CsvParser.DeserializeTo가 Activator.CreateInstance 사용)
- `class` 사용 (struct 아님)
- 모든 필드는 `public` (CsvParser 리플렉션이 public 필드만 처리)
- 기본값 할당 필수
- 필드 이름 = Google Sheets CSV 컬럼 헤더 (정확히 일치)
- 비즈니스 로직 메서드 포함 가능

### Step 2: ConfigAsset ScriptableObject 래퍼 생성

위치: `Assets/Features/{Feature}/Scripts/Configs/{ConfigName}Asset.cs`

Config 타입에 따라 래퍼 패턴을 선택한다:
- **단일 Config**: `_config` 필드 + `Config` 프로퍼티
- **다중 Config (List)**: `_configs` 리스트 필드 + `Configs` 프로퍼티 (IList<T> 반환)
- **역직렬화 후 처리 필요**: `ISerializationCallbackReceiver` 구현 (BuildLookup 등)

### Step 3: Google Sheets 탭 준비

스프레드시트 ID: `1f1kK8ul7E8Y-B_Ipr0MQDZ1RqzmbL9tVnHLoFZsel2s`

시트 레이아웃:
- **단일 행 Config**: 첫 행 = 필드명 헤더, 둘째 행 = 값 (가로 레이아웃)
- **다중 행 Config**: 첫 행 = 필드명 헤더, 이후 행 = 각 항목 (Id 포함)

탭 생성 후 gid를 확인한다 (URL의 `#gid=` 파라미터).

### Step 4: Importer 클래스 작성

위치: `Assets/Editor/Importers/{ConfigName}Importer.cs`

`GoogleSheetScriptableObjectImportContainer<TAsset>`를 상속하여 작성한다.

핵심 패턴:
- `GoogleSheetUrl`: `ConfigImporterHelper.BuildSheetUrl("{gid}")` 사용
- `OnImport`: `CsvParser.DeserializeTo<T>(row)`로 역직렬화
- `ConfigImporterHelper.SetPrivateField(asset, "_fieldName", value)`로 private 필드 설정

enum 필드가 빈 값일 수 있는 경우:
- `ConfigImporterHelper.RemoveEmptyEnumFields(row, "FieldName1", "FieldName2")` 호출 후 DeserializeTo 실행
- CsvParser.Parse가 Enum.Parse를 커스텀 역직렬화기보다 먼저 호출하므로, 빈 enum 키를 딕셔너리에서 제거하여 기본값을 유지

### Step 5: asmdef 참조 추가

`Assets/Editor/Importers/IdleRPG.Importers.asmdef`에 새 Feature의 asmdef 참조를 추가한다.

### Step 6: ScriptableObject Asset 생성 및 Addressables 등록

1. Unity Editor에서 Asset 파일 생성: `Assets/_Project/Configs/{ConfigName}Asset.asset`
2. Addressables 그룹 `Configs`에 등록, Address: `{ConfigName}Asset`
3. Google Sheets Import 실행하여 데이터 채우기

### Step 7: GameInstaller 등록

위치: `Assets/Core/Scripts/Bootstrap/GameInstaller.cs`

3곳에 코드를 추가한다:
1. **필드 선언**: `private {ConfigName}Asset _{configName}Asset;`
2. **LoadConfigAssetsAsync**: Addressables.LoadAssetAsync로 로드
3. **InitializeConfigsProvider**:
   - 단일: `provider.AddSingletonConfig(_{configName}Asset.Config);`
   - 다중: `provider.AddConfigs(config => config.Id, _{configName}sAsset.Configs);`

## 체크리스트

Config 추가 완료 전 확인:
- [ ] Config POCO: `[Serializable]`, `class`, public 필드, 기본값
- [ ] ConfigAsset: `[SerializeField] private`, `[CreateAssetMenu]`
- [ ] Google Sheets: 컬럼 헤더 = 필드 이름 정확히 일치
- [ ] Importer: `ConfigImporterHelper.BuildSheetUrl` 사용, gid 정확
- [ ] Importer: enum 빈 값 처리 (RemoveEmptyEnumFields)
- [ ] asmdef: IdleRPG.Importers에 Feature 참조 추가
- [ ] Asset 파일: Addressables `Configs` 그룹에 등록
- [ ] GameInstaller: 로드 + ConfigsProvider 등록

## 주의사항

- CsvParser는 public 필드만 역직렬화한다. private/protected 필드는 무시된다.
- CsvParser는 Enum.Parse를 커스텀 역직렬화기보다 먼저 호출한다. 빈 enum 값은 RemoveEmptyEnumFields로 사전 처리한다.
- ConfigAsset의 `_config`/`_configs` 필드는 private이므로 Importer에서 `SetPrivateField` 리플렉션을 사용한다.
- Google Sheets URL의 gid가 잘못되면 빈 데이터가 반환된다. 탭별 gid를 정확히 확인한다.

## Resources

### references/
- `existing-patterns.md`: 모든 Config 타입별 코드 패턴, 기존 Config 목록, gid 참조
