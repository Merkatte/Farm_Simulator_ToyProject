# Project_FAD — Architecture

Unity 6000.3.9f1 (URP 17.3) 기반 2D 농경 + AI 에이전트 협력 게임.

---

## 폴더 구조

```
Project_FAD/
├── Assets/
│   ├── Scripts/
│   │   ├── Enum/           # 프로젝트 전체 열거형 (FarmCellState, FarmerState)
│   │   ├── Interface/      # 프로젝트 전체 인터페이스 (IFarmCell, IWarehouse)
│   │   ├── Data/           # ScriptableObject 정의 (FarmCellConfig, SeedConfig, FarmerConfig)
│   │   ├── Actor/          # 게임 오브젝트 로직 (FarmCell, FarmerAI)
│   │   ├── Manager/        # Singleton Manager (FarmlandManager, WarehouseManager)
│   │   └── UI/             # UI 패널, HUD (미구현)
│   └── Scenes/
│       └── SampleScene.unity
├── AI_CONTEXT_Claude/      # Claude 전용 작업 기록
├── AI_CONTEXT_Codex/       # Codex 전용 작업 기록 (Claude 접근 금지)
└── AI_CONTEXT_Public/      # 양쪽 공용 문서 (이 파일)
```

---

## 현재 구현된 시스템: 농경지 (Farmland)

### 개요

2x2 격자의 농경지 칸(FarmCell)이 상태 사이클을 갖고 시간에 따라 자동으로 성장한다.
키보드 디버그 입력으로 각 칸의 상태 전이를 수동으로 검증할 수 있다.

### 상태 사이클

```
Untilled → Tilled → Seeded → Growing → Grown
   (Q)       (W)     (자동)    (자동)
                     ↑ E(Boost)로 가속 가능
```

### 스크립트 목록

#### `Enum/FarmCellState.cs`
농경지 칸의 상태를 정의하는 열거형.

```
Untilled  — 경작 전 (초기 상태)
Tilled    — 경작 완료, 씨앗을 심을 수 있는 상태
Seeded    — 씨앗 심음, 성장 타이머 진행 중
Growing   — 성장 중, 성장 타이머 진행 중
Grown     — 성장 완료 (수확 가능)
```

#### `Interface/IFarmCell.cs`
FarmCell의 공용 API. AI 에이전트와 플레이어 모두 이 인터페이스를 통해 상호작용한다.

```
State              — 현재 상태 프로퍼티
CanTill()          — Untilled 상태 + config null 아님
CanPlant()         — Tilled 상태 + config null 아님
CanBoost()         — Seeded 또는 Growing 상태 + config null 아님
CanHarvest()       — Grown 상태 + config null 아님
Till()             — Untilled → Tilled
PlantSeed(SeedConfig seed) — Tilled → Seeded. 씨앗을 넘겨받아 성장 타이머 시작
Boost()            — 성장 타이머 감소 (BoostReductionSeconds만큼)
Harvest()          — Grown → Untilled 리셋. 수확량(int) 반환. 실패 시 0
SetHighlight(bool) — 선택 하이라이트 시각 처리
```

#### `Interface/IWarehouse.cs`
창고 공용 계약. AI가 수확물을 보관할 때 이 인터페이스를 사용한다.

```
Position      — 창고 월드 좌표 (Vector3)
Deposit(int)  — 수확물 보관
```

#### `Data/FarmCellConfig.cs` (ScriptableObject)
칸(Cell) 고유 설정. 씨앗과 무관한 데이터만 보관한다.

```
BoostReductionSeconds   — Boost 1회당 타이머 감소량 (기본 2초)
UntilledColor / TilledColor / SeededColor / GrowingColor / GrownColor — 상태별 색상
```

에셋 생성: Project 패널 우클릭 → Create → Project_FAD/FarmCellConfig

#### `Data/SeedConfig.cs` (ScriptableObject)
씨앗 종류별 성장 데이터. 씨앗을 심을 때 FarmCell에 넘겨주며, 칸이 아닌 씨앗이 타이밍을 결정한다.

```
SeededToGrowingSeconds  — Seeded → Growing 전이 소요 시간 (기본 5초)
GrowingToGrownSeconds   — Growing → Grown 전이 소요 시간 (기본 10초)
```

에셋 생성: Project 패널 우클릭 → Create → Project_FAD/SeedConfig

#### `Actor/FarmCell.cs` (MonoBehaviour, IFarmCell 구현)
개별 농경지 칸의 로직을 담당한다.

