# Idle RPG

Unity 6.3 (6000.3.10f1) + URP 17.3.0 기반 방치형 RPG 모바일 2D 게임.
C# 9.0, 명시적 네임스페이스 사용.

## CBD (Component-Based Design) 원칙

모든 코드 작성 시 컴포넌트 기반 설계를 따른다.

### 필수 규칙
- 깊은 상속 금지. MonoBehaviour 최대 1단계 상속만 허용
- MonoBehaviour는 단일 책임. Unity 연결(입력, 렌더링, 충돌) 역할만 담당
- 비즈니스 로직은 POCO 클래스로 분리
- ScriptableObject로 설정 데이터와 이벤트 채널 분리
- 상속 대신 컴포넌트 조합으로 기능 구성
- 인터페이스 기반 추상화. 구체 타입 직접 참조 최소화

### 금지 패턴
- God class (하나의 클래스가 여러 시스템 관리)
- 싱글톤 패턴 (MainInstaller 서비스 로케이터 사용)
- static mutable state (서비스를 통해 접근)
- 2단계 이상 MonoBehaviour 상속 체인
- MonoBehaviour에 비즈니스 로직 직접 구현

### 권장 구조
- Model: POCO 클래스 + ObservableField로 상태 관리
- View: MonoBehaviour로 Unity 연결, 표시만 담당
- Presenter: UiPresenter<T> 상속, View와 Model 연결
- Config: ScriptableObject 또는 ConfigsProvider로 설정 관리
- Service: MainInstaller에 바인딩, 인터페이스로 접근

## Geuneda Frameworks

설치된 9개 패키지. 각 패키지 API 사용 시 해당 스킬을 참조할 것.

| 패키지 | 용도 |
|--------|------|
| com.geuneda.gamedata | Observable 반응형 데이터, ConfigsProvider, floatP, 직렬화 |
| com.geuneda.services | DI 컨테이너(MainInstaller), 메시지 브로커, 틱/풀/데이터/시간 서비스 |
| com.geuneda.uiservice | MVP 패턴 UI 관리, Addressables 기반 비동기 UI 로딩, 피처 시스템 |
| com.geuneda.statechart | 계층적 상태 머신(HFSM), 비동기 대기, 병렬/조건 분기 |
| com.geuneda.assetsimporter | Addressables 에셋 로딩/언로딩, 씬 관리, 타입별 에셋 설정 |
| com.geuneda.googlesheetimporter | Google Sheets -> ScriptableObject 데이터 임포트 (Editor) |
| com.geuneda.inputextensions | 터치/드래그/스와이프/탭 제스처, 게임패드 입력 |
| com.geuneda.nativeui | iOS/Android 네이티브 다이얼로그, 토스트 메시지 |
| com.geuneda.notificationservice | 모바일 로컬/원격 푸시 알림 예약/관리 |

의존 체인: gamedata -> services -> uiservice

## Skills 참조

.claude/skills/ 에 10개 스킬 등록:

- geuneda-frameworks: 프레임워크 전체 구조, 패키지 간 의존성
- geuneda-dataextensions: ObservableField/List/Dict, ConfigsProvider, floatP
- geuneda-services: MainInstaller, MessageBroker, TickService, PoolService, DataService
- geuneda-uiservice: UiPresenter, UI 생명주기, 피처 시스템, UiSetConfig
- geuneda-statechart: Statechart HFSM, 상태/전이/이벤트, 비동기 대기
- geuneda-assetsimporter: AssetResolverService, Addressables 로딩, 씬 관리
- geuneda-googlesheetimporter: CsvParser, 시트 임포터, ScriptableObject 변환
- geuneda-inputextensions: PointerInputManager, GestureController, GamepadInput
- geuneda-notificationservice: MobileNotificationService, 알림 채널, 예약
- geuneda-nativeui: NativeUiService, AlertButton, 네이티브 다이얼로그

## 폴더 구조

Feature 기반 구조. 비즈니스 로직은 Features/, UI는 Assets/UI/에 집중한다.

### 폴더 역할 기준

| 찾고 싶은 것 | 위치 | 설명 |
|-------------|------|------|
| 비즈니스 로직 | `Features/{Feature}/Scripts/` | Service, Model, Component, Config |
| 게임플레이 프리팹 | `Features/{Feature}/Prefabs/` | 캐릭터, 적, 투사체 등 |
| UI 스크립트 | `UI/{Common,HUD,Popup}/Scripts/` | Presenter, View 스크립트 |
| UI 프리팹 | `UI/{Common,HUD,Popup}/Prefabs/` | UI 화면 프리팹 (Addressables) |
| 공통 프리팹 | `_Project/Prefabs/Common/` | 이펙트, 로딩 등 공유 프리팹 |
| 아트 에셋 | `_Project/Art/{종류}/` | Sprites, Animations, Fonts, UI, VFX |
| 설정 파일 | `_Project/Settings/` | URP, Input System, 렌더러 설정 |
| 씬 | `_Project/Scenes/` | 게임 씬 파일 |
| 데이터 설정 | `_Project/Configs/` | ConfigsProvider, 버전 데이터 등 |

### 디렉토리 트리

