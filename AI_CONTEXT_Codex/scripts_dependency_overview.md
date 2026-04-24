# Assets/Scripts Dependency Overview

작성일: 2026-04-22

이 문서는 `Assets/Scripts` 아래 C# 스크립트의 의존 관계와 주요 함수 역할을 빠르게 다시 이해하기 위한 Codex용 공용 메모다.

## 1. 전체 구조

```text
Assets/Scripts
├── Actor
│   ├── FarmCell.cs
│   ├── FarmerAI.cs
│   └── FarmerBehaviors.cs
├── Data
│   ├── FarmCellConfig.cs
│   ├── FarmerConfig.cs
│   └── SeedConfig.cs
├── Enum
│   ├── FarmCellState.cs
│   └── FarmerState.cs
├── Interface
│   ├── IFarmCell.cs
│   └── IWarehouse.cs
└── Manager
    ├── FarmlandManager.cs
    └── WarehouseManager.cs
```

핵심 시스템은 농지 셀(`FarmCell`)과 농부 AI(`FarmerAI`)로 나뉜다. `FarmCell`은 농지 상태와 작업 진행도를 관리하고, `FarmerAI`는 가능한 농지 작업을 가중치 기반으로 선택해 이동, 작업, 수확, 창고 반납을 반복한다.

## 2. 의존도 그래프

```text
FarmerAI + FarmerBehaviors
├── FarmerConfig
├── FarmerState
├── FarmlandManager.Instance
│   └── IFarmCell[] AllCells
│       └── FarmCell
│           ├── FarmCellConfig
│           ├── SeedConfig
│           ├── FarmCellState
│           └── SpriteRenderer
├── IFarmCell _targetCell
└── IWarehouse _warehouse
    └── WarehouseManager.Instance

FarmlandManager
├── FarmCell[] _cells
└── IFarmCell[] _farmCells

WarehouseManager
└── IWarehouse 구현
```

주요 의존 방향은 다음과 같다.

- `FarmerAI`는 구체 `FarmCell` 대신 `IFarmCell` 인터페이스를 통해 농지 셀을 조작한다.
- `FarmerAI`는 창고 역시 `IWarehouse` 인터페이스로 사용하지만, 실제 참조는 `WarehouseManager.Instance`에서 가져온다.
- `FarmlandManager`는 씬에 연결된 `FarmCell[]`을 보관하고, 외부에는 `IReadOnlyList<IFarmCell>`로 노출한다.
- `FarmCell`은 `FarmCellConfig`의 수치와 색상 설정, `SeedConfig`의 존재 여부, `FarmCellState`에 따라 동작한다.
- `WarehouseManager`는 수확물 총량만 누적하는 간단한 싱글톤 창고다.

## 3. 런타임 흐름

### 3.1 농부 AI 행동 루프

```text
Idle
└── BuildActionCandidates()
    ├── Loitering 후보
    ├── Tilling 후보: CanTill()
    ├── Planting 후보: CanPlant()
    ├── HelpingGrow 후보: CanHelpGrow()
    └── Harvesting 후보: CanHarvest()

후보별 가중치 계산
└── baseWeight * recencyPenalty * environmentMultiplier * contextMultiplier

선택된 행동 실행
└── MovingToCell
    └── 도착 후 셀 상태에 맞는 작업 상태로 전환
        ├── Tilling
        ├── Planting
        ├── HelpingGrow
        └── Harvesting
```

작업 상태에서는 `FarmCell.RegisterFarmer()`로 활성 농부 수를 올리고, 매 프레임 `FarmCell.ContributeWork()`를 호출한다. 작업 상태에서 빠져나갈 때는 `FarmCell.UnregisterFarmer()`를 호출한다.

### 3.2 농지 셀 상태 전환

```text
Untilled
  └── Tilling 작업 완료
Tilled
  └── Planting 작업 완료
Seeded
  └── 자연 성장 완료
Growing
  ├── 자연 성장
  └── HelpingGrow 작업 기여 가능
Grown
  └── Harvesting 작업 완료
      ├── 수확 횟수 < MaxHarvestCount: Growing으로 복귀
      └── 수확 횟수 >= MaxHarvestCount: Untilled로 리셋
```

`Seeded`와 `Growing` 상태는 `Update()`에서 `NaturalGrowthRate * Time.deltaTime`만큼 자연 성장한다. `Growing`은 농부의 `HelpingGrow` 작업 기여도 함께 받을 수 있다. `Seeded`는 `ContributeWork()`에서 명시적으로 무시된다.