- `[SerializeField] FarmCellConfig _config` — Inspector에서 에셋 할당 (칸 고유 설정)
- `_state` — 현재 상태 (FarmCellState)
- `_plantedSeed` — 현재 심겨 있는 씨앗의 SeedConfig. PlantSeed 시 보관, 수확(Grown) 시 null 처리
- `_growthTimer` — Seeded/Growing 상태에서 카운트다운
- `_isHighlighted` — 선택 하이라이트 플래그
- `Awake()`: `_config` null 시 `enabled = false` 및 에러 로그 출력
- `Update()`: 성장 상태일 때 타이머 감소 → 0 이하 시 `AdvanceGrowth()` while 루프
- `AdvanceGrowth()`: `_plantedSeed`의 타이밍 값 사용. 초과 시간을 다음 단계 타이머에 이월
- `ApplyStateVisual()`: 상태 색상 + 하이라이트 혼합 → SpriteRenderer 적용

#### `Manager/FarmlandManager.cs` (MonoBehaviour, Singleton)
2x2 FarmCell 배열을 관리한다. AI와 플레이어가 셀에 접근하기 위한 공용 API를 제공한다.

- `[SerializeField] FarmCell[] _cells` — Inspector에서 4개 셀 연결 (직렬화 목적)
- `[SerializeField] SeedConfig _defaultSeed` — AI가 파종 시 사용하는 기본 씨앗
- `IFarmCell[] _farmCells` — Awake()에서 캐시, 이후 모든 게임 로직은 인터페이스로 호출
- `DefaultSeed` — `_defaultSeed` 프로퍼티 (AI용)
- `AllCells` — `IReadOnlyList<IFarmCell>` (AI 탐색용)
- `TryGetCellPosition(IFarmCell, out Vector3)` — 셀 월드 좌표 조회. 실패 시 false 반환

### 의존 관계

```
FarmlandManager
    │ (SerializeField 직렬화)
    ├─ FarmCell[] _cells
    ├─ SeedConfig _defaultSeed ─────────────────────┐
    │                                               │
    │ (IFarmCell 인터페이스로만 호출)                  │
    └─ IFarmCell[] _farmCells                       │
           │                                        │
           └─ FarmCell (IFarmCell 구현)              │
                  │                                 │
                  ├─ FarmCellConfig (SO, 칸 설정)    │
                  └─ SeedConfig _plantedSeed ←──────┘
                     (PlantSeed 호출 시 주입)

FarmerAI
    │ (IFarmCell 인터페이스로만 호출)
    ├─ FarmlandManager.Instance → AllCells, TryGetCellPosition, DefaultSeed
    ├─ IFarmCell _targetCell
    ├─ IWarehouse _warehouse → WarehouseManager.Instance
    └─ FarmerConfig (SO, 수치 설정)
```

AI 에이전트나 플레이어 캐릭터가 추가될 때, `IFarmCell`만 참조하면 된다. FarmCell 구체 타입에 의존하지 않는다.
씨앗 종류를 바꾸려면 `SeedConfig` 에셋만 교체하면 되며, FarmCell은 어떤 씨앗인지 알 필요가 없다.

---

## Scene 조립 가이드 (Unity 에디터 수동 작업)

1. Hierarchy에서 빈 오브젝트 `Farmland` 생성 → `FarmlandManager` 컴포넌트 추가
2. `Farmland` 자식으로 `Cell_0` ~ `Cell_3` 4개 오브젝트 생성
   - 각각 `FarmCell` 컴포넌트 + `SpriteRenderer` 추가 (`FarmCell`에 `[RequireComponent]` 적용됨)
   - SpriteRenderer의 Sprite에 Default Sprite(흰 사각형) 할당
3. 위치: 2x2 격자 배치 (예: `(-0.6, 0.6)`, `(0.6, 0.6)`, `(-0.6, -0.6)`, `(0.6, -0.6)`)
4. `FarmCellConfig` SO 에셋 생성 후 각 `FarmCell._config` 슬롯에 할당
5. `SeedConfig` SO 에셋 생성 후 `FarmlandManager._seedConfig` 슬롯에 할당
7. `FarmlandManager._cells[4]` 배열에 `Cell_0` ~ `Cell_3` 연결
8. Main Camera의 Projection을 **Orthographic**으로 변경, Size 조정

---

## 구현된 시스템: AI 농부 에이전트 (FarmerAI)

### 개요

자율적으로 농사 사이클을 수행하는 AI 농부 1체. 스폰 직후 밭 탐색을 시작해 경작→파종→부스트→수확→창고 반납을 반복한다.

### 상태 사이클

```
Idle ──[가중치 선택]──► Loitering (멍때리기, 1~3초 대기)
     │
     └──► MovingToCell → Tilling   (Untilled 칸)
                       → Planting  (Tilled 칸)
                       → Boosting  (성장 가속) → Harvesting → MovingToWarehouse → Depositing → Idle
                       → Harvesting (Grown 칸)
```

행동 선택: **Weighted Utility AI** — `기본값 × RecencyPenalty × EnvironmentMultiplier × ContextMultiplier`