```
Assets/
  _Project/                  # 비코드 에셋 (상단 정렬)
    Art/
      Animations/
      Fonts/                 # 폰트 에셋 (NotoSansKR 등)
      Sprites/
      UI/                    # UI 스프라이트, 아이콘
      VFX/
    Audio/
      BGM/
      SFX/
    Configs/                 # 설정 데이터
      Resources/             # 버전 데이터 등 (Resources 로드)
    Prefabs/Common/          # 공통 프리팹 (여러 Feature 공유)
    Scenes/                  # 게임 씬
    Settings/                # URP, 렌더러, Input System 설정

  Core/                      # 공통 기반 코드 (Feature 의존 금지)
    Scripts/
      Bootstrap/             # 게임 진입점, MainInstaller 설정
      Events/                # 공통 이벤트 채널 (ScriptableObject)
      Extensions/            # 확장 메서드
      Interfaces/            # 공통 인터페이스
      SaveSystem/            # 저장/로드 시스템
      Utils/                 # 유틸리티

  Features/                  # 기능별 독립 폴더
    Hero/                    # 캐릭터/영웅 시스템
      Scripts/
        Components/          # MonoBehaviour 컴포넌트
        Configs/             # ScriptableObject 설정
        Models/              # POCO 데이터 모델
        Services/            # Feature 전용 서비스
      Prefabs/               # Feature 전용 프리팹
    Battle/                  # 전투 시스템 (자동 전투)
      Scripts/Components/, Configs/, Models/, Services/
      Prefabs/
    Stage/                   # 스테이지/던전 시스템
    Economy/                 # 재화/경제 시스템
    Equipment/               # 장비 시스템
    Inventory/               # 인벤토리 시스템
    Gacha/                   # 가챠/소환 시스템
    Growth/                  # 성장/강화 시스템
    Quest/                   # 퀘스트 시스템
    Shop/                    # 상점 시스템
    Reward/                  # 보상 시스템
    OfflineReward/           # 오프라인 보상 시스템

  UI/                        # 모든 UI (여러 Feature 공유)
    IdleRPG.UI.asmdef        # UI 전체 커버 asmdef
    Common/Scripts/, Prefabs/ # 공통 UI (탭바, 전투 HUD 등)
    HUD/Scripts/, Prefabs/    # 상단 HUD
    Popup/Scripts/, Prefabs/  # 팝업 UI (영웅, 던전, 소환 탭 등)

  Editor/                    # 에디터 전용 (별도 asmdef)
    Scripts/
    Importers/

  ThirdParty/                # 외부 에셋

  Tests/
    EditMode/
    PlayMode/
```

### 건드리지 않는 외부 폴더

- `TextMesh Pro/` - Unity 자동 생성 (이동 시 재생성 위험)
- `Plugins/Demigiant/` - DOTween 라이브러리 관례 유지
- `Resources/` - DOTween 런타임 설정 전용 (게임 에셋은 Addressables 사용)

## 서비스 초기화 순서

MainInstaller를 통한 등록 순서 (의존성 방향):
1. ConfigsProvider (설정 데이터)
2. DataService (저장/로드)
3. TimeService (시간 관리, 오프라인 보상 계산)
4. MessageBrokerService (이벤트 통신)
5. TickService (업데이트 관리)
6. PoolService (오브젝트 풀링)
7. AssetResolverService (에셋 로딩)
8. UiService (UI 관리)
9. 게임 고유 서비스 (BattleService, StageService 등)

## 코드 문서화 규칙

모든 public/protected 멤버에 XML 문서 주석(`/// <summary>`)을 작성한다.

### 필수 문서화 대상
- 클래스, 구조체, 인터페이스, 열거형 선언
- public/protected 메서드 (매개변수 `<param>`, 반환값 `<returns>` 포함)
- public/protected 프로퍼티, 필드
- 이벤트, 델리게이트

### 작성 규칙
- 한국어로 작성
- `<summary>`는 한 줄로 간결하게. 필요 시 `<remarks>`로 보충
- `<param name="">`, `<returns>`, `<exception cref="">` 적극 활용
- `<see cref=""/>`, `<seealso cref=""/>` 로 관련 타입/메서드 참조
- private 멤버는 로직이 복잡한 경우에만 선택적으로 문서화

### 금지
- 코드를 그대로 반복하는 무의미한 주석 (예: `/// <summary>값을 가져옵니다</summary>` on `GetValue()`)
- 영어/한국어 혼용 (한국어로 통일)

## 작업 검증 규칙

코드 작성/수정 완료 후 반드시 Unity MCP의 리컴파일 기능을 실행하여 컴파일 결과를 확인한다.
IDE 린트 에러가 아닌 Unity 컴파일러의 실제 빌드 결과를 기준으로 오류를 판단한다.

- 코드 변경 후 Unity MCP로 리컴파일 실행
- 컴파일 에러 발생 시 즉시 수정 후 재컴파일
- 컴파일 성공 확인 후 작업 완료로 간주

## 프로젝트 고유 규칙

- 시간 계산은 반드시 TimeService 기준. 시간 조작 방지
- 오프라인 보상 계산은 순수 함수로 구현하여 테스트 용이성 확보
- 밸런스 데이터는 Google Sheets -> ConfigsProvider 파이프라인으로 관리
- 재화/아이템 수량 변경은 반드시 CommandService 커맨드 패턴으로 실행
- UI는 Addressables 기반 비동기 로딩. Resources 폴더 사용 최소화
- 전투 로직에 결정론이 필요하면 floatP 사용
