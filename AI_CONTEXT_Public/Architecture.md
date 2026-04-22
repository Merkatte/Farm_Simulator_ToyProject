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
│   │   ├── Actor/          # 게임 오브젝트 로직 (FarmCell, FarmerAI, FarmerBehaviors — partial)
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

2x2 격자의 농경지 칸(FarmCell)이 **WorkProgress 기반 협력 작업 시스템**으로 동작한다. 농부들이 매 프레임 `ContributeWork()`로 작업량을 누적하면 셀이 자동으로 상태를 전이한다. AnimationCurve 효율 체감으로 다수 농부 협력을 밸런싱한다.

### 상태 사이클

```
Untilled ──(Tilling 작업)──► Tilled ──(Planting 작업)──► Seeded
                                                           │
                                                    (자연 성장만)
                                                           ▼
Grown ◄──(Growing 작업 + 자연 성장)── Growing
  │
  ├ 수확 1~4회 → Growing 재진입
  └ 수확 5회   → Untilled 리셋
```

### 협력 작업 모델

각 상태마다 `MaxWork` 임계값이 있으며, 농부들이 `ContributeWork(rawAmount)`를 호출하면 셀 내부에서 효율 곡선을 적용해 `_workProgress`에 누적한다.

| 상태 | MaxWork 필드 | 농부 기여 | 자연 성장 |
|------|-------------|-----------|-----------|
| Untilled | TillingMaxWork | ✅ | ❌ |
| Tilled | PlantingMaxWork | ✅ | ❌ |
| Seeded | SeededMaxWork | ❌ (무시) | ✅ |
| Growing | GrowingMaxWork | ✅ | ✅ |
| Grown | HarvestingMaxWork | ✅ | ❌ |

효율 계산: `rawAmount × FarmerEfficiencyCurve(min(n, MaxEffectiveFarmers)) / n` (n = 활성 농부 수)

### 스크립트 목록

#### `Enum/FarmCellState.cs`
농경지 칸의 상태를 정의하는 열거형.

```
Untilled  — 경작 전 (초기 상태)
Tilled    — 경작 완료, 씨앗을 심을 수 있는 상태
Seeded    — 씨앗 심음, 자연 성장 진행 중 (농부 개입 불가)
Growing   — 성장 중, 농부 기여 + 자연 성장
Grown     — 성장 완료 (수확 가능)
```

#### `Interface/IFarmCell.cs`
FarmCell의 공용 API. AI 에이전트와 플레이어 모두 이 인터페이스를 통해 상호작용한다.

```
State                      — 현재 상태 프로퍼티
WorkProgress               — 현재 작업 누적량
MaxWork                    — 현재 상태의 목표 작업량
HarvestCount               — 현재 사이클에서 수확한 횟수

CanTill()                  — Untilled 상태
CanPlant()                 — Tilled + _dedicatedSeed 할당됨
CanHelpGrow()              — Growing 상태
CanHarvest()               — Grown 상태

RegisterFarmer()           — 활성 농부 카운터 증가
UnregisterFarmer()         — 활성 농부 카운터 감소 (min 0)
ContributeWork(float)      — 원시 기여량 전달 (셀이 효율 곡선 적용)
TryConsumeHarvest(out int) — 최초 호출자만 true + yield 반환 (토큰 패턴)
SetHighlight(bool)         — 선택 하이라이트 시각 처리
```

#### `Interface/IWarehouse.cs`
창고 공용 계약. AI가 수확물을 보관할 때 이 인터페이스를 사용한다.

```
Position      — 창고 월드 좌표 (Vector3)
Deposit(int)  — 수확물 보관
```

#### `Data/FarmCellConfig.cs` (ScriptableObject)
칸(Cell) 고유 설정. `Normalize()`/`OnValidate()` 포함.