### 스크립트 목록

#### `Enum/FarmerState.cs`
AI 농부의 9가지 작업 상태 열거형. `Loitering`(멍때리기)이 추가되었다.

#### `Interface/IWarehouse.cs`
창고 공용 계약. `Vector3 Position`, `void Deposit(int amount)`.

#### `Data/FarmerConfig.cs` (ScriptableObject)
AI 수치 설정. 작업 시간 및 Weighted Utility AI 행동 가중치·패널티를 제어한다. `Normalize()`로 런타임 최솟값 보장.

```
[Movement]
MoveSpeed              — 이동 속도 (기본 2f)
InteractionRange       — 도달 판정 거리 (기본 0.1f)

[Work Duration]
TillingSeconds         — 경작 소요 시간 (기본 1.0f)
PlantingSeconds        — 파종 소요 시간 (기본 0.8f)
HarvestingSeconds      — 수확 소요 시간 (기본 1.2f)
BoostIntervalSeconds   — Boost 호출 간격 (기본 0.5f)

[Action Base Weights]
LoiterBaseWeight       — 멍때리기 기본 가중치 (기본 1f)
TillBaseWeight         — 경작 기본 가중치 (기본 1f)
PlantBaseWeight        — 파종 기본 가중치 (기본 1f)
BoostBaseWeight        — 부스트 기본 가중치 (기본 1f)
HarvestBaseWeight      — 수확 기본 가중치 (기본 2f)

[Loitering]
LoiterMinSeconds / LoiterMaxSeconds — 멍때리기 지속 범위 (기본 1~3초)

[Recency Penalty]
RecencyPenaltyFactor   — 최근 수행 행동에 곱하는 패널티 배율 (기본 0.3f)
RecencyHistorySize     — 추적할 최근 행동 이력 크기 (기본 3)

[Context Multipliers]
AvailableTargetMultiplier — 유효 타겟 존재 시 가중치 배율 (기본 2f)
```

에셋 생성: Project 패널 우클릭 → Create → Project_FAD/FarmerConfig

#### `Actor/FarmerAI.cs` (MonoBehaviour)
AI 본체. `FarmerConfig`를 Inspector에서 할당해야 동작한다.

- 행동 선택: `BuildActionCandidates()`로 후보 리스트 구성 → 각 후보의 `ComputeWeight()` 계산 → 가중치 비례 랜덤 선택
- 가중치 공식: `기본값 × GetRecencyPenalty() × GetEnvironmentMultiplier() × ContextMultiplier`
  - `GetRecencyPenalty()`: 최근 행동 이력 큐를 검사해 반복 행동에 패널티
  - `GetEnvironmentMultiplier()`: 현재 stub (1f). 향후 배고픔/피로 등 환경 요소 확장 지점
  - `ContextMultiplier`: 유효 타겟 존재 시 `AvailableTargetMultiplier` 적용. 타겟 없는 비-Loiter 행동은 0
- `ActionCandidate` 내부 클래스: `Kind`, `FindTarget`, `ComputeWeight`, `Execute`. 새 행동은 이 타입으로 리스트에 추가
- `IsTargetValid()`: Unity 소멸 객체 + FarmlandManager 등록 여부를 동시에 검증
- `HandleLoitering()`: LoiterMinSeconds~LoiterMaxSeconds 범위 무작위 대기 후 Idle 복귀
- 이동: `Vector3.MoveTowards` 기반 2D 이동
- Boost: `BoostIntervalSeconds` 간격으로 `Boost()` 반복, Grown 감지 시 Harvesting으로 전이

#### `Manager/WarehouseManager.cs` (MonoBehaviour, Singleton, IWarehouse 구현)
수확물 보관 창고.

- `_storedCount` — 총 수확량 누적
- `Deposit(int amount)` — 수확량 추가 및 로그 출력

### Scene 조립 가이드 (AI 에이전트 추가 시)

1. `FarmerConfig` SO 에셋 생성 후 수치 설정
2. Hierarchy에 빈 `Warehouse` 오브젝트 생성 → `WarehouseManager` 컴포넌트 추가, 적당한 위치 배치
3. `Farmer` 오브젝트 생성 → `FarmerAI` 컴포넌트 + SpriteRenderer 추가, `FarmerConfig` 슬롯에 할당
4. `FarmlandManager` Inspector에서 `_defaultSeed` 슬롯에 SeedConfig 연결

---

## 미구현 예정 시스템

- **플레이어 상호작용**: 클릭 또는 이동 기반 입력. `IFarmCell`을 통해 동일하게 구현 가능.
- **애니메이션**: 물 주기, 잡초 제거, 비료 주기 등 모션.
- **UI**: 상태 표시 HUD, 창고 수확량 표시.
