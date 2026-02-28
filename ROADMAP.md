# Idle RPG 개발 로드맵

> 최종 업데이트: 2026-02-28

## 현재 상태 요약

핵심 전투 루프(영웅 ↔ 적 전투, 웨이브/스테이지/챕터 진행)가 동작하는 상태.
방치형 RPG의 핵심 게임 루프(**전투 → 보상 → 성장 → 더 강한 전투**)를 완성하기 위해
경제 → 성장 → 저장 → UI 순서로 구조를 잡아간다.

로드맵의 세부 내용은 절대로 그대로 구현할 필요 없으며 임시로 작성해둔것임.
큰 단위의 구현을 진행하면서 작업내역을 정리해두면 됨.

---

## Phase 1. 경제 기반 (Core Economy)

> 보상/성장/상점/가챠 등 거의 모든 시스템이 재화에 의존한다. 가장 먼저 구축해야 하는 기반.

### 1-1. 재화 시스템 (CurrencyService)

- [x] `CurrencyType` 열거형 정의 (Gold, Gem)
- [x] `CurrencyModel` 구현 (ObservableDictionary<CurrencyType, BigNumber> 기반 재화 저장소)
- [x] `ICurrencyService` / `CurrencyService` 구현
- [x] 커맨드 패턴으로 재화 증감 처리 (CommandService 연동)
- [x] GameInstaller에 CurrencyService 바인딩
- [x] asmdef 생성 (IdleRPG.Economy)

### 1-2. 보상 시스템 (RewardService)

- [x] `RewardConfig` 설정 데이터 (적 처치 보상)
- [x] `IRewardService` / `RewardService` 구현
- [x] `EnemyDiedMessage` 구독 → 골드 지급
- [x] GameInstaller에 RewardService 바인딩
- [x] asmdef 생성 (IdleRPG.Reward)

---

## Phase 2. 성장/강화 시스템

> 재화를 소비해서 영웅이 강해지는 핵심 루프. 이것이 있어야 방치형의 재미가 생긴다.

### 2-1. 영웅 성장 서비스

- [x] `GrowthConfig` + `StatGrowthEntry` 설정 데이터 (스탯별 성장/비용 곡선 파라미터)
- [x] `StatGrowthFormula` 순수 함수 (스탯 값/비용 계산, HP 재생 연동)
- [x] `HeroGrowthModel` 구현 (ObservableDictionary 기반 스탯별 레벨 관리)
- [x] `CombatPowerCalculator` 전투력 계산 (DPS _ 크리티컬 _ 멀티샷 + HP 보정)
- [x] `IHeroGrowthService` / `HeroGrowthService` 구현
- [x] HeroStatType 확장 (DoubleShot, TripleShot, AdvancedAttack, EnemyBonusDamage)
- [x] HeroModel에 확장 스탯 4개 추가 + SetBigNumberStat/SetFloatStat 메서드
- [x] 레벨업 시 HeroModel의 ObservableField 갱신 + 전투력 재계산
- [x] 스탯 체인 언락 시스템 (AttackSpeed→DoubleShot→TripleShot→AdvancedAttack→EnemyBonusDamage)
- [x] BattleService/BattleField 연동 (이중/삼중 사격, 고급공격 데미지 배율)
- [x] GameInstaller에 HeroGrowthService 바인딩
- [x] asmdef 생성 (IdleRPG.Growth)
- [x] `StatLevelUpMessage`, `CombatPowerChangedMessage` 이벤트 정의

---

## Phase 3. 저장/로드 시스템

> Phase 1~2가 있어야 저장할 "의미 있는 데이터"가 생긴다.

### 3-1. SaveSystem 구현

- [x] 저장 데이터 구조 정의 (GameSaveData DTO + Stage/Currency/Growth 서브 DTO)
- [x] DataService를 활용한 JSON 직렬화/역직렬화
- [x] 저장 대상: StageModel, CurrencyModel, HeroGrowthModel
- [x] 2-Tier 저장 전략 (즉시 저장 + Dirty Debounce)
- [x] 30초 주기 자동 저장 (TickService)
- [x] 이벤트 기반 dirty 마킹 (StatLevelUpMessage, StageClearedMessage, ChapterClearedMessage)
- [x] 앱 시작 시 데이터 복원 로직
- [x] 앱 종료/백그라운드 전환 시 저장 (OnApplicationPause/Quit)
- [x] SaveVersion 필드로 향후 마이그레이션 대응
- [x] LastSaveTimestamp 필드로 오프라인 보상 준비 (Phase 6)

---

## Phase 4. 기본 UI

> 로직이 동작해야 보여줄 것이 생긴다. UiPresenter MVP 패턴으로 구현.

### 4-1. Battle HUD

- [ ] 스테이지 표시 UI ("1-3", 웨이브 진행률)
- [ ] 영웅 HP 바
- [ ] 적 HP 바
- [ ] 보스 도전 토글 버튼
- [ ] 재화 표시 (골드, 젬 등)
- [ ] UiPresenter<T> 기반 MVP 구현

### 4-2. Growth UI (강화 패널)

- [ ] 스탯별 현재 레벨 / 레벨업 비용 표시
- [ ] 레벨업 버튼 (재화 부족 시 비활성화)
- [ ] 레벨업 효과 연출 (선택)
- [ ] UiPresenter<T> 기반 MVP 구현