### 3.3 수확과 창고 반납

```text
Harvesting
└── FarmCell 작업 완료로 Grown 상태가 바뀜
    └── TryConsumeHarvest(out yield)
        └── MovingToWarehouse
            └── Depositing
                └── WarehouseManager.Deposit(yield)
                    └── Idle
```

`TryConsumeHarvest()`는 `_harvestClaimed` 플래그로 같은 수확물을 중복 수령하지 못하게 막는다.

## 4. 파일별 역할

### 4.1 Actor/FarmCell.cs

`FarmCell`은 개별 농지 칸 하나의 상태 머신이다. `IFarmCell`을 구현하며, 작업 진행도, 자연 성장, 수확 횟수, 하이라이트 색상을 관리한다.

주요 필드:

- `_config`: 작업량, 성장 속도, 효율 곡선, 수확량, 상태 색상 설정.
- `_dedicatedSeed`: 이 셀에 지정된 작물 데이터. 현재는 `CanPlant()`에서 null 여부만 사용한다.
- `_state`: 현재 농지 상태.
- `_workProgress`: 현재 상태의 작업 누적량.
- `_harvestCount`: 현재 사이클에서 수확한 횟수.
- `_lastHarvestYield`: 마지막 수확 완료 시 산출된 수확량.
- `_harvestClaimed`: 마지막 수확량을 이미 가져갔는지 여부.
- `_activeFarmerCount`: 이 셀에 등록된 작업 중 농부 수.
- `_isHighlighted`: 선택 하이라이트 여부.

함수 역할:

- `Awake()`: `SpriteRenderer`를 캐시하고 `FarmCellConfig` 할당 여부를 검증한다. 설정값을 정규화한 뒤 초기 색상을 적용한다.
- `Update()`: `Seeded` 또는 `Growing` 상태에서 자연 성장 진행도를 누적하고, 목표치 도달 시 `AutoTransition()`을 호출한다.
- `CanTill()`: `Untilled` 상태이면 경작 가능으로 판단한다.
- `CanPlant()`: `Tilled` 상태이고 `_dedicatedSeed`가 있으면 파종 가능으로 판단한다.
- `CanHelpGrow()`: `Growing` 상태이면 성장 보조 가능으로 판단한다.
- `CanHarvest()`: `Grown` 상태이면 수확 가능으로 판단한다.
- `RegisterFarmer()`: 이 셀에서 작업 중인 농부 수를 1 증가시킨다.
- `UnregisterFarmer()`: 작업 중 농부 수를 1 감소시키되 0 아래로 내려가지 않게 한다.
- `ContributeWork(float amount)`: 활성 농부 수와 효율 곡선을 반영해 작업 진행도를 증가시킨다. `Seeded` 상태에서는 농부 작업 기여를 무시한다.
- `TryConsumeHarvest(out int yield)`: 마지막 수확량을 아직 가져가지 않았다면 수확량을 반환하고, 이후 중복 반환을 막는다.
- `SetHighlight(bool highlight)`: 선택 표시 여부를 저장하고 시각 상태를 다시 적용한다.
- `AutoTransition()`: 작업 진행도 완료 시 다음 농지 상태로 전환한다. `Grown` 완료 시 수확 횟수를 증가시키고, 최대 수확 횟수에 따라 `Growing` 또는 `Untilled`로 전환한다.
- `GetMaxWorkForState(FarmCellState state)`: 상태별 목표 작업량을 `FarmCellConfig`에서 가져온다.
- `SetState(FarmCellState newState)`: 현재 상태를 변경하고 시각 상태를 갱신한다.
- `ApplyStateVisual()`: 현재 상태 색상과 하이라이트 여부를 `SpriteRenderer.color`에 반영한다.
- `GetStateColor()`: 현재 상태에 대응하는 색상을 `FarmCellConfig`에서 반환한다.

### 4.2 Actor/FarmerAI.cs

`FarmerAI`는 농부의 생명주기, 상태 전환, 행동 선택 로직을 담당한다. 실제 상태별 행동 구현은 `FarmerBehaviors.cs` partial 파일에 나뉘어 있다.

주요 필드:

- `_config`: 이동 속도, 작업 기여량, 행동 가중치, 배회 시간, 최근 행동 패널티 설정.
- `_state`: 현재 농부 상태.
- `_targetCell`: 현재 작업 대상으로 선택된 농지 셀.
- `_warehouse`: 수확물을 맡길 창고 인터페이스.
- `_loiterTimer`: 배회 상태 유지 시간.
- `_heldCrop`: 농부가 들고 있는 수확량.
- `_recentActions`: 최근 선택 행동 기록. 같은 행동 반복을 줄이는 패널티 계산에 사용된다.