```
[Work Requirements]
TillingMaxWork    — 경작 목표 작업량 (기본 30f)
PlantingMaxWork   — 파종 목표 작업량 (기본 20f)
SeededMaxWork     — 발아 자연 성장 임계값 (기본 10f)
GrowingMaxWork    — 성장 목표 작업량 (기본 60f)
HarvestingMaxWork — 수확 목표 작업량 (기본 15f)

[Growth]
NaturalGrowthRate — Seeded/Growing 초당 자연 진척량 (기본 1f)

[Farmer Efficiency]
FarmerEfficiencyCurve — 농부 수 → 총 효율 배율 AnimationCurve
MaxEffectiveFarmers   — 곡선 적용 최대 농부 수 (기본 5)

[Harvest]
MaxHarvestCount — 최대 수확 횟수, 초과 시 Untilled 리셋 (기본 5)
HarvestYield    — 수확 1회당 수확량 (기본 1, 최소 1)

[State Colors]
UntilledColor / TilledColor / SeededColor / GrowingColor / GrownColor
```

에셋 생성: Project 패널 우클릭 → Create → Project_FAD/FarmCellConfig

#### `Data/SeedConfig.cs` (ScriptableObject)
작물 정체성 데이터. 현재는 메타데이터 보관 전용이며 런타임 로직에 직접 영향을 주지 않는다.

```
DisplayName  — 작물 이름 (기본 "Unknown Crop")
HarvestYield — 작물별 수확량 (미래 확장용, 현재는 FarmCellConfig.HarvestYield 우선)
Icon         — 작물 아이콘 Sprite (선택)
```

각 FarmCell의 `_dedicatedSeed` 슬롯에 Inspector에서 고정 할당한다 (전용 밭 방식).

에셋 생성: Project 패널 우클릭 → Create → Project_FAD/SeedConfig

#### `Actor/FarmCell.cs` (MonoBehaviour, IFarmCell 구현)
개별 농경지 칸의 로직을 담당한다.

- `[SerializeField] FarmCellConfig _config` — Inspector에서 에셋 할당
- `[SerializeField] SeedConfig _dedicatedSeed` — Inspector에서 고정 할당 (전용 밭)
- `_workProgress` — 현재 작업 누적량
- `_harvestCount` — 현재 사이클 수확 횟수
- `_activeFarmerCount` — 현재 등록된 농부 수
- `_harvestClaimed` — 수확 토큰 플래그 (중복 수취 방지)
- `Awake()`: `_config.Normalize()` 호출로 런타임 수치 보장
- `Update()`: Seeded/Growing에서 `NaturalGrowthRate` 누적 → `MaxWork` 도달 시 `AutoTransition()`
- `ContributeWork(float)`: Seeded 무시, 그 외 효율 곡선 적용 후 `_workProgress` 누적
- `AutoTransition()`: Grown에서 `_harvestCount++` → `MaxHarvestCount` 도달 시 Untilled, 아니면 Growing 재진입
- `TryConsumeHarvest(out int)`: `_harvestClaimed` 토큰으로 최초 호출자만 yield 반환

#### `Manager/FarmlandManager.cs` (MonoBehaviour, Singleton)
2x2 FarmCell 배열을 관리한다. AI와 플레이어가 셀에 접근하기 위한 공용 API를 제공한다.

- `[SerializeField] FarmCell[] _cells` — Inspector에서 4개 셀 연결 (직렬화 목적)
- `IFarmCell[] _farmCells` — Awake()에서 캐시, 이후 모든 게임 로직은 인터페이스로 호출
- `AllCells` — `IReadOnlyList<IFarmCell>` (AI 탐색용)
- `TryGetCellPosition(IFarmCell, out Vector3)` — 셀 월드 좌표 조회. 파괴된 오브젝트 방어 코드 포함.

### 의존 관계