### 4-3. 메인 네비게이션

- [ ] 탭 기반 하단 네비게이션 (전투 / 강화 / 장비 / 소환 등)
- [ ] 피처 시스템으로 탭별 UI 세트 관리

---

## Phase 5. Config 데이터 파이프라인

> 밸런싱을 위한 데이터 관리 체계. 하드코딩을 제거하고 외부 데이터 소스로 전환.

### 5-1. ConfigsProvider 통합

- [ ] HeroConfig → ScriptableObject 전환
- [ ] EnemyConfig → ScriptableObject 전환
- [ ] StageConfig → ScriptableObject 전환
- [ ] GrowthConfig → ScriptableObject 전환
- [ ] RewardConfig → ScriptableObject 전환
- [ ] GameInstaller에서 하드코딩 제거, ConfigsProvider로 대체

### 5-2. Google Sheets 연동

- [ ] 시트 구조 설계 (Hero, Enemy, Stage, Growth, Reward 탭)
- [ ] CsvParser 기반 임포터 작성
- [ ] Editor 스크립트로 원클릭 임포트

---

## Phase 6. 오프라인 보상

> 방치형 게임의 정체성. 앱을 켜지 않아도 보상이 쌓이는 시스템.

### 6-1. OfflineRewardService

- [ ] TimeService 기반 오프라인 경과 시간 계산
- [ ] 순수 함수로 오프라인 보상 계산 (테스트 용이성)
- [ ] 최대 오프라인 시간 제한
- [ ] 앱 복귀 시 오프라인 보상 팝업 UI

---

## Phase 7. 콘텐츠 시스템

> 게임의 깊이를 더하는 콘텐츠. Phase 1~6이 안정된 후 순차적으로 추가.

### 7-1. 장비 시스템 (Equipment)

- [ ] EquipmentConfig (장비 타입, 등급, 스탯 보너스)
- [ ] EquipmentModel (장착 상태, 강화 레벨)
- [ ] IEquipmentService / EquipmentService
- [ ] 장비 장착 시 HeroModel 스탯 반영
- [ ] 장비 UI

### 7-2. 인벤토리 시스템 (Inventory)

- [ ] InventoryModel (아이템 목록, 슬롯 관리)
- [ ] IInventoryService / InventoryService
- [ ] 인벤토리 UI (그리드/리스트 뷰)

### 7-3. 가챠 시스템 (Gacha)

- [ ] GachaConfig (확률 테이블, 보장 시스템)
- [ ] IGachaService / GachaService
- [ ] 소환 연출 / 결과 UI

### 7-4. 퀘스트 시스템 (Quest)

- [ ] QuestConfig (일일/주간/업적)
- [ ] QuestModel (진행 상태, 완료 여부)
- [ ] IQuestService / QuestService
- [ ] 퀘스트 UI

### 7-5. 상점 시스템 (Shop)

- [ ] ShopConfig (상품 목록, 가격)
- [ ] IShopService / ShopService
- [ ] 상점 UI

---

## 구현 완료된 항목

### Core 인프라 ✅

- [x] GameInstaller (DI 부트스트랩)
- [x] MainInstaller 서비스 바인딩
- [x] MessageBrokerService (Pub/Sub 이벤트)
- [x] TickService, CoroutineService
- [x] PoolService (오브젝트 풀링)
- [x] DataService (데이터 영속성)
- [x] TimeService (시간 관리)
- [x] BigNumber 구조체 (대형 숫자)
- [x] BattleMessages (전투 이벤트 정의)
- [x] StageMessages (스테이지 이벤트 정의)
- [x] ISaveService / SaveService (2-Tier 저장: 즉시 + Dirty Debounce)
- [x] GameSaveData DTO (Stage/Currency/Growth)
- [x] SaveMessages (저장/로드 이벤트)

### 전투 시스템 ✅

- [x] IBattleService / BattleService
- [x] BattleField (전투 필드 MonoBehaviour)
- [x] BattleModel (전투 런타임 상태)
- [x] EnemyConfig / EnemyModel / EnemyView
- [x] HeroConfig / HeroModel / HeroView
- [x] HeroState / HeroStatType 열거형
- [x] ProjectileView / ProjectileData
- [x] 영웅 공격 → 투사체 → 적 피격 루프
- [x] 적 이동/공격 → 영웅 피격 루프
- [x] 사망/리스폰 처리

### 스테이지 시스템 ✅

- [x] IStageService / StageService
- [x] StageModel (챕터/스테이지/웨이브 진행)
- [x] StageConfig (스테이지 구조/밸런스)
- [x] 웨이브 진행 (일반 → 보스)
- [x] 보스 자동 도전 토글
- [x] 챕터/스테이지 진행 로직

### 폴더 구조 ✅

- [x] Feature 기반 폴더 구조 스캐폴딩
- [x] asmdef 구성 (Core, Bootstrap, Hero, Battle, Stage, Editor, Tests)

---

## 참고

- 모든 코드는 CLAUDE.md의 CBD 원칙을 따른다
- 재화 변경은 반드시 CommandService 커맨드 패턴으로 실행
- UI는 UiPresenter MVP 패턴 + Addressables 비동기 로딩
- 밸런스 데이터는 최종적으로 Google Sheets → ConfigsProvider 파이프라인으로 관리
- 시간 계산은 반드시 TimeService 기준