함수 역할:

- `Awake()`: `FarmerConfig` 할당 여부를 검증하고 설정값을 정규화한다.
- `Start()`: `WarehouseManager.Instance`와 `FarmlandManager.Instance`를 확인한다.
- `OnDisable()`: 작업 중 비활성화될 때 대상 셀에서 농부 등록을 안전하게 해제한다.
- `Update()`: 현재 `FarmerState`에 따라 적절한 처리 함수로 분기한다.
- `HandleIdle()`: 가능한 행동 후보를 만들고, 가중치 기반 확률 선택으로 다음 행동을 실행한다.
- `TransitionTo(FarmerState next)`: 상태를 전환한다. 작업 상태 이탈 시 대상 셀 등록을 해제하고, 작업 상태 진입 시 대상 셀에 농부를 등록한다.
- `SafeUnregisterTarget()`: `_targetCell`이 null이거나 Unity에서 파괴된 객체인지 확인한 뒤 안전하게 `UnregisterFarmer()`를 호출한다.
- `BuildActionCandidates(IReadOnlyList<IFarmCell> cells)`: 배회, 경작, 파종, 성장 보조, 수확 후보를 구성한다.
- `ComputeWeight(FarmerState kind, float baseWeight, bool hasTarget)`: 행동 기본 가중치, 최근 행동 패널티, 환경 배율, 대상 존재 배율을 곱해 최종 가중치를 계산한다.
- `GetRecencyPenalty(FarmerState kind)`: 최근 행동 기록에 같은 행동이 많을수록 낮은 배율을 반환한다.
- `GetEnvironmentMultiplier(FarmerState kind)`: 현재는 항상 1을 반환한다. 추후 욕구나 환경 조건을 넣기 위한 확장 지점이다.
- `RecordAction(FarmerState kind)`: 선택된 행동을 최근 행동 큐에 기록하고, 최대 기록 수를 넘으면 오래된 기록을 제거한다.
- `IsWorkingState(FarmerState s)`: 셀에 등록해야 하는 실제 작업 상태인지 판정한다.
- `ActionCandidate`: 행동 종류, 대상 탐색 함수, 가중치 계산 함수, 실행 함수를 묶는 내부 클래스다.

### 4.3 Actor/FarmerBehaviors.cs

`FarmerBehaviors`는 `FarmerAI` partial 클래스의 상태별 실행 로직이다.

함수 역할:

- `HandleLoitering()`: 배회 시간을 감소시키고 시간이 끝나면 `Idle`로 돌아간다.
- `HandleMoveToCell()`: 대상 셀 위치로 이동한다. 도착하면 대상 셀의 현재 가능 작업에 따라 `Harvesting`, `Planting`, `Tilling`, `HelpingGrow` 중 하나로 전환한다.
- `HandleWorking()`: 대상 셀이 유효한지 확인하고, 현재 작업 상태가 기대하는 농지 상태와 일치하면 작업 기여량을 전달한다. 수확 작업이 완료된 경우 `TryConsumeHarvest()`로 수확량을 받고 창고 이동 상태로 전환한다.
- `HandleMoveToWarehouse()`: 창고 위치로 이동하고 도착하면 `Depositing`으로 전환한다.
- `HandleDepositing()`: 들고 있는 수확물을 창고에 맡기고 다시 `Idle`로 돌아간다.
- `IsTargetValid()`: `_targetCell`이 null이 아니고 Unity 파괴 객체가 아니며 `FarmlandManager`에 등록된 셀인지 확인한다.
- `MoveTowards(Vector3 target)`: 목표 위치로 이동하고, `InteractionRange` 안에 도착했는지 반환한다.
- `FindByState(IReadOnlyList<IFarmCell> cells, FarmCellState state)`: 특정 상태의 첫 번째 셀을 찾는다. 현재 코드에서는 사용되지 않는다.
- `FindFirstWhere(IReadOnlyList<IFarmCell> cells, Func<IFarmCell, bool> predicate)`: 조건을 만족하는 첫 번째 셀을 찾는다.

### 4.4 Manager/FarmlandManager.cs

`FarmlandManager`는 2x2 농지 셀 배열을 관리하는 싱글톤이다.

주요 필드:

- `_cells`: Inspector에서 연결하는 실제 `FarmCell` 배열.
- `_farmCells`: 외부 노출용 `IFarmCell` 캐시 배열.
- `_selectedIndex`: 현재 선택된 셀 인덱스.