```
FarmlandManager
    │ (SerializeField 직렬화)
    ├─ FarmCell[] _cells
    │
    │ (IFarmCell 인터페이스로만 호출)
    └─ IFarmCell[] _farmCells
           │
           └─ FarmCell (IFarmCell 구현)
                  ├─ FarmCellConfig (SO, 작업 수치 + 효율 곡선 + 색상)
                  └─ SeedConfig _dedicatedSeed (SO, 작물 정체성 — Inspector 고정)

FarmerAI
    │ (IFarmCell 인터페이스로만 호출)
    ├─ FarmlandManager.Instance → AllCells, TryGetCellPosition
    ├─ IFarmCell _targetCell → RegisterFarmer / UnregisterFarmer / ContributeWork
    ├─ IWarehouse _warehouse → WarehouseManager.Instance
    └─ FarmerConfig (SO, 수치 설정)
```

AI 에이전트나 플레이어 캐릭터가 추가될 때 `IFarmCell`만 참조하면 된다. FarmCell 구체 타입에 의존하지 않는다.

---

## Scene 조립 가이드 (Unity 에디터 수동 작업)

1. Hierarchy에서 빈 오브젝트 `Farmland` 생성 → `FarmlandManager` 컴포넌트 추가
2. `Farmland` 자식으로 `Cell_0` ~ `Cell_3` 4개 오브젝트 생성
   - 각각 `FarmCell` 컴포넌트 + `SpriteRenderer` 추가 (`FarmCell`에 `[RequireComponent]` 적용됨)
   - SpriteRenderer의 Sprite에 Default Sprite(흰 사각형) 할당
3. 위치: 2x2 격자 배치 (예: `(-0.6, 0.6)`, `(0.6, 0.6)`, `(-0.6, -0.6)`, `(0.6, -0.6)`)
4. `FarmCellConfig` SO 에셋 생성 후 각 `FarmCell._config` 슬롯에 할당
5. `SeedConfig` SO 에셋 각 작물별로 생성 후 각 `FarmCell._dedicatedSeed` 슬롯에 할당 (전용 밭)
6. `FarmlandManager._cells[4]` 배열에 `Cell_0` ~ `Cell_3` 연결
7. Main Camera의 Projection을 **Orthographic**으로 변경, Size 조정

---

## 구현된 시스템: AI 농부 에이전트 (FarmerAI)

### 개요

자율적으로 농사 사이클을 수행하는 AI 농부 1체. 스폰 직후 밭 탐색을 시작해 경작→파종→성장보조→수확→창고 반납을 반복한다.

### 상태 사이클

```
Idle ──[가중치 선택]──► Loitering (멍때리기, 1~3초 대기)
     │
     └──► MovingToCell → Tilling     (Untilled 칸, ContributeWork 반복)
                       → Planting    (Tilled 칸, ContributeWork 반복)
                       → HelpingGrow (Growing 칸, ContributeWork 반복)
                       → Harvesting  (Grown 칸, ContributeWork → TryConsumeHarvest)
                                              ↓
                                       MovingToWarehouse → Depositing → Idle
```

행동 선택: **Weighted Utility AI** — `기본값 × RecencyPenalty × EnvironmentMultiplier × ContextMultiplier`

### 스크립트 목록

#### `Enum/FarmerState.cs`
AI 농부의 9가지 작업 상태 열거형.

```
Idle / Loitering / MovingToCell / Tilling / Planting / HelpingGrow / Harvesting / MovingToWarehouse / Depositing
```

#### `Interface/IWarehouse.cs`
창고 공용 계약. `Vector3 Position`, `void Deposit(int amount)`.

#### `Data/FarmerConfig.cs` (ScriptableObject)
AI 수치 설정. Weighted Utility AI 행동 가중치·패널티를 제어한다. `Normalize()`로 런타임 최솟값 보장.

