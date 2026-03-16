# Idle RPG

Unity 6.3 (6000.3.11f1) + URP 17.3.0 방치형 RPG 모바일 2D 게임. C# 9.0, 명시적 네임스페이스.

## 금지 패턴

- 싱글톤 금지 (MainInstaller 서비스 로케이터 사용)
- static mutable state 금지 (서비스를 통해 접근)
- 2단계 이상 MonoBehaviour 상속 금지
- MonoBehaviour에 비즈니스 로직 직접 구현 금지
- God class 금지 (단일 책임 원칙)
- Debug.Log/Debug.LogWarning 직접 호출 금지 (`DevLog` 사용, `Core/Scripts/Utils/DevLog.cs` 참조)
- Resources 폴더 사용 최소화 (Addressables 사용)

## CBD 설계 원칙

- MonoBehaviour는 단일 책임 (Unity 연결 역할만)
- 비즈니스 로직은 POCO 클래스로 분리
- 상속 대신 컴포넌트 조합
- 인터페이스 기반 추상화, 구체 타입 직접 참조 최소화

### 권장 구조

- Model: POCO + ObservableField 상태 관리
- View: MonoBehaviour, Unity 표시만
- Presenter: UiPresenter<T> 상속, View와 Model 연결
- Config: ScriptableObject 또는 ConfigsProvider
- Service: MainInstaller 바인딩, 인터페이스 접근

## 프레임워크 스킬 필수 규칙

Geuneda API 코드 작성/수정 전 반드시 해당 스킬을 invoke하여 API 사양 확인.

| using 네임스페이스            | 필수 스킬                      |
| ----------------------------- | ------------------------------ |
| `Geuneda.DataExtensions`      | `/geuneda-gamedata`            |
| `Geuneda.Services`            | `/geuneda-services`            |
| `Geuneda.UiService`           | `/geuneda-uiservice`           |
| `Geuneda.StatechartMachine`   | `/geuneda-statechart`          |
| `Geuneda.AssetsImporter`      | `/geuneda-assetsimporter`      |
| `Geuneda.GoogleSheetImporter` | `/geuneda-googlesheetimporter` |
| `Geuneda.InputExtensions`     | `/geuneda-inputextensions`     |
| `Geuneda.NativeUi`            | `/geuneda-nativeui`            |
| `Geuneda.NotificationService` | `/geuneda-notificationservice` |

| 작업 유형                        | 필수 스킬             |
| -------------------------------- | --------------------- |
| Config/Importer/ConfigAsset 생성 | `/config-pipeline`    |
| 프레임워크 구조/의존성 변경      | `/geuneda-frameworks` |

- 하나의 파일이 여러 네임스페이스 사용 시 관련 스킬 모두 invoke
- 에이전트 위임 시 스킬 invoke를 명시

## 핵심 디렉토리

| 경로                             | 용도                                   |
| -------------------------------- | -------------------------------------- |
| `Core/Scripts/Bootstrap/`        | MainInstaller, 게임 진입점             |
| `Core/Scripts/SaveSystem/`       | 저장/로드 시스템                       |
| `Features/{Feature}/Scripts/`    | 비즈니스 로직 (Service, Model, Config) |
| `UI/{Common,HUD,Popup}/Scripts/` | UI Presenter, View                     |
| `_Project/Configs/`              | ConfigsProvider 설정 데이터            |
| `Editor/Importers/`              | Google Sheets Importer                 |

## 문서화 규칙

- 모든 public/protected 멤버에 `/// <summary>` XML 주석 작성 (한국어)
- `<param>`, `<returns>`, `<see cref=""/>` 활용
- 코드를 반복하는 무의미한 주석 금지

## 작업 검증

- 코드 변경 후 반드시 Unity MCP 리컴파일 실행
- 컴파일 에러 시 즉시 수정 후 재컴파일
- IDE 린트가 아닌 Unity 컴파일러 빌드 결과 기준

## 프로젝트 고유 규칙

- 시간 계산은 TimeService 기준
- 재화/아이템 수량 변경은 CommandService 커맨드 패턴으로 실행
- 밸런스 데이터는 Google Sheets -> ConfigsProvider 파이프라인
- UI는 Addressables 비동기 로딩
- 전투 결정론이 필요하면 floatP 사용