함수 역할:

- `Awake()`: 싱글톤 중복을 방지하고 `_cells` 배열을 `IFarmCell[]`로 캐시한다.
- `Start()`: 초기 선택 하이라이트를 적용한다.
- `SelectCell(int index)`: 선택 인덱스를 변경하고 하이라이트를 갱신한다. 현재 private이므로 외부 입력과 연결되어 있지는 않다.
- `RefreshHighlight()`: 모든 셀에 선택 여부를 전달해 하이라이트 상태를 갱신한다.
- `SelectedCell()`: 현재 선택된 셀을 반환한다. 현재 코드에서는 사용되지 않는다.
- `TryGetCellPosition(IFarmCell cell, out Vector3 position)`: 전달된 인터페이스 셀이 실제 `_cells` 배열의 어느 객체인지 찾아 월드 좌표를 반환한다.

### 4.5 Manager/WarehouseManager.cs

`WarehouseManager`는 수확물을 보관하는 싱글톤이며 `IWarehouse`를 구현한다.

주요 필드:

- `_storedCount`: 창고에 누적된 총 수확량.

함수와 프로퍼티 역할:

- `Position`: 창고 위치를 반환한다. 현재 값은 `(4, -2, 0)`으로 하드코딩되어 있다.
- `Awake()`: 싱글톤 중복을 방지한다.
- `Deposit(int amount)`: 수확량을 누적하고 로그를 출력한다.

### 4.6 Interface/IFarmCell.cs

농지 셀을 외부에서 조작하기 위한 계약이다. `FarmerAI`와 향후 플레이어 상호작용은 구체 `FarmCell` 대신 이 인터페이스를 통해 셀을 다루는 구조다.

멤버 역할:

- `State`: 현재 농지 상태.
- `WorkProgress`: 현재 작업 진행도.
- `MaxWork`: 현재 상태의 목표 작업량.
- `HarvestCount`: 현재 사이클의 누적 수확 횟수.
- `CanTill()`: 경작 가능 여부.
- `CanPlant()`: 파종 가능 여부.
- `CanHelpGrow()`: 성장 보조 가능 여부.
- `CanHarvest()`: 수확 가능 여부.
- `RegisterFarmer()`: 작업 농부 등록.
- `UnregisterFarmer()`: 작업 농부 해제.
- `ContributeWork(float amount)`: 작업 기여량 전달.
- `TryConsumeHarvest(out int yield)`: 수확물 수령 시도.
- `SetHighlight(bool highlight)`: 선택 하이라이트 표시 여부 설정.

### 4.7 Interface/IWarehouse.cs

창고를 외부에서 사용하기 위한 계약이다.

멤버 역할:

- `Position`: 창고 월드 좌표.
- `Deposit(int amount)`: 수확물 보관.

### 4.8 Data/FarmCellConfig.cs

농지 셀의 작업량, 성장 속도, 농부 효율, 수확 설정, 상태 색상을 담는 ScriptableObject다.

주요 설정:

- `TillingMaxWork`, `PlantingMaxWork`, `SeededMaxWork`, `GrowingMaxWork`, `HarvestingMaxWork`: 상태별 목표 작업량.
- `NaturalGrowthRate`: 자연 성장 속도.
- `FarmerEfficiencyCurve`: 동시에 작업하는 농부 수에 따른 총 효율 곡선.
- `MaxEffectiveFarmers`: 효율 곡선 계산에 반영할 최대 농부 수.
- `MaxHarvestCount`: 한 사이클에서 가능한 최대 수확 횟수.
- `HarvestYield`: 수확 1회당 산출량.
- `UntilledColor`, `TilledColor`, `SeededColor`, `GrowingColor`, `GrownColor`: 상태별 표시 색.

함수 역할:

- `OnValidate()`: Inspector 값 변경 시 `Normalize()`를 호출한다.
- `Normalize()`: 수치가 너무 낮거나 잘못된 값이 되지 않도록 최소값을 보정하고, 효율 곡선이 비어 있으면 기본 곡선을 넣는다.

### 4.9 Data/FarmerConfig.cs

농부 AI의 이동, 작업, 행동 선택 가중치, 최근 행동 패널티 설정을 담는 ScriptableObject다.

주요 설정:

- `MoveSpeed`: 이동 속도.
- `InteractionRange`: 목표 도착 판정 거리.
- `WorkContributionPerSecond`: 초당 작업 기여량.
- `LoiterBaseWeight`, `TillBaseWeight`, `PlantBaseWeight`, `HelpGrowBaseWeight`, `HarvestBaseWeight`: 행동 선택 기본 가중치.
- `LoiterMinSeconds`, `LoiterMaxSeconds`: 배회 시간 범위.
- `RecencyPenaltyFactor`: 최근 반복 행동의 점수를 낮추는 계수.
- `RecencyHistorySize`: 최근 행동 기록 개수.
- `AvailableTargetMultiplier`: 유효 대상이 있는 행동에 곱하는 배율.

함수 역할:

- `OnValidate()`: Inspector 값 변경 시 `Normalize()`를 호출한다.
- `Normalize()`: 이동, 작업, 시간, 기록 크기, 패널티 값이 유효 범위에 머물도록 보정한다.

### 4.10 Data/SeedConfig.cs

작물 종류에 대한 데이터 ScriptableObject다.

주요 설정:

- `DisplayName`: 작물 표시 이름.
- `HarvestYield`: 작물별 수확량 메타데이터.
- `Icon`: 작물 아이콘.

현재 런타임 로직에서는 `FarmCell._dedicatedSeed != null` 여부만 `CanPlant()` 조건으로 사용한다. `SeedConfig.HarvestYield`는 아직 실제 수확량 계산에 반영되지 않고, `FarmCellConfig.HarvestYield`가 사용된다.

### 4.11 Enum/FarmCellState.cs

농지 셀 상태를 정의한다.

- `Untilled`: 경작 전.
- `Tilled`: 경작 완료, 파종 가능.
- `Seeded`: 파종 완료, 자연 성장 중.
- `Growing`: 성장 중, 농부 성장 보조 가능.
- `Grown`: 성장 완료, 수확 가능.

### 4.12 Enum/FarmerState.cs

농부 AI 상태를 정의한다.

- `Idle`: 다음 행동 선택.
- `Loitering`: 배회 또는 대기.
- `MovingToCell`: 목표 농지 셀로 이동.
- `Tilling`: 경작 작업.
- `Planting`: 파종 작업.
- `HelpingGrow`: 성장 보조 작업.
- `Harvesting`: 수확 작업.
- `MovingToWarehouse`: 창고로 이동.
- `Depositing`: 수확물 보관.

## 5. 중요한 구현 포인트

- `FarmCell.ContributeWork()`는 `amount * efficiency / _activeFarmerCount`를 누적한다. 여기서 `efficiency`는 `FarmerEfficiencyCurve.Evaluate(min(activeCount, MaxEffectiveFarmers))`이다.
- `Seeded` 상태는 자연 성장만 받고 농부의 작업 기여는 무시한다.
- `Growing` 상태는 자연 성장과 농부 성장 보조가 모두 가능하다.
- `TransitionTo()`는 작업 상태 진입과 이탈 시 `RegisterFarmer()`와 `UnregisterFarmer()`를 자동 처리한다.
- 수확은 작업 완료 시 즉시 수확량이 생기고, `TryConsumeHarvest()`를 처음 호출한 농부만 해당 수확량을 받을 수 있다.
- `FarmlandManager.SelectCell()`과 `SelectedCell()`은 현재 private이며 입력 시스템과 연결되어 있지 않다.
- `FarmerBehaviors.FindByState()`는 현재 사용되지 않는다.
- `WarehouseManager.Position`은 씬 오브젝트의 실제 transform 위치가 아니라 하드코딩 좌표를 반환한다.
- `SeedConfig.HarvestYield`는 아직 실제 수확량 계산에 사용되지 않는다.

## 6. 확장 시 참고

- 새 농부 행동을 추가하려면 `FarmerState`에 상태를 추가하고, `FarmerAI.BuildActionCandidates()`, `FarmerAI.Update()` switch, `FarmerBehaviors`의 Handle 메서드를 함께 확장해야 한다.
- 새 농지 작업 단계를 추가하려면 `FarmCellState`, `FarmCell.GetMaxWorkForState()`, `FarmCell.AutoTransition()`, `FarmCell.GetStateColor()`, `IFarmCell` 가능 행동 API, `FarmerAI` 후보 생성 로직을 함께 검토해야 한다.
- 작물별 수확량을 적용하려면 `FarmCell.AutoTransition()`의 `_lastHarvestYield = _config.HarvestYield` 부분에서 `_dedicatedSeed.HarvestYield`를 반영할지 정책을 정해야 한다.
- 창고 위치를 씬 배치와 동기화하려면 `WarehouseManager.Position`을 `transform.position` 기반으로 바꾸는 것이 자연스럽다.