```
[Movement]
MoveSpeed              — 이동 속도 (기본 2f)
InteractionRange       — 도달 판정 거리 (기본 0.1f)

[Work]
WorkContributionPerSecond — 농부 1명의 초당 기여량 (기본 1f)

[Action Base Weights]
LoiterBaseWeight       — 멍때리기 기본 가중치 (기본 1f)
TillBaseWeight         — 경작 기본 가중치 (기본 1f)
PlantBaseWeight        — 파종 기본 가중치 (기본 1f)
HelpGrowBaseWeight     — 성장보조 기본 가중치 (기본 1f)
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

#### `Actor/FarmerAI.cs` + `Actor/FarmerBehaviors.cs` (partial class, MonoBehaviour)
AI 본체. `FarmerConfig`를 Inspector에서 할당해야 동작한다.

- `partial class FarmerAI`로 두 파일에 분산 구성한다.
  - `FarmerAI.cs`: 생명주기(Awake/Start/Update), TransitionTo, HandleIdle, 가중치 계산, ActionCandidate, 모든 필드
  - `FarmerBehaviors.cs`: 상태별 Handle* 실행, IsTargetValid, MoveTowards, FindFirstWhere
- 새 행동 추가 절차: ①`FarmerState` enum 추가 → ②`FarmerAI.cs`의 `BuildActionCandidates`에 `ActionCandidate` 추가 → ③`FarmerBehaviors.cs`에 Handle 메서드 추가 → ④`FarmerAI.cs`의 `Update` switch에 라우팅 추가
- 행동 선택: `BuildActionCandidates()`로 후보 리스트 구성 → 각 후보의 `ComputeWeight()` 계산 → 가중치 비례 랜덤 선택
- 가중치 공식: `기본값 × GetRecencyPenalty() × GetEnvironmentMultiplier() × ContextMultiplier`
  - `GetRecencyPenalty()`: 최근 행동 이력 큐를 검사해 반복 행동에 패널티
  - `GetEnvironmentMultiplier()`: 현재 stub (1f). 향후 배고픔/피로 등 환경 요소 확장 지점
  - `ContextMultiplier`: 유효 타겟 존재 시 `AvailableTargetMultiplier` 적용. 타겟 없는 비-Loiter 행동은 0
- `ActionCandidate` 내부 클래스: `Kind`, `FindTarget`, `ComputeWeight`, `Execute`. 새 행동은 이 타입으로 리스트에 추가
- `FindFirstWhere(cells, predicate)`: CanXxx 술어로 첫 번째 유효 타겟 탐색 (`FarmerBehaviors.cs`)
- `SafeUnregisterTarget()`: Unity overloaded null 체크 후 UnregisterFarmer 호출. `TransitionTo`와 `OnDisable`에서 사용
- `TransitionTo(next)`: Working 상태 이탈 시 `SafeUnregisterTarget()`, 진입 시 `RegisterFarmer()` 자동 처리
- `IsTargetValid()`: Unity 소멸 객체(`obj == null`) + FarmlandManager 등록 여부를 동시에 검증
- `HandleWorking()`: Tilling/Planting/HelpingGrow/Harvesting 공통 처리. 상태 불일치 시 Idle 복귀. Harvesting 완료 시 `TryConsumeHarvest` → MovingToWarehouse
- 이동: `Vector3.MoveTowards` 기반 2D 이동

#### `Manager/WarehouseManager.cs` (MonoBehaviour, Singleton, IWarehouse 구현)
수확물 보관 창고.

- `_storedCount` — 총 수확량 누적
- `Deposit(int amount)` — 수확량 추가 및 로그 출력

### Scene 조립 가이드 (AI 에이전트 추가 시)

1. `FarmerConfig` SO 에셋 생성 후 수치 설정
2. Hierarchy에 빈 `Warehouse` 오브젝트 생성 → `WarehouseManager` 컴포넌트 추가, 적당한 위치 배치
3. `Farmer` 오브젝트 생성 → `FarmerAI` 컴포넌트 + SpriteRenderer 추가, `FarmerConfig` 슬롯에 할당
4. 각 FarmCell의 `_dedicatedSeed` 슬롯에 작물 SeedConfig 할당 (없으면 CanPlant() = false)

---

## 미구현 예정 시스템

- **플레이어 상호작용**: 클릭 또는 이동 기반 입력. `IFarmCell`을 통해 동일하게 구현 가능.
- **애니메이션**: 물 주기, 잡초 제거, 비료 주기 등 모션.
- **UI**: 상태 표시 HUD, 창고 수확량 표시.
