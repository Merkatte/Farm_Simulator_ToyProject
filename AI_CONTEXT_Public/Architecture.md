# Project_FAD — Architecture

Unity 6000.3.9f1 (URP 17.3) 기반 2D 농경 + AI 에이전트 협력 게임.

---

## 폴더 구조

```
Project_FAD/
├── Assets/
│   ├── Scripts/
│   │   ├── Enum/           # 프로젝트 전체 열거형
│   │   ├── Interface/      # 프로젝트 전체 인터페이스
│   │   ├── Data/           # ScriptableObject 정의
│   │   ├── Actor/          # 게임 오브젝트 로직 (Player, Enemy, FarmCell 등)
│   │   ├── Manager/        # Singleton Manager (시스템 관리)
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
Till()             — Untilled → Tilled
PlantSeed(SeedConfig seed) — Tilled → Seeded. 씨앗을 넘겨받아 성장 타이머 시작
Boost()            — 성장 타이머 감소 (BoostReductionSeconds만큼)
SetHighlight(bool) — 선택 하이라이트 시각 처리
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
2x2 FarmCell 배열을 관리하고 테스트용 키보드 입력을 처리한다. 입력은 New Input System(`Keyboard.current`) 기반이다.

- `[SerializeField] FarmCell[] _cells` — Inspector에서 4개 셀 연결 (직렬화 목적)
- `[SerializeField] SeedConfig _seedConfig` — Inspector에서 심을 씨앗 에셋 할당
- `IFarmCell[] _farmCells` — Awake()에서 캐시, 이후 모든 게임 로직은 인터페이스로 호출
- `_selectedIndex` — 현재 선택된 셀 인덱스
- `HandleCellSelection_Test()` / `HandleCellAction_Test()` — **TEST ONLY**, AI/플레이어 시스템 구현 후 제거 예정

```
키 입력 (TEST ONLY):
  1 / 2 / 3 / 4  — 셀 선택 (선택된 셀 하이라이트)
  Q              — Till() 호출
  W              — PlantSeed(_seedConfig) 호출
  E              — Boost() 호출
```

### 의존 관계

```
FarmlandManager
    │ (SerializeField 직렬화)
    ├─ FarmCell[] _cells
    ├─ SeedConfig _seedConfig ──────────────────────┐
    │                                               │
    │ (IFarmCell 인터페이스로만 호출)                  │
    └─ IFarmCell[] _farmCells                       │
           │                                        │
           └─ FarmCell (IFarmCell 구현)              │
                  │                                 │
                  ├─ FarmCellConfig (SO, 칸 설정)    │
                  └─ SeedConfig _plantedSeed ←──────┘
                     (PlantSeed 호출 시 주입)
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

## 미구현 예정 시스템

- **AI 에이전트 캐릭터**: `IFarmCell`을 통해 Till/PlantSeed/Boost 호출. Actor 레이어에 추가 예정.
- **플레이어 상호작용**: 클릭 또는 이동 기반 입력. 디버그 키 입력 대체.
- **애니메이션**: 물 주기, 잡초 제거, 비료 주기 등 모션.
- **UI**: 상태 표시 HUD.
