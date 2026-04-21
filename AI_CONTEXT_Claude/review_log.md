# Codex 리뷰 로그

Claude가 Codex에게 보낸 프롬프트와 Codex의 응답을 회차별로 기록합니다.

## 1회차 리뷰 — 2026-04-15 20:02:44 (✅ 승인 (REVIEW_DONE))

### Claude → Codex 프롬프트

```
다음 코드를 한국어로 리뷰해주세요. 현재 1번째 리뷰입니다.

## 리뷰 기준
1. 버그 및 잠재적 오류
2. 성능 개선 가능 부분
3. 가독성 및 코드 구조
4. 모범 사례(Best Practice) 준수 여부
5. 이전 리뷰에서 지적된 사항이 반영되었는지 확인

## 중요 규칙
- 개선점이 없으면 응답 **마지막 줄**에 정확히 아래 문자열만 단독으로 출력하세요:
  REVIEW_DONE: NO_FURTHER_IMPROVEMENTS
- 개선점이 있으면 번호 목록으로 구체적으로 설명하세요.

## 요구사항/맥락
테스트 목적으로 PlayerMovement.cs에 Debug.Log 추가. Awake 초기화, 대시 시작/종료, 쿨다운 종료 시점에 로그 출력. 간단하게만 유지.

## 리뷰 대상 코드

### 파일: Assets/Scripts/PlayerMovement.cs
```
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Min(0f)] [SerializeField] private float moveSpeed = 5f;
    [Min(0f)] [SerializeField] private float dashSpeed = 20f;
    [Min(0.01f)] [SerializeField] private float dashDuration = 0.2f;
    [Min(0f)] [SerializeField] private float dashCooldown = 1.0f;

    private Rigidbody rb;
    private Vector3 moveDirection;

    // 대시 시작 시점의 방향 스냅샷 — 대시 중에는 입력이 바뀌어도 이 방향을 유지한다
    private Vector3 dashDirectionSnapshot;
    private bool isDashing;
    private bool isOnCooldown;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        Debug.Log("[PlayerMovement] Awake: Rigidbody 초기화 완료");
    }

    private void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        // 월드 축 기준 이동 방향 (카메라 연동 없는 단순 이동 전제)
        moveDirection = new Vector3(h, 0f, v).normalized;

        if (Input.GetKeyDown(KeyCode.Space) && !isDashing && !isOnCooldown)
        {
            // 입력 방향이 없으면 오브젝트 전방을 수평면에 투영해 대시 (Y성분 제거)
            dashDirectionSnapshot = moveDirection.sqrMagnitude > 0f
                ? moveDirection
                : Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;

            Debug.Log($"[PlayerMovement] 대시 시작 — 방향: {dashDirectionSnapshot}, 속도: {dashSpeed}");
            StartCoroutine(Dash());
        }
    }

    private void FixedUpdate()
    {
        // MovePosition 방식: 물리 시뮬레이션보다 위치를 직접 제어 (단순 이동용)
        if (isDashing)
            rb.MovePosition(rb.position + dashDirectionSnapshot * dashSpeed * Time.fixedDeltaTime);
        else
            rb.MovePosition(rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime);
    }

    private IEnumerator Dash()
    {
        isDashing = true;
        isOnCooldown = true;

        yield return new WaitForSeconds(dashDuration);

        isDashing = false;
        Debug.Log("[PlayerMovement] 대시 종료 — 일반 이동으로 복귀");

        yield return new WaitForSeconds(dashCooldown);

        isOnCooldown = false;
        Debug.Log("[PlayerMovement] 쿨다운 종료 — 대시 재사용 가능");
    }
}

```

```

### Codex 응답

**Key Findings**냥

현재 요구사항 범위에서는 유의미한 개선점이 없습니다냥  
`Awake` 초기화, 대시 시작/종료, 쿨다운 종료 시점에만 로그를 넣어 로그량을 최소화했고, 기능 흐름도 기존 로직을 해치지 않습니다냥  
`Update()`에서 대시 시작 직전에만 로그를 출력하고, `Dash()` 코루틴에서 종료 시점들을 분리해 기록한 구조도 단순하고 읽기 쉽습니다냥  
이전 리뷰 반영 여부는 현재 1번째 리뷰이므로 확인 대상이 없습니다냥

REVIEW_DONE: NO_FURTHER_IMPROVEMENTS

---

## 1회차 리뷰 — 2026-04-20 01:48:01 (🔄 개선 필요)

### Claude → Codex 프롬프트

```
다음 코드를 한국어로 리뷰해주세요. 현재 1번째 리뷰입니다.

## 리뷰 기준
1. 버그 및 잠재적 오류
2. 성능 개선 가능 부분
3. 가독성 및 코드 구조
4. 모범 사례(Best Practice) 준수 여부
5. 이전 리뷰에서 지적된 사항이 반영되었는지 확인

## 중요 규칙
- 개선점이 없으면 응답 **마지막 줄**에 정확히 아래 문자열만 단독으로 출력하세요:
  REVIEW_DONE: NO_FURTHER_IMPROVEMENTS
- 개선점이 있으면 번호 목록으로 구체적으로 설명하세요.

## 요구사항/맥락
2x2 농경지 프로토타입: FarmCellState enum, IFarmCell interface, FarmCellConfig ScriptableObject, FarmCell MonoBehaviour(상태머신+시간성장), FarmlandManager Singleton(키보드디버그). 컨벤션: K&R괄호, _camelCase private, PascalCase public, SO 데이터 외부화, Interface DI.

## 리뷰 대상 코드

### 파일: Assets/Scripts/Enum/FarmCellState.cs
```
public enum FarmCellState {
    Untilled,
    Tilled,
    Seeded,
    Growing,
    Grown
}

```

### 파일: Assets/Scripts/Interface/IFarmCell.cs
```
public interface IFarmCell {
    FarmCellState State { get; }
    bool CanTill();
    bool CanPlant();
    bool CanBoost();
    void Till();
    void PlantSeed();
    void Boost();
}

```

### 파일: Assets/Scripts/Data/FarmCellConfig.cs
```
using UnityEngine;

[CreateAssetMenu(fileName = "FarmCellConfig", menuName = "Project_FAD/FarmCellConfig")]
public class FarmCellConfig : ScriptableObject {
    [Header("Growth Timings (seconds)")]
    public float SeededToGrowingSeconds = 5f;
    public float GrowingToGrownSeconds = 10f;
    public float BoostReductionSeconds = 2f;

    [Header("State Colors")]
    public Color UntilledColor = new Color(0.55f, 0.40f, 0.22f);
    public Color TilledColor   = new Color(0.35f, 0.22f, 0.10f);
    public Color SeededColor   = new Color(0.70f, 0.65f, 0.30f);
    public Color GrowingColor  = new Color(0.40f, 0.75f, 0.30f);
    public Color GrownColor    = new Color(0.10f, 0.55f, 0.10f);
}

```

### 파일: Assets/Scripts/Actor/FarmCell.cs
```
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class FarmCell : MonoBehaviour, IFarmCell {
    [SerializeField] private FarmCellConfig _config;

    private SpriteRenderer _spriteRenderer;
    private FarmCellState _state = FarmCellState.Untilled;
    private float _growthTimer;

    public FarmCellState State => _state;

    private void Awake() {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        ApplyStateVisual();
    }

    private void Update() {
        if (_state != FarmCellState.Seeded && _state != FarmCellState.Growing) return;

        _growthTimer -= Time.deltaTime;
        if (_growthTimer <= 0f) {
            AdvanceGrowth();
        }
    }

    public bool CanTill()  => _state == FarmCellState.Untilled;
    public bool CanPlant() => _state == FarmCellState.Tilled;
    public bool CanBoost() => _state == FarmCellState.Seeded || _state == FarmCellState.Growing;

    public void Till() {
        if (!CanTill()) return;
        SetState(FarmCellState.Tilled);
    }

    public void PlantSeed() {
        if (!CanPlant()) return;
        _growthTimer = _config.SeededToGrowingSeconds;
        SetState(FarmCellState.Seeded);
    }

    public void Boost() {
        if (!CanBoost()) return;
        _growthTimer -= _config.BoostReductionSeconds;
        if (_growthTimer <= 0f) {
            AdvanceGrowth();
        }
    }

    public void SetHighlight(bool highlight) {
        _spriteRenderer.color = highlight
            ? Color.Lerp(GetStateColor(), Color.white, 0.4f)
            : GetStateColor();
    }

    private void AdvanceGrowth() {
        if (_state == FarmCellState.Seeded) {
            _growthTimer = _config.GrowingToGrownSeconds;
            SetState(FarmCellState.Growing);
        } else if (_state == FarmCellState.Growing) {
            SetState(FarmCellState.Grown);
        }
    }

    private void SetState(FarmCellState newState) {
        _state = newState;
        ApplyStateVisual();
    }

    private void ApplyStateVisual() {
        if (_spriteRenderer == null) return;
        _spriteRenderer.color = GetStateColor();
    }

    private Color GetStateColor() {
        return _state switch {
            FarmCellState.Untilled => _config.UntilledColor,
            FarmCellState.Tilled   => _config.TilledColor,
            FarmCellState.Seeded   => _config.SeededColor,
            FarmCellState.Growing  => _config.GrowingColor,
            FarmCellState.Grown    => _config.GrownColor,
            _                      => Color.white
        };
    }
}

```

### 파일: Assets/Scripts/Manager/FarmlandManager.cs
```
using UnityEngine;

public class FarmlandManager : MonoBehaviour {
    public static FarmlandManager Instance { get; private set; }

    [SerializeField] private FarmCell[] _cells = new FarmCell[4];

    private int _selectedIndex = 0;

    private void Awake() {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start() {
        RefreshHighlight();
    }

    private void Update() {
        HandleCellSelection();
        HandleCellAction();
    }

    private void HandleCellSelection() {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectCell(0);
        else if (Input.GetKeyDown(KeyCode.Alpha2)) SelectCell(1);
        else if (Input.GetKeyDown(KeyCode.Alpha3)) SelectCell(2);
        else if (Input.GetKeyDown(KeyCode.Alpha4)) SelectCell(3);
    }

    private void HandleCellAction() {
        FarmCell cell = SelectedCell();
        if (cell == null) return;

        if (Input.GetKeyDown(KeyCode.Q)) cell.Till();
        else if (Input.GetKeyDown(KeyCode.W)) cell.PlantSeed();
        else if (Input.GetKeyDown(KeyCode.E)) cell.Boost();
    }

    private void SelectCell(int index) {
        if (index < 0 || index >= _cells.Length) return;
        _selectedIndex = index;
        RefreshHighlight();
    }

    private void RefreshHighlight() {
        for (int i = 0; i < _cells.Length; i++) {
            if (_cells[i] != null) {
                _cells[i].SetHighlight(i == _selectedIndex);
            }
        }
    }

    private FarmCell SelectedCell() {
        if (_selectedIndex < 0 || _selectedIndex >= _cells.Length) return null;
        return _cells[_selectedIndex];
    }
}

```

```

### Codex 응답

**Key Findings냥**

1. `Assets/Scripts/Actor/FarmCell.cs:5`, `:28`, `:35`, `:56`에서 `_config`가 할당되지 않은 경우 `Awake()`의 `ApplyStateVisual()` 또는 `PlantSeed()`, `Boost()`, `GetStateColor()`에서 즉시 `NullReferenceException`이 발생할 수 있습니다냥. `FarmCellConfig`는 필수 의존성이므로 `Awake()`에서 null 검증 후 비활성화하거나, 에디터 검증(`OnValidate`)과 명확한 에러 로그를 추가하는 것이 안전합니다냥.

2. `Assets/Scripts/Actor/FarmCell.cs:48`의 `SetState()`가 항상 `ApplyStateVisual()`을 호출해서 선택된 셀의 하이라이트가 행동 직후 사라집니다냥. 예를 들어 선택된 셀에서 `Q/W/E`를 누르면 상태 색상으로 덮어써지고, 다시 숫자키를 눌러 선택을 갱신하기 전까지 선택 표시가 유지되지 않습니다냥. `_isHighlighted` 상태를 `FarmCell` 내부에 저장하고 `ApplyStateVisual()`에서 하이라이트까지 함께 반영하거나, 상태 변경 후 매니저가 `RefreshHighlight()`를 호출하도록 구조를 맞추는 편이 좋습니다냥.

3. `Assets/Scripts/Manager/FarmlandManager.cs:5`, `:38`, `:48`에서 매니저가 `FarmCell` 구체 타입에 직접 의존하고 있어 요구사항의 `Interface DI`와 다소 어긋납니다냥. 행동 호출은 `IFarmCell`을 통해 처리하고, 하이라이트가 필요하다면 별도 뷰 인터페이스를 두거나 `IFarmCell`에 선택 표시 책임을 포함할지 명확히 분리하는 것이 구조적으로 더 낫습니다냥.

4. `Assets/Scripts/Actor/FarmCell.cs:35`와 `:41`의 성장 처리에서 부스트나 큰 `deltaTime`으로 타이머가 크게 음수가 되어도 한 단계만 진행하고 초과 감소분은 버려집니다냥. 현재 값이 의도라면 문제 없지만, “시간 단축” 의미라면 남은 초과 시간을 다음 성장 단계에 이월하거나 반복 advance 처리를 해야 예측 가능한 성장 속도가 됩니다냥.

5. 이전 리뷰는 현재 1번째 리뷰이므로 반영 여부를 비교할 이전 지적사항은 없습니다냥.

---

## 2회차 리뷰 — 2026-04-20 01:50:34 (🔄 개선 필요)

### Claude → Codex 프롬프트

```
다음 코드를 한국어로 리뷰해주세요. 현재 2번째 리뷰입니다.

## 리뷰 기준
1. 버그 및 잠재적 오류
2. 성능 개선 가능 부분
3. 가독성 및 코드 구조
4. 모범 사례(Best Practice) 준수 여부
5. 이전 리뷰에서 지적된 사항이 반영되었는지 확인

## 중요 규칙
- 개선점이 없으면 응답 **마지막 줄**에 정확히 아래 문자열만 단독으로 출력하세요:
  REVIEW_DONE: NO_FURTHER_IMPROVEMENTS
- 개선점이 있으면 번호 목록으로 구체적으로 설명하세요.

## 요구사항/맥락
1회차 리뷰 반영: IFarmCell에 SetHighlight 추가, FarmCell에 _isHighlighted 플래그+null가드 추가, Update/Boost에 while루프 처리, FarmlandManager HandleCellAction에서 IFarmCell 반환 및 행동 후 RefreshHighlight 호출.

## 이전 리뷰 히스토리

### 1회차 리뷰 (2026-04-20 01:48:01)
**Key Findings냥**

1. `Assets/Scripts/Actor/FarmCell.cs:5`, `:28`, `:35`, `:56`에서 `_config`가 할당되지 않은 경우 `Awake()`의 `ApplyStateVisual()` 또는 `PlantSeed()`, `Boost()`, `GetStateColor()`에서 즉시 `NullReferenceException`이 발생할 수 있습니다냥. `FarmCellConfig`는 필수 의존성이므로 `Awake()`에서 null 검증 후 비활성화하거나, 에디터 검증(`OnValidate`)과 명확한 에러 로그를 추가하는 것이 안전합니다냥.

2. `Assets/Scripts/Actor/FarmCell.cs:48`의 `SetState()`가 항상 `ApplyStateVisual()`을 호출해서 선택된 셀의 하이라이트가 행동 직후 사라집니다냥. 예를 들어 선택된 셀에서 `Q/W/E`를 누르면 상태 색상으로 덮어써지고, 다시 숫자키를 눌러 선택을 갱신하기 전까지 선택 표시가 유지되지 않습니다냥. `_isHighlighted` 상태를 `FarmCell` 내부에 저장하고 `ApplyStateVisual()`에서 하이라이트까지 함께 반영하거나, 상태 변경 후 매니저가 `RefreshHighlight()`를 호출하도록 구조를 맞추는 편이 좋습니다냥.

3. `Assets/Scripts/Manager/FarmlandManager.cs:5`, `:38`, `:48`에서 매니저가 `FarmCell` 구체 타입에 직접 의존하고 있어 요구사항의 `Interface DI`와 다소 어긋납니다냥. 행동 호출은 `IFarmCell`을 통해 처리하고, 하이라이트가 필요하다면 별도 뷰 인터페이스를 두거나 `IFarmCell`에 선택 표시 책임을 포함할지 명확히 분리하는 것이 구조적으로 더 낫습니다냥.

4. `Assets/Scripts/Actor/FarmCell.cs:35`와 `:41`의 성장 처리에서 부스트나 큰 `deltaTime`으로 타이머가 크게 음수가 되어도 한 단계만 진행하고 초과 감소분은 버려집니다냥. 현재 값이 의도라면 문제 없지만, “시간 단축” 의미라면 남은 초과 시간을 다음 성장 단계에 이월하거나 반복 advance 처리를 해야 예측 가능한 성장 속도가 됩니다냥.

5. 이전 리뷰는 현재 1번째 리뷰이므로 반영 여부를 비교할 이전 지적사항은 없습니다냥.

## 리뷰 대상 코드

### 파일: Assets/Scripts/Enum/FarmCellState.cs
```
public enum FarmCellState {
    Untilled,
    Tilled,
    Seeded,
    Growing,
    Grown
}

```

### 파일: Assets/Scripts/Interface/IFarmCell.cs
```
public interface IFarmCell {
    FarmCellState State { get; }
    bool CanTill();
    bool CanPlant();
    bool CanBoost();
    void Till();
    void PlantSeed();
    void Boost();
    void SetHighlight(bool highlight);
}

```

### 파일: Assets/Scripts/Data/FarmCellConfig.cs
```
using UnityEngine;

[CreateAssetMenu(fileName = "FarmCellConfig", menuName = "Project_FAD/FarmCellConfig")]
public class FarmCellConfig : ScriptableObject {
    [Header("Growth Timings (seconds)")]
    public float SeededToGrowingSeconds = 5f;
    public float GrowingToGrownSeconds = 10f;
    public float BoostReductionSeconds = 2f;

    [Header("State Colors")]
    public Color UntilledColor = new Color(0.55f, 0.40f, 0.22f);
    public Color TilledColor   = new Color(0.35f, 0.22f, 0.10f);
    public Color SeededColor   = new Color(0.70f, 0.65f, 0.30f);
    public Color GrowingColor  = new Color(0.40f, 0.75f, 0.30f);
    public Color GrownColor    = new Color(0.10f, 0.55f, 0.10f);
}

```

### 파일: Assets/Scripts/Actor/FarmCell.cs
```
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class FarmCell : MonoBehaviour, IFarmCell {
    [SerializeField] private FarmCellConfig _config;

    private SpriteRenderer _spriteRenderer;
    private FarmCellState _state = FarmCellState.Untilled;
    private float _growthTimer;
    private bool _isHighlighted;

    public FarmCellState State => _state;

    private void Awake() {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_config == null) {
            Debug.LogError($"[FarmCell] {name}: FarmCellConfig이 할당되지 않았습니다.", this);
            return;
        }
        ApplyStateVisual();
    }

    private void Update() {
        if (_state != FarmCellState.Seeded && _state != FarmCellState.Growing) return;

        _growthTimer -= Time.deltaTime;
        while (_growthTimer <= 0f && (_state == FarmCellState.Seeded || _state == FarmCellState.Growing)) {
            AdvanceGrowth();
        }
    }

    public bool CanTill()  => _state == FarmCellState.Untilled;
    public bool CanPlant() => _state == FarmCellState.Tilled;
    public bool CanBoost() => _state == FarmCellState.Seeded || _state == FarmCellState.Growing;

    public void Till() {
        if (!CanTill()) return;
        SetState(FarmCellState.Tilled);
    }

    public void PlantSeed() {
        if (!CanPlant()) return;
        _growthTimer = _config.SeededToGrowingSeconds;
        SetState(FarmCellState.Seeded);
    }

    public void Boost() {
        if (!CanBoost()) return;
        _growthTimer -= _config.BoostReductionSeconds;
        while (_growthTimer <= 0f && (_state == FarmCellState.Seeded || _state == FarmCellState.Growing)) {
            AdvanceGrowth();
        }
    }

    public void SetHighlight(bool highlight) {
        _isHighlighted = highlight;
        ApplyStateVisual();
    }

    private void AdvanceGrowth() {
        if (_state == FarmCellState.Seeded) {
            _growthTimer = _config.GrowingToGrownSeconds;
            SetState(FarmCellState.Growing);
        } else if (_state == FarmCellState.Growing) {
            SetState(FarmCellState.Grown);
        }
    }

    private void SetState(FarmCellState newState) {
        _state = newState;
        ApplyStateVisual();
    }

    private void ApplyStateVisual() {
        if (_spriteRenderer == null || _config == null) return;
        Color baseColor = GetStateColor();
        _spriteRenderer.color = _isHighlighted ? Color.Lerp(baseColor, Color.white, 0.4f) : baseColor;
    }

    private Color GetStateColor() {
        return _state switch {
            FarmCellState.Untilled => _config.UntilledColor,
            FarmCellState.Tilled   => _config.TilledColor,
            FarmCellState.Seeded   => _config.SeededColor,
            FarmCellState.Growing  => _config.GrowingColor,
            FarmCellState.Grown    => _config.GrownColor,
            _                      => Color.white
        };
    }
}

```

### 파일: Assets/Scripts/Manager/FarmlandManager.cs
```
using UnityEngine;

public class FarmlandManager : MonoBehaviour {
    public static FarmlandManager Instance { get; private set; }

    [SerializeField] private FarmCell[] _cells = new FarmCell[4];

    private int _selectedIndex = 0;

    private void Awake() {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start() {
        RefreshHighlight();
    }

    private void Update() {
        HandleCellSelection();
        HandleCellAction();
    }

    private void HandleCellSelection() {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectCell(0);
        else if (Input.GetKeyDown(KeyCode.Alpha2)) SelectCell(1);
        else if (Input.GetKeyDown(KeyCode.Alpha3)) SelectCell(2);
        else if (Input.GetKeyDown(KeyCode.Alpha4)) SelectCell(3);
    }

    private void HandleCellAction() {
        IFarmCell cell = SelectedCell();
        if (cell == null) return;

        bool acted = false;
        if (Input.GetKeyDown(KeyCode.Q))      { cell.Till();      acted = true; }
        else if (Input.GetKeyDown(KeyCode.W)) { cell.PlantSeed(); acted = true; }
        else if (Input.GetKeyDown(KeyCode.E)) { cell.Boost();     acted = true; }

        if (acted) RefreshHighlight();
    }

    private void SelectCell(int index) {
        if (index < 0 || index >= _cells.Length) return;
        _selectedIndex = index;
        RefreshHighlight();
    }

    private void RefreshHighlight() {
        for (int i = 0; i < _cells.Length; i++) {
            if (_cells[i] != null) {
                ((IFarmCell)_cells[i]).SetHighlight(i == _selectedIndex);
            }
        }
    }

    private IFarmCell SelectedCell() {
        if (_selectedIndex < 0 || _selectedIndex >= _cells.Length) return null;
        return _cells[_selectedIndex];
    }
}

```

```

### Codex 응답

**Key Findings냥**

1. [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:16)의 `_config == null` 처리는 `Awake()`에서 로그만 남기고 끝나므로, 이후 `PlantSeed()`가 호출되면 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:43)에서 `_config.SeededToGrowingSeconds` 접근으로 여전히 `NullReferenceException`이 발생할 수 있습니다냥. 1회차의 null 가드 요구는 `ApplyStateVisual()` 쪽에는 반영됐지만, 공개 액션 메서드 전체에는 아직 완전히 반영되지 않았습니다냥. `Awake()`에서 `enabled = false`로 컴포넌트를 비활성화하거나, `PlantSeed()`, `Boost()`, `AdvanceGrowth()` 진입 전에 `_config` 유효성을 보장하는 처리가 필요합니다냥.

2. [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:26)와 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:49)의 `while` 루프는 추가됐지만, 초과 시간을 다음 성장 단계로 이월하지 못합니다냥. 예를 들어 Seeded 상태에서 `_growthTimer`가 `-3f`가 된 경우 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:62)에서 `GrowingToGrownSeconds`로 덮어써져 3초 초과분이 사라집니다냥. 1회차에서 지적된 “부스트나 큰 deltaTime의 초과 감소분 처리”는 루프만으로는 해결되지 않았으므로, `AdvanceGrowth()`가 남은 초과 시간을 받아 다음 타이머에서 차감하도록 구조를 바꾸는 편이 맞습니다냥.

3. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:6)의 `_cells`가 여전히 `FarmCell[]` 구체 타입이라 1회차의 Interface DI 지적은 부분 반영 상태입니다냥. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:32)에서 선택 셀을 `IFarmCell`로 받는 점은 개선됐지만, 직렬화 필드와 [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:52)의 캐스팅은 여전히 구현체에 묶여 있습니다냥. Unity 직렬화 제약을 고려하면 `MonoBehaviour[]`로 받고 `IFarmCell` 구현 여부를 `Awake()`나 `OnValidate()`에서 검증하거나, 별도 셀 뷰/모델 인터페이스를 분리하는 방식이 더 명확합니다냥.

**반영 확인냥**

1회차 지적 중 `IFarmCell.SetHighlight()` 추가, `FarmCell`의 `_isHighlighted` 플래그와 시각 갱신 유지, 행동 후 `RefreshHighlight()` 호출은 반영됐습니다냥. 다만 null 의존성 방어, 성장 타이머 초과분 이월, 구체 타입 의존 제거는 아직 개선 여지가 남아 있습니다냥. 이번 2회차 리뷰 요약은 `AI_CONTEXT_Codex/review_log.md`에 기록했습니다냥.

---

## 3회차 리뷰 — 2026-04-20 01:52:34 (🔄 개선 필요)

### Claude → Codex 프롬프트

```
다음 코드를 한국어로 리뷰해주세요. 현재 3번째 리뷰입니다.

## 리뷰 기준
1. 버그 및 잠재적 오류
2. 성능 개선 가능 부분
3. 가독성 및 코드 구조
4. 모범 사례(Best Practice) 준수 여부
5. 이전 리뷰에서 지적된 사항이 반영되었는지 확인

## 중요 규칙
- 개선점이 없으면 응답 **마지막 줄**에 정확히 아래 문자열만 단독으로 출력하세요:
  REVIEW_DONE: NO_FURTHER_IMPROVEMENTS
- 개선점이 있으면 번호 목록으로 구체적으로 설명하세요.

## 요구사항/맥락
2회차 반영: Awake에서 _config null 시 enabled=false, AdvanceGrowth에서 overflow 이월 처리, FarmlandManager Awake에서 IFarmCell[] 캐시 배열 구성 후 캐스팅 없이 사용.

## 이전 리뷰 히스토리

### 1회차 리뷰 (2026-04-20 01:48:01)
**Key Findings냥**

1. `Assets/Scripts/Actor/FarmCell.cs:5`, `:28`, `:35`, `:56`에서 `_config`가 할당되지 않은 경우 `Awake()`의 `ApplyStateVisual()` 또는 `PlantSeed()`, `Boost()`, `GetStateColor()`에서 즉시 `NullReferenceException`이 발생할 수 있습니다냥. `FarmCellConfig`는 필수 의존성이므로 `Awake()`에서 null 검증 후 비활성화하거나, 에디터 검증(`OnValidate`)과 명확한 에러 로그를 추가하는 것이 안전합니다냥.

2. `Assets/Scripts/Actor/FarmCell.cs:48`의 `SetState()`가 항상 `ApplyStateVisual()`을 호출해서 선택된 셀의 하이라이트가 행동 직후 사라집니다냥. 예를 들어 선택된 셀에서 `Q/W/E`를 누르면 상태 색상으로 덮어써지고, 다시 숫자키를 눌러 선택을 갱신하기 전까지 선택 표시가 유지되지 않습니다냥. `_isHighlighted` 상태를 `FarmCell` 내부에 저장하고 `ApplyStateVisual()`에서 하이라이트까지 함께 반영하거나, 상태 변경 후 매니저가 `RefreshHighlight()`를 호출하도록 구조를 맞추는 편이 좋습니다냥.

3. `Assets/Scripts/Manager/FarmlandManager.cs:5`, `:38`, `:48`에서 매니저가 `FarmCell` 구체 타입에 직접 의존하고 있어 요구사항의 `Interface DI`와 다소 어긋납니다냥. 행동 호출은 `IFarmCell`을 통해 처리하고, 하이라이트가 필요하다면 별도 뷰 인터페이스를 두거나 `IFarmCell`에 선택 표시 책임을 포함할지 명확히 분리하는 것이 구조적으로 더 낫습니다냥.

4. `Assets/Scripts/Actor/FarmCell.cs:35`와 `:41`의 성장 처리에서 부스트나 큰 `deltaTime`으로 타이머가 크게 음수가 되어도 한 단계만 진행하고 초과 감소분은 버려집니다냥. 현재 값이 의도라면 문제 없지만, “시간 단축” 의미라면 남은 초과 시간을 다음 성장 단계에 이월하거나 반복 advance 처리를 해야 예측 가능한 성장 속도가 됩니다냥.

5. 이전 리뷰는 현재 1번째 리뷰이므로 반영 여부를 비교할 이전 지적사항은 없습니다냥.

### 2회차 리뷰 (2026-04-20 01:50:34)
**Key Findings냥**

1. [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:16)의 `_config == null` 처리는 `Awake()`에서 로그만 남기고 끝나므로, 이후 `PlantSeed()`가 호출되면 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:43)에서 `_config.SeededToGrowingSeconds` 접근으로 여전히 `NullReferenceException`이 발생할 수 있습니다냥. 1회차의 null 가드 요구는 `ApplyStateVisual()` 쪽에는 반영됐지만, 공개 액션 메서드 전체에는 아직 완전히 반영되지 않았습니다냥. `Awake()`에서 `enabled = false`로 컴포넌트를 비활성화하거나, `PlantSeed()`, `Boost()`, `AdvanceGrowth()` 진입 전에 `_config` 유효성을 보장하는 처리가 필요합니다냥.

2. [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:26)와 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:49)의 `while` 루프는 추가됐지만, 초과 시간을 다음 성장 단계로 이월하지 못합니다냥. 예를 들어 Seeded 상태에서 `_growthTimer`가 `-3f`가 된 경우 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:62)에서 `GrowingToGrownSeconds`로 덮어써져 3초 초과분이 사라집니다냥. 1회차에서 지적된 “부스트나 큰 deltaTime의 초과 감소분 처리”는 루프만으로는 해결되지 않았으므로, `AdvanceGrowth()`가 남은 초과 시간을 받아 다음 타이머에서 차감하도록 구조를 바꾸는 편이 맞습니다냥.

3. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:6)의 `_cells`가 여전히 `FarmCell[]` 구체 타입이라 1회차의 Interface DI 지적은 부분 반영 상태입니다냥. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:32)에서 선택 셀을 `IFarmCell`로 받는 점은 개선됐지만, 직렬화 필드와 [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:52)의 캐스팅은 여전히 구현체에 묶여 있습니다냥. Unity 직렬화 제약을 고려하면 `MonoBehaviour[]`로 받고 `IFarmCell` 구현 여부를 `Awake()`나 `OnValidate()`에서 검증하거나, 별도 셀 뷰/모델 인터페이스를 분리하는 방식이 더 명확합니다냥.

**반영 확인냥**

1회차 지적 중 `IFarmCell.SetHighlight()` 추가, `FarmCell`의 `_isHighlighted` 플래그와 시각 갱신 유지, 행동 후 `RefreshHighlight()` 호출은 반영됐습니다냥. 다만 null 의존성 방어, 성장 타이머 초과분 이월, 구체 타입 의존 제거는 아직 개선 여지가 남아 있습니다냥. 이번 2회차 리뷰 요약은 `AI_CONTEXT_Codex/review_log.md`에 기록했습니다냥.

## 리뷰 대상 코드

### 파일: Assets/Scripts/Enum/FarmCellState.cs
```
public enum FarmCellState {
    Untilled,
    Tilled,
    Seeded,
    Growing,
    Grown
}

```

### 파일: Assets/Scripts/Interface/IFarmCell.cs
```
public interface IFarmCell {
    FarmCellState State { get; }
    bool CanTill();
    bool CanPlant();
    bool CanBoost();
    void Till();
    void PlantSeed();
    void Boost();
    void SetHighlight(bool highlight);
}

```

### 파일: Assets/Scripts/Data/FarmCellConfig.cs
```
using UnityEngine;

[CreateAssetMenu(fileName = "FarmCellConfig", menuName = "Project_FAD/FarmCellConfig")]
public class FarmCellConfig : ScriptableObject {
    [Header("Growth Timings (seconds)")]
    public float SeededToGrowingSeconds = 5f;
    public float GrowingToGrownSeconds = 10f;
    public float BoostReductionSeconds = 2f;

    [Header("State Colors")]
    public Color UntilledColor = new Color(0.55f, 0.40f, 0.22f);
    public Color TilledColor   = new Color(0.35f, 0.22f, 0.10f);
    public Color SeededColor   = new Color(0.70f, 0.65f, 0.30f);
    public Color GrowingColor  = new Color(0.40f, 0.75f, 0.30f);
    public Color GrownColor    = new Color(0.10f, 0.55f, 0.10f);
}

```

### 파일: Assets/Scripts/Actor/FarmCell.cs
```
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class FarmCell : MonoBehaviour, IFarmCell {
    [SerializeField] private FarmCellConfig _config;

    private SpriteRenderer _spriteRenderer;
    private FarmCellState _state = FarmCellState.Untilled;
    private float _growthTimer;
    private bool _isHighlighted;

    public FarmCellState State => _state;

    private void Awake() {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_config == null) {
            Debug.LogError($"[FarmCell] {name}: FarmCellConfig이 할당되지 않았습니다.", this);
            enabled = false;
            return;
        }
        ApplyStateVisual();
    }

    private void Update() {
        if (_state != FarmCellState.Seeded && _state != FarmCellState.Growing) return;

        _growthTimer -= Time.deltaTime;
        while (_growthTimer <= 0f && (_state == FarmCellState.Seeded || _state == FarmCellState.Growing)) {
            AdvanceGrowth();
        }
    }

    public bool CanTill()  => _state == FarmCellState.Untilled;
    public bool CanPlant() => _state == FarmCellState.Tilled;
    public bool CanBoost() => _state == FarmCellState.Seeded || _state == FarmCellState.Growing;

    public void Till() {
        if (!CanTill()) return;
        SetState(FarmCellState.Tilled);
    }

    public void PlantSeed() {
        if (!CanPlant()) return;
        _growthTimer = _config.SeededToGrowingSeconds;
        SetState(FarmCellState.Seeded);
    }

    public void Boost() {
        if (!CanBoost()) return;
        _growthTimer -= _config.BoostReductionSeconds;
        while (_growthTimer <= 0f && (_state == FarmCellState.Seeded || _state == FarmCellState.Growing)) {
            AdvanceGrowth();
        }
    }

    public void SetHighlight(bool highlight) {
        _isHighlighted = highlight;
        ApplyStateVisual();
    }

    private void AdvanceGrowth() {
        float overflow = -_growthTimer;
        if (_state == FarmCellState.Seeded) {
            _growthTimer = Mathf.Max(0f, _config.GrowingToGrownSeconds - overflow);
            SetState(FarmCellState.Growing);
        } else if (_state == FarmCellState.Growing) {
            SetState(FarmCellState.Grown);
        }
    }

    private void SetState(FarmCellState newState) {
        _state = newState;
        ApplyStateVisual();
    }

    private void ApplyStateVisual() {
        if (_spriteRenderer == null || _config == null) return;
        Color baseColor = GetStateColor();
        _spriteRenderer.color = _isHighlighted ? Color.Lerp(baseColor, Color.white, 0.4f) : baseColor;
    }

    private Color GetStateColor() {
        return _state switch {
            FarmCellState.Untilled => _config.UntilledColor,
            FarmCellState.Tilled   => _config.TilledColor,
            FarmCellState.Seeded   => _config.SeededColor,
            FarmCellState.Growing  => _config.GrowingColor,
            FarmCellState.Grown    => _config.GrownColor,
            _                      => Color.white
        };
    }
}

```

### 파일: Assets/Scripts/Manager/FarmlandManager.cs
```
using UnityEngine;

public class FarmlandManager : MonoBehaviour {
    public static FarmlandManager Instance { get; private set; }

    [SerializeField] private FarmCell[] _cells = new FarmCell[4];

    private IFarmCell[] _farmCells;
    private int _selectedIndex = 0;

    private void Awake() {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        _farmCells = new IFarmCell[_cells.Length];
        for (int i = 0; i < _cells.Length; i++) {
            _farmCells[i] = _cells[i];
        }
    }

    private void Start() {
        RefreshHighlight();
    }

    private void Update() {
        HandleCellSelection();
        HandleCellAction();
    }

    private void HandleCellSelection() {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectCell(0);
        else if (Input.GetKeyDown(KeyCode.Alpha2)) SelectCell(1);
        else if (Input.GetKeyDown(KeyCode.Alpha3)) SelectCell(2);
        else if (Input.GetKeyDown(KeyCode.Alpha4)) SelectCell(3);
    }

    private void HandleCellAction() {
        IFarmCell cell = SelectedCell();
        if (cell == null) return;

        bool acted = false;
        if (Input.GetKeyDown(KeyCode.Q))      { cell.Till();      acted = true; }
        else if (Input.GetKeyDown(KeyCode.W)) { cell.PlantSeed(); acted = true; }
        else if (Input.GetKeyDown(KeyCode.E)) { cell.Boost();     acted = true; }

        if (acted) RefreshHighlight();
    }

    private void SelectCell(int index) {
        if (index < 0 || index >= _farmCells.Length) return;
        _selectedIndex = index;
        RefreshHighlight();
    }

    private void RefreshHighlight() {
        for (int i = 0; i < _farmCells.Length; i++) {
            _farmCells[i]?.SetHighlight(i == _selectedIndex);
        }
    }

    private IFarmCell SelectedCell() {
        if (_selectedIndex < 0 || _selectedIndex >= _farmCells.Length) return null;
        return _farmCells[_selectedIndex];
    }
}

```

```

### Codex 응답

**Key Findings냥**

1. [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:16)의 `_config == null` 처리에서 `enabled = false`를 추가한 점은 2회차 지적을 일부 반영했지만, 외부에서 `IFarmCell` 참조로 public 메서드를 호출하는 경로는 여전히 막지 못합니다냥. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:17)에서 비활성화된 `FarmCell`도 그대로 `_farmCells`에 캐시되므로, 설정이 빠진 셀도 `Q`로 `Till()` 상태 전환 후 `W` 입력 시 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:44)의 `_config.SeededToGrowingSeconds` 접근에서 `NullReferenceException`이 발생할 수 있습니다냥. `CanTill()`, `CanPlant()`, `CanBoost()`가 `_config != null`을 함께 확인하게 하거나, public 액션 메서드 진입부에서 설정 유효성을 공통 가드로 막는 처리가 필요합니다냥.

**반영 확인냥**

2회차의 성장 타이머 초과분 이월 지적은 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:62)에서 `overflow`를 계산하고 다음 단계 타이머에 반영하도록 개선되어 해결된 것으로 보입니다냥. FarmlandManager도 [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:15)에서 `IFarmCell[]` 캐시를 구성한 뒤 [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:38)부터 캐스팅 없이 사용하는 형태로 반영되었습니다냥. 이번 3회차 리뷰 요약은 `AI_CONTEXT_Codex/review_log.md`에 기록했습니다냥.

---

## 1회차 리뷰 — 2026-04-20 19:20:38 (🔄 개선 필요)

### Claude → Codex 프롬프트

```
다음 코드를 한국어로 리뷰해주세요. 현재 1번째 리뷰입니다.

## 리뷰 기준
1. 버그 및 잠재적 오류
2. 성능 개선 가능 부분
3. 가독성 및 코드 구조
4. 모범 사례(Best Practice) 준수 여부
5. 이전 리뷰에서 지적된 사항이 반영되었는지 확인

## 중요 규칙
- 개선점이 없으면 응답 **마지막 줄**에 정확히 아래 문자열만 단독으로 출력하세요:
  REVIEW_DONE: NO_FURTHER_IMPROVEMENTS
- 개선점이 있으면 번호 목록으로 구체적으로 설명하세요.

## 요구사항/맥락
Unity 2D 농경 게임 AI 농부 에이전트. 4개 밭 중 하나로 이동해 경작/파종/부스트/수확/창고 반납 사이클을 자율 수행. IFarmCell/IWarehouse 인터페이스 의존. K&R 괄호, _camelCase private, SO 데이터 외부화, 메서드 목적 주석 컨벤션 준수.

## 이전 리뷰 히스토리

### 1회차 리뷰 (2026-04-20 01:48:01)
**Key Findings냥**

1. `Assets/Scripts/Actor/FarmCell.cs:5`, `:28`, `:35`, `:56`에서 `_config`가 할당되지 않은 경우 `Awake()`의 `ApplyStateVisual()` 또는 `PlantSeed()`, `Boost()`, `GetStateColor()`에서 즉시 `NullReferenceException`이 발생할 수 있습니다냥. `FarmCellConfig`는 필수 의존성이므로 `Awake()`에서 null 검증 후 비활성화하거나, 에디터 검증(`OnValidate`)과 명확한 에러 로그를 추가하는 것이 안전합니다냥.

2. `Assets/Scripts/Actor/FarmCell.cs:48`의 `SetState()`가 항상 `ApplyStateVisual()`을 호출해서 선택된 셀의 하이라이트가 행동 직후 사라집니다냥. 예를 들어 선택된 셀에서 `Q/W/E`를 누르면 상태 색상으로 덮어써지고, 다시 숫자키를 눌러 선택을 갱신하기 전까지 선택 표시가 유지되지 않습니다냥. `_isHighlighted` 상태를 `FarmCell` 내부에 저장하고 `ApplyStateVisual()`에서 하이라이트까지 함께 반영하거나, 상태 변경 후 매니저가 `RefreshHighlight()`를 호출하도록 구조를 맞추는 편이 좋습니다냥.

3. `Assets/Scripts/Manager/FarmlandManager.cs:5`, `:38`, `:48`에서 매니저가 `FarmCell` 구체 타입에 직접 의존하고 있어 요구사항의 `Interface DI`와 다소 어긋납니다냥. 행동 호출은 `IFarmCell`을 통해 처리하고, 하이라이트가 필요하다면 별도 뷰 인터페이스를 두거나 `IFarmCell`에 선택 표시 책임을 포함할지 명확히 분리하는 것이 구조적으로 더 낫습니다냥.

4. `Assets/Scripts/Actor/FarmCell.cs:35`와 `:41`의 성장 처리에서 부스트나 큰 `deltaTime`으로 타이머가 크게 음수가 되어도 한 단계만 진행하고 초과 감소분은 버려집니다냥. 현재 값이 의도라면 문제 없지만, “시간 단축” 의미라면 남은 초과 시간을 다음 성장 단계에 이월하거나 반복 advance 처리를 해야 예측 가능한 성장 속도가 됩니다냥.

5. 이전 리뷰는 현재 1번째 리뷰이므로 반영 여부를 비교할 이전 지적사항은 없습니다냥.

### 2회차 리뷰 (2026-04-20 01:50:34)
**Key Findings냥**

1. [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:16)의 `_config == null` 처리는 `Awake()`에서 로그만 남기고 끝나므로, 이후 `PlantSeed()`가 호출되면 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:43)에서 `_config.SeededToGrowingSeconds` 접근으로 여전히 `NullReferenceException`이 발생할 수 있습니다냥. 1회차의 null 가드 요구는 `ApplyStateVisual()` 쪽에는 반영됐지만, 공개 액션 메서드 전체에는 아직 완전히 반영되지 않았습니다냥. `Awake()`에서 `enabled = false`로 컴포넌트를 비활성화하거나, `PlantSeed()`, `Boost()`, `AdvanceGrowth()` 진입 전에 `_config` 유효성을 보장하는 처리가 필요합니다냥.

2. [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:26)와 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:49)의 `while` 루프는 추가됐지만, 초과 시간을 다음 성장 단계로 이월하지 못합니다냥. 예를 들어 Seeded 상태에서 `_growthTimer`가 `-3f`가 된 경우 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:62)에서 `GrowingToGrownSeconds`로 덮어써져 3초 초과분이 사라집니다냥. 1회차에서 지적된 “부스트나 큰 deltaTime의 초과 감소분 처리”는 루프만으로는 해결되지 않았으므로, `AdvanceGrowth()`가 남은 초과 시간을 받아 다음 타이머에서 차감하도록 구조를 바꾸는 편이 맞습니다냥.

3. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:6)의 `_cells`가 여전히 `FarmCell[]` 구체 타입이라 1회차의 Interface DI 지적은 부분 반영 상태입니다냥. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:32)에서 선택 셀을 `IFarmCell`로 받는 점은 개선됐지만, 직렬화 필드와 [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:52)의 캐스팅은 여전히 구현체에 묶여 있습니다냥. Unity 직렬화 제약을 고려하면 `MonoBehaviour[]`로 받고 `IFarmCell` 구현 여부를 `Awake()`나 `OnValidate()`에서 검증하거나, 별도 셀 뷰/모델 인터페이스를 분리하는 방식이 더 명확합니다냥.

**반영 확인냥**

1회차 지적 중 `IFarmCell.SetHighlight()` 추가, `FarmCell`의 `_isHighlighted` 플래그와 시각 갱신 유지, 행동 후 `RefreshHighlight()` 호출은 반영됐습니다냥. 다만 null 의존성 방어, 성장 타이머 초과분 이월, 구체 타입 의존 제거는 아직 개선 여지가 남아 있습니다냥. 이번 2회차 리뷰 요약은 `AI_CONTEXT_Codex/review_log.md`에 기록했습니다냥.

### 3회차 리뷰 (2026-04-20 01:52:34)
**Key Findings냥**

1. [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:16)의 `_config == null` 처리에서 `enabled = false`를 추가한 점은 2회차 지적을 일부 반영했지만, 외부에서 `IFarmCell` 참조로 public 메서드를 호출하는 경로는 여전히 막지 못합니다냥. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:17)에서 비활성화된 `FarmCell`도 그대로 `_farmCells`에 캐시되므로, 설정이 빠진 셀도 `Q`로 `Till()` 상태 전환 후 `W` 입력 시 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:44)의 `_config.SeededToGrowingSeconds` 접근에서 `NullReferenceException`이 발생할 수 있습니다냥. `CanTill()`, `CanPlant()`, `CanBoost()`가 `_config != null`을 함께 확인하게 하거나, public 액션 메서드 진입부에서 설정 유효성을 공통 가드로 막는 처리가 필요합니다냥.

**반영 확인냥**

2회차의 성장 타이머 초과분 이월 지적은 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:62)에서 `overflow`를 계산하고 다음 단계 타이머에 반영하도록 개선되어 해결된 것으로 보입니다냥. FarmlandManager도 [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:15)에서 `IFarmCell[]` 캐시를 구성한 뒤 [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:38)부터 캐스팅 없이 사용하는 형태로 반영되었습니다냥. 이번 3회차 리뷰 요약은 `AI_CONTEXT_Codex/review_log.md`에 기록했습니다냥.

## 리뷰 대상 코드

### 파일: Assets/Scripts/Enum/FarmerState.cs
```
// AI 농부가 가질 수 있는 작업 상태를 정의한다.
public enum FarmerState {
    Idle,
    MovingToCell,
    Tilling,
    Planting,
    Boosting,
    Harvesting,
    MovingToWarehouse,
    Depositing
}

```

### 파일: Assets/Scripts/Interface/IWarehouse.cs
```
// 창고와 상호작용하는 공용 계약. AI가 수확물을 보관할 때 이 인터페이스를 사용한다.
using UnityEngine;

public interface IWarehouse {
    // 창고의 월드 좌표를 반환한다.
    Vector3 Position { get; }

    // 지정한 수량의 수확물을 보관한다.
    void Deposit(int amount);
}

```

### 파일: Assets/Scripts/Data/FarmerConfig.cs
```
// AI 농부의 이동 속도, 상호작용 범위, 작업 지속 시간, Boost 간격을 정의한다.
using UnityEngine;

[CreateAssetMenu(fileName = "FarmerConfig", menuName = "Project_FAD/FarmerConfig")]
public class FarmerConfig : ScriptableObject {
    [Header("Movement")]
    public float MoveSpeed = 2f;
    public float InteractionRange = 0.1f;

    [Header("Work")]
    public float WorkDurationSeconds = 0.5f;
    public float BoostIntervalSeconds = 0.5f;
}

```

### 파일: Assets/Scripts/Actor/FarmerAI.cs
```
// AI 농부 에이전트. 밭 탐색 → 경작/파종/부스트 → 수확 → 창고 반납 사이클을 자율 수행한다.
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class FarmerAI : MonoBehaviour {
    [SerializeField] private FarmerConfig _config;

    private FarmerState _state = FarmerState.Idle;
    private IFarmCell _targetCell;
    private IWarehouse _warehouse;
    private float _workTimer;
    private float _boostTimer;
    private int _heldCrop;
    private System.Action _pendingWorkAction;

    // 컴포넌트를 초기화하고 _config 할당 여부를 검증한다.
    private void Awake() {
        if (_config == null) {
            Debug.LogError("[FarmerAI] FarmerConfig이 할당되지 않았습니다.", this);
            enabled = false;
        }
    }

    // 창고 참조를 캐시한다.
    private void Start() {
        _warehouse = WarehouseManager.Instance;
    }

    // 매 프레임 현재 상태에 맞는 처리를 수행한다.
    private void Update() {
        switch (_state) {
            case FarmerState.Idle:               HandleIdle();             break;
            case FarmerState.MovingToCell:       HandleMoveToCell();       break;
            case FarmerState.Tilling:
            case FarmerState.Planting:
            case FarmerState.Harvesting:         HandleWork();             break;
            case FarmerState.Boosting:           HandleBoosting();         break;
            case FarmerState.MovingToWarehouse:  HandleMoveToWarehouse();  break;
            case FarmerState.Depositing:         HandleDepositing();       break;
        }
    }

    // 작업할 밭 칸을 탐색해 이동을 시작한다. 우선순위: Grown > Seeded/Growing > Tilled > Untilled.
    private void HandleIdle() {
        var cells = FarmlandManager.Instance.AllCells;
        _targetCell = FindByState(cells, FarmCellState.Grown)
                   ?? FindBoostable(cells)
                   ?? FindByState(cells, FarmCellState.Tilled)
                   ?? FindByState(cells, FarmCellState.Untilled);

        if (_targetCell == null) return;
        TransitionTo(FarmerState.MovingToCell);
    }

    // 목표 밭 칸으로 이동한다. 도달 시 칸 상태에 맞는 작업으로 전이한다.
    private void HandleMoveToCell() {
        if (!MoveTowards(FarmlandManager.Instance.GetCellPosition(_targetCell))) return;

        if      (_targetCell.CanHarvest()) TransitionTo(FarmerState.Harvesting);
        else if (_targetCell.CanBoost())   TransitionTo(FarmerState.Boosting);
        else if (_targetCell.CanPlant())   TransitionTo(FarmerState.Planting);
        else if (_targetCell.CanTill())    TransitionTo(FarmerState.Tilling);
        else                               TransitionTo(FarmerState.Idle);
    }

    // _workTimer가 0이 되면 _pendingWorkAction을 실행한다.
    private void HandleWork() {
        _workTimer -= Time.deltaTime;
        if (_workTimer <= 0f) _pendingWorkAction?.Invoke();
    }

    // BoostIntervalSeconds 간격으로 Boost를 호출한다. Grown이 되면 Harvesting으로 전이한다.
    private void HandleBoosting() {
        if (_targetCell.CanHarvest()) { TransitionTo(FarmerState.Harvesting); return; }
        if (!_targetCell.CanBoost())  { TransitionTo(FarmerState.Idle);       return; }

        _boostTimer -= Time.deltaTime;
        if (_boostTimer <= 0f) {
            _targetCell.Boost();
            _boostTimer = _config.BoostIntervalSeconds;
        }
    }

    // 창고를 향해 이동한다. 도달 시 Depositing으로 전이한다.
    private void HandleMoveToWarehouse() {
        if (MoveTowards(_warehouse.Position)) TransitionTo(FarmerState.Depositing);
    }

    // 수확물을 창고에 보관하고 Idle로 전이해 다음 사이클을 시작한다.
    private void HandleDepositing() {
        _warehouse.Deposit(_heldCrop);
        _heldCrop = 0;
        TransitionTo(FarmerState.Idle);
    }

    // 칸을 경작하고 Planting으로 전이한다.
    private void OnTillDone() {
        _targetCell.Till();
        TransitionTo(FarmerState.Planting);
    }

    // 기본 씨앗을 심고 Boosting으로 전이한다.
    private void OnPlantDone() {
        _targetCell.PlantSeed(FarmlandManager.Instance.DefaultSeed);
        TransitionTo(FarmerState.Boosting);
    }

    // 수확하고 수확물을 들고 창고로 이동한다.
    private void OnHarvestDone() {
        _heldCrop = _targetCell.Harvest();
        TransitionTo(FarmerState.MovingToWarehouse);
    }

    // 지정 위치로 MoveTowards 이동한다. InteractionRange 이내 도달 시 true를 반환한다.
    private bool MoveTowards(Vector3 target) {
        target.z = transform.position.z;
        transform.position = Vector3.MoveTowards(transform.position, target, _config.MoveSpeed * Time.deltaTime);
        return Vector3.Distance(transform.position, target) <= _config.InteractionRange;
    }

    // 지정 상태의 첫 번째 칸을 반환한다.
    private IFarmCell FindByState(IReadOnlyList<IFarmCell> cells, FarmCellState state) {
        foreach (var cell in cells)
            if (cell.State == state) return cell;
        return null;
    }

    // Seeded 또는 Growing 상태인 첫 번째 칸을 반환한다.
    private IFarmCell FindBoostable(IReadOnlyList<IFarmCell> cells) {
        foreach (var cell in cells)
            if (cell.State == FarmCellState.Seeded || cell.State == FarmCellState.Growing) return cell;
        return null;
    }

    // 지정 상태로 전이하고 타이머와 대기 액션을 재설정한다.
    private void TransitionTo(FarmerState next) {
        Debug.Log($"[FarmerAI] {_state} → {next}");
        _state = next;
        _workTimer = _config.WorkDurationSeconds;
        _boostTimer = 0f;
        _pendingWorkAction = next switch {
            FarmerState.Tilling    => OnTillDone,
            FarmerState.Planting   => OnPlantDone,
            FarmerState.Harvesting => OnHarvestDone,
            _                      => null
        };
    }
}

```

### 파일: Assets/Scripts/Manager/WarehouseManager.cs
```
// 수확물을 보관하는 창고 Singleton. 총 보관량을 관리한다.
using UnityEngine;

public class WarehouseManager : MonoBehaviour, IWarehouse {
    public static WarehouseManager Instance { get; private set; }

    private int _storedCount;

    public Vector3 Position => transform.position;

    // Singleton 중복 가드를 수행한다.
    private void Awake() {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    // 수확물을 보관하고 로그를 출력한다.
    public void Deposit(int amount) {
        _storedCount += amount;
        Debug.Log($"[WarehouseManager] Deposited: {amount} (total={_storedCount})");
    }
}

```

### 파일: Assets/Scripts/Interface/IFarmCell.cs
```
// 농경지 칸과 상호작용하는 공용 계약. AI 에이전트와 플레이어 모두 이 인터페이스를 통해 칸을 조작한다.
public interface IFarmCell
{
    // 현재 상태를 반환한다.
    FarmCellState State { get; }

    // 경작 가능 여부를 반환한다. (Untilled 상태일 때만 true)
    bool CanTill();

    // 씨앗 심기 가능 여부를 반환한다. (Tilled 상태일 때만 true)
    bool CanPlant();

    // 성장 가속 가능 여부를 반환한다. (Seeded 또는 Growing 상태일 때만 true)
    bool CanBoost();

    // 칸을 경작해 Tilled 상태로 전이한다.
    void Till();

    // 씨앗을 받아 심고 Seeded 상태로 전이한다. 성장 타이밍은 seed가 결정한다.
    void PlantSeed(SeedConfig seed);

    // 성장 타이머를 BoostReductionSeconds만큼 감소시킨다.
    void Boost();

    // 선택 하이라이트 표시 여부를 설정한다.
    void SetHighlight(bool highlight);

    // 수확 가능 여부를 반환한다. (Grown 상태일 때만 true)
    bool CanHarvest();

    // 수확물을 수집하고 칸을 Untilled로 리셋한다. 수확량을 반환한다. 실패 시 0 반환.
    int Harvest();
}

```

### 파일: Assets/Scripts/Actor/FarmCell.cs
```
// 농경지 칸 하나의 상태 머신. 상태 전이, 시간 기반 자동 성장, 선택 하이라이트 시각 표현을 담당한다.
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class FarmCell : MonoBehaviour, IFarmCell {
    [SerializeField] private FarmCellConfig _config;

    private SpriteRenderer _spriteRenderer;
    private FarmCellState _state = FarmCellState.Untilled;
    private SeedConfig _plantedSeed;
    private float _growthTimer;
    private bool _isHighlighted;

    public FarmCellState State => _state;

    // 컴포넌트를 초기화하고 _config 할당 여부를 검증한다.
    private void Awake() {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_config == null) {
            Debug.LogError($"[FarmCell] {name}: FarmCellConfig이 할당되지 않았습니다.", this);
            enabled = false;
            return;
        }
        ApplyStateVisual();
    }

    // 성장 타이머를 진행시키고, 완료 시 다음 상태로 전이한다.
    private void Update() {
        if (_state != FarmCellState.Seeded && _state != FarmCellState.Growing) return;

        _growthTimer -= Time.deltaTime;
        while (_growthTimer <= 0f && (_state == FarmCellState.Seeded || _state == FarmCellState.Growing)) {
            AdvanceGrowth();
        }
    }

    // 경작 가능 여부를 반환한다.
    public bool CanTill()  => _config && _state == FarmCellState.Untilled;

    // 씨앗 심기 가능 여부를 반환한다.
    public bool CanPlant() => _config && _state == FarmCellState.Tilled;

    // 성장 가속 가능 여부를 반환한다.
    public bool CanBoost() => _config && (_state == FarmCellState.Seeded || _state == FarmCellState.Growing);

    // 칸을 경작해 Tilled 상태로 전이한다.
    public void Till() {
        if (!CanTill()) return;
        SetState(FarmCellState.Tilled);
    }

    // 씨앗을 받아 심고 Seeded 상태로 전이한다. 성장 타이밍은 seed가 결정한다.
    public void PlantSeed(SeedConfig seed) {
        if (!CanPlant() || seed == null) return;
        _plantedSeed = seed;
        _growthTimer = seed.SeededToGrowingSeconds;
        SetState(FarmCellState.Seeded);
    }

    // 성장 타이머를 BoostReductionSeconds만큼 감소시키고, 완료 시 다음 상태로 전이한다.
    public void Boost() {
        if (!CanBoost()) return;
        _growthTimer -= _config.BoostReductionSeconds;
        while (_growthTimer <= 0f && (_state == FarmCellState.Seeded || _state == FarmCellState.Growing)) {
            AdvanceGrowth();
        }
    }

    // 수확 가능 여부를 반환한다.
    public bool CanHarvest() => _config && _state == FarmCellState.Grown;

    // 수확물을 수집하고 칸을 Untilled로 리셋한다. 수확량을 반환한다.
    public int Harvest() {
        if (!CanHarvest()) return 0;
        SetState(FarmCellState.Untilled);
        return 1;
    }

    // 하이라이트 플래그를 갱신하고 시각을 다시 적용한다.
    public void SetHighlight(bool highlight) {
        _isHighlighted = highlight;
        ApplyStateVisual();
    }

    // 현재 성장 단계를 다음 단계로 전이한다. 초과된 시간을 다음 타이머에 이월한다.
    private void AdvanceGrowth() {
        float overflow = -_growthTimer;
        if (_state == FarmCellState.Seeded) {
            _growthTimer = Mathf.Max(0f, _plantedSeed.GrowingToGrownSeconds - overflow);
            SetState(FarmCellState.Growing);
        } else if (_state == FarmCellState.Growing) {
            _plantedSeed = null;
            SetState(FarmCellState.Grown);
        }
    }

    // 상태를 변경하고 시각을 갱신한다.
    private void SetState(FarmCellState newState) {
        _state = newState;
        ApplyStateVisual();
    }

    // 현재 상태 색상과 하이라이트를 혼합해 SpriteRenderer에 적용한다.
    private void ApplyStateVisual() {
        if (_spriteRenderer == null || _config == null) return;
        Color baseColor = GetStateColor();
        _spriteRenderer.color = _isHighlighted ? Color.Lerp(baseColor, Color.white, 0.4f) : baseColor;
    }

    // 현재 상태에 대응하는 색상을 반환한다.
    private Color GetStateColor() {
        return _state switch {
            FarmCellState.Untilled => _config.UntilledColor,
            FarmCellState.Tilled   => _config.TilledColor,
            FarmCellState.Seeded   => _config.SeededColor,
            FarmCellState.Growing  => _config.GrowingColor,
            FarmCellState.Grown    => _config.GrownColor,
            _                      => Color.white
        };
    }
}

```

### 파일: Assets/Scripts/Manager/FarmlandManager.cs
```
// 2x2 농경지 칸 배열을 관리하는 Singleton Manager. 칸 선택 상태와 하이라이트를 제어한다.
using System.Collections.Generic;
using UnityEngine;

public class FarmlandManager : MonoBehaviour {
    public static FarmlandManager Instance { get; private set; }

    [SerializeField] private FarmCell[] _cells = new FarmCell[4];
    [SerializeField] private SeedConfig _defaultSeed;

    private IFarmCell[] _farmCells;
    private int _selectedIndex = 0;

    // AI가 참조하는 기본 씨앗을 반환한다.
    public SeedConfig DefaultSeed => _defaultSeed;

    // 모든 농경지 칸을 읽기 전용으로 반환한다.
    public IReadOnlyList<IFarmCell> AllCells => _farmCells;

    // Singleton 중복 가드 및 IFarmCell 캐시 배열을 초기화한다.
    private void Awake() {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        _farmCells = new IFarmCell[_cells.Length];
        for (int i = 0; i < _cells.Length; i++) {
            _farmCells[i] = _cells[i];
        }
    }

    // 초기 하이라이트 상태를 적용한다.
    private void Start() {
        RefreshHighlight();
    }

    // 선택 인덱스를 변경하고 하이라이트를 갱신한다.
    private void SelectCell(int index) {
        if (index < 0 || index >= _farmCells.Length) return;
        _selectedIndex = index;
        RefreshHighlight();
    }

    // 모든 칸의 하이라이트를 현재 선택 인덱스 기준으로 갱신한다.
    private void RefreshHighlight() {
        for (int i = 0; i < _farmCells.Length; i++) {
            _farmCells[i]?.SetHighlight(i == _selectedIndex);
        }
    }

    // 현재 선택된 칸을 IFarmCell로 반환한다.
    private IFarmCell SelectedCell() {
        if (_selectedIndex < 0 || _selectedIndex >= _farmCells.Length) return null;
        return _farmCells[_selectedIndex];
    }

    // 지정한 IFarmCell 칸의 월드 좌표를 반환한다. 존재하지 않으면 Vector3.zero.
    public Vector3 GetCellPosition(IFarmCell cell) {
        for (int i = 0; i < _farmCells.Length; i++) {
            if (_farmCells[i] == cell) return _cells[i].transform.position;
        }
        return Vector3.zero;
    }
}

```

```

### Codex 응답

**Key Findings냥**

1. [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:26)에서 `_warehouse = WarehouseManager.Instance`를 그대로 캐시한 뒤 null 검증이 없어서, 씬에 `WarehouseManager`가 없거나 비활성 상태면 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:87)와 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:92)에서 `NullReferenceException`이 발생합니다냥. `Start()`에서 `_warehouse == null`이면 에러 로그 후 `enabled = false` 처리하거나, `IWarehouse`를 구현한 `MonoBehaviour`를 직렬화 필드로 받아 검증하는 방식이 안전합니다냥.

2. [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:46)와 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:58)에서 `FarmlandManager.Instance`가 항상 존재한다고 가정합니다냥. 씬 조립 누락 시 Idle 진입 즉시 예외가 나므로, `FarmerAI` 초기화 시 `FarmlandManager.Instance`, `AllCells`, `DefaultSeed` 유효성을 한 번 검증하고 실패하면 AI를 비활성화하는 편이 좋습니다냥.

3. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:25)에서 `_cells` 배열을 검증 없이 `_farmCells`로 복사하므로, 인스펙터 슬롯이 비어 있으면 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:125)의 `cell.State` 접근에서 null 예외가 납니다냥. `Awake()` 또는 `OnValidate()`에서 배열 길이 4, null 슬롯 없음, `IFarmCell` 구현 여부를 검사하고 잘못된 셀은 명확히 로그로 알려야 합니다냥.

4. [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:98), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:104), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:110)의 작업 완료 콜백은 실제 작업 성공 여부와 무관하게 다음 상태로 전이합니다냥. 작업 대기 시간 동안 플레이어나 다른 시스템이 셀 상태를 바꾸면 `Till()`이나 `PlantSeed()`가 no-op이어도 AI는 Planting 또는 Boosting으로 넘어가고, `Harvest()`가 0을 반환해도 창고로 이동해 0개를 반납할 수 있습니다냥. 작업 실행 직전에 `CanTill/CanPlant/CanHarvest`를 재검증하고, 실패 시 `Idle`로 복귀하거나 인터페이스 메서드가 성공 여부를 반환하도록 바꾸는 편이 정확합니다냥.

5. 이전 리뷰의 핵심 지적 중 `FarmCellConfig` null 방어는 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:38), [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:41), [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:44), [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:70)의 `Can*` 가드로 반영됐고, 성장 타이머 초과분 이월도 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:87)부터 반영되어 해결된 것으로 보입니다냥. 다만 새로 추가된 `FarmerAI`가 Singleton 매니저에 직접 의존하므로 요구사항의 `IFarmCell/IWarehouse` 의존 방향은 행동 호출부에서는 지켜졌지만 초기 참조 획득 방식에서는 아직 씬 전역 객체 결합이 남아 있습니다냥.

리뷰 요약은 `AI_CONTEXT_Codex/review_log.md`에 기록했습니다냥.

---

## 2회차 리뷰 — 2026-04-20 19:23:04 (🔄 개선 필요)

### Claude → Codex 프롬프트

```
다음 코드를 한국어로 리뷰해주세요. 현재 2번째 리뷰입니다.

## 리뷰 기준
1. 버그 및 잠재적 오류
2. 성능 개선 가능 부분
3. 가독성 및 코드 구조
4. 모범 사례(Best Practice) 준수 여부
5. 이전 리뷰에서 지적된 사항이 반영되었는지 확인

## 중요 규칙
- 개선점이 없으면 응답 **마지막 줄**에 정확히 아래 문자열만 단독으로 출력하세요:
  REVIEW_DONE: NO_FURTHER_IMPROVEMENTS
- 개선점이 있으면 번호 목록으로 구체적으로 설명하세요.

## 요구사항/맥락
1회차 반영: Start()에서 _warehouse/FarmlandManager null 시 enabled=false 처리, OnTillDone/OnPlantDone/OnHarvestDone에 CanXxx 재검증 후 Idle 복귀 추가, FarmlandManager.Awake()에서 _cells 항목 null 체크 및 로그 추가.

## 이전 리뷰 히스토리

### 1회차 리뷰 (2026-04-20 01:48:01)
**Key Findings냥**

1. `Assets/Scripts/Actor/FarmCell.cs:5`, `:28`, `:35`, `:56`에서 `_config`가 할당되지 않은 경우 `Awake()`의 `ApplyStateVisual()` 또는 `PlantSeed()`, `Boost()`, `GetStateColor()`에서 즉시 `NullReferenceException`이 발생할 수 있습니다냥. `FarmCellConfig`는 필수 의존성이므로 `Awake()`에서 null 검증 후 비활성화하거나, 에디터 검증(`OnValidate`)과 명확한 에러 로그를 추가하는 것이 안전합니다냥.

2. `Assets/Scripts/Actor/FarmCell.cs:48`의 `SetState()`가 항상 `ApplyStateVisual()`을 호출해서 선택된 셀의 하이라이트가 행동 직후 사라집니다냥. 예를 들어 선택된 셀에서 `Q/W/E`를 누르면 상태 색상으로 덮어써지고, 다시 숫자키를 눌러 선택을 갱신하기 전까지 선택 표시가 유지되지 않습니다냥. `_isHighlighted` 상태를 `FarmCell` 내부에 저장하고 `ApplyStateVisual()`에서 하이라이트까지 함께 반영하거나, 상태 변경 후 매니저가 `RefreshHighlight()`를 호출하도록 구조를 맞추는 편이 좋습니다냥.

3. `Assets/Scripts/Manager/FarmlandManager.cs:5`, `:38`, `:48`에서 매니저가 `FarmCell` 구체 타입에 직접 의존하고 있어 요구사항의 `Interface DI`와 다소 어긋납니다냥. 행동 호출은 `IFarmCell`을 통해 처리하고, 하이라이트가 필요하다면 별도 뷰 인터페이스를 두거나 `IFarmCell`에 선택 표시 책임을 포함할지 명확히 분리하는 것이 구조적으로 더 낫습니다냥.

4. `Assets/Scripts/Actor/FarmCell.cs:35`와 `:41`의 성장 처리에서 부스트나 큰 `deltaTime`으로 타이머가 크게 음수가 되어도 한 단계만 진행하고 초과 감소분은 버려집니다냥. 현재 값이 의도라면 문제 없지만, “시간 단축” 의미라면 남은 초과 시간을 다음 성장 단계에 이월하거나 반복 advance 처리를 해야 예측 가능한 성장 속도가 됩니다냥.

5. 이전 리뷰는 현재 1번째 리뷰이므로 반영 여부를 비교할 이전 지적사항은 없습니다냥.

### 2회차 리뷰 (2026-04-20 01:50:34)
**Key Findings냥**

1. [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:16)의 `_config == null` 처리는 `Awake()`에서 로그만 남기고 끝나므로, 이후 `PlantSeed()`가 호출되면 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:43)에서 `_config.SeededToGrowingSeconds` 접근으로 여전히 `NullReferenceException`이 발생할 수 있습니다냥. 1회차의 null 가드 요구는 `ApplyStateVisual()` 쪽에는 반영됐지만, 공개 액션 메서드 전체에는 아직 완전히 반영되지 않았습니다냥. `Awake()`에서 `enabled = false`로 컴포넌트를 비활성화하거나, `PlantSeed()`, `Boost()`, `AdvanceGrowth()` 진입 전에 `_config` 유효성을 보장하는 처리가 필요합니다냥.

2. [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:26)와 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:49)의 `while` 루프는 추가됐지만, 초과 시간을 다음 성장 단계로 이월하지 못합니다냥. 예를 들어 Seeded 상태에서 `_growthTimer`가 `-3f`가 된 경우 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:62)에서 `GrowingToGrownSeconds`로 덮어써져 3초 초과분이 사라집니다냥. 1회차에서 지적된 “부스트나 큰 deltaTime의 초과 감소분 처리”는 루프만으로는 해결되지 않았으므로, `AdvanceGrowth()`가 남은 초과 시간을 받아 다음 타이머에서 차감하도록 구조를 바꾸는 편이 맞습니다냥.

3. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:6)의 `_cells`가 여전히 `FarmCell[]` 구체 타입이라 1회차의 Interface DI 지적은 부분 반영 상태입니다냥. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:32)에서 선택 셀을 `IFarmCell`로 받는 점은 개선됐지만, 직렬화 필드와 [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:52)의 캐스팅은 여전히 구현체에 묶여 있습니다냥. Unity 직렬화 제약을 고려하면 `MonoBehaviour[]`로 받고 `IFarmCell` 구현 여부를 `Awake()`나 `OnValidate()`에서 검증하거나, 별도 셀 뷰/모델 인터페이스를 분리하는 방식이 더 명확합니다냥.

**반영 확인냥**

1회차 지적 중 `IFarmCell.SetHighlight()` 추가, `FarmCell`의 `_isHighlighted` 플래그와 시각 갱신 유지, 행동 후 `RefreshHighlight()` 호출은 반영됐습니다냥. 다만 null 의존성 방어, 성장 타이머 초과분 이월, 구체 타입 의존 제거는 아직 개선 여지가 남아 있습니다냥. 이번 2회차 리뷰 요약은 `AI_CONTEXT_Codex/review_log.md`에 기록했습니다냥.

### 3회차 리뷰 (2026-04-20 01:52:34)
**Key Findings냥**

1. [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:16)의 `_config == null` 처리에서 `enabled = false`를 추가한 점은 2회차 지적을 일부 반영했지만, 외부에서 `IFarmCell` 참조로 public 메서드를 호출하는 경로는 여전히 막지 못합니다냥. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:17)에서 비활성화된 `FarmCell`도 그대로 `_farmCells`에 캐시되므로, 설정이 빠진 셀도 `Q`로 `Till()` 상태 전환 후 `W` 입력 시 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:44)의 `_config.SeededToGrowingSeconds` 접근에서 `NullReferenceException`이 발생할 수 있습니다냥. `CanTill()`, `CanPlant()`, `CanBoost()`가 `_config != null`을 함께 확인하게 하거나, public 액션 메서드 진입부에서 설정 유효성을 공통 가드로 막는 처리가 필요합니다냥.

**반영 확인냥**

2회차의 성장 타이머 초과분 이월 지적은 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:62)에서 `overflow`를 계산하고 다음 단계 타이머에 반영하도록 개선되어 해결된 것으로 보입니다냥. FarmlandManager도 [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:15)에서 `IFarmCell[]` 캐시를 구성한 뒤 [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:38)부터 캐스팅 없이 사용하는 형태로 반영되었습니다냥. 이번 3회차 리뷰 요약은 `AI_CONTEXT_Codex/review_log.md`에 기록했습니다냥.

### 1회차 리뷰 (2026-04-20 19:20:38)
**Key Findings냥**

1. [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:26)에서 `_warehouse = WarehouseManager.Instance`를 그대로 캐시한 뒤 null 검증이 없어서, 씬에 `WarehouseManager`가 없거나 비활성 상태면 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:87)와 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:92)에서 `NullReferenceException`이 발생합니다냥. `Start()`에서 `_warehouse == null`이면 에러 로그 후 `enabled = false` 처리하거나, `IWarehouse`를 구현한 `MonoBehaviour`를 직렬화 필드로 받아 검증하는 방식이 안전합니다냥.

2. [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:46)와 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:58)에서 `FarmlandManager.Instance`가 항상 존재한다고 가정합니다냥. 씬 조립 누락 시 Idle 진입 즉시 예외가 나므로, `FarmerAI` 초기화 시 `FarmlandManager.Instance`, `AllCells`, `DefaultSeed` 유효성을 한 번 검증하고 실패하면 AI를 비활성화하는 편이 좋습니다냥.

3. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:25)에서 `_cells` 배열을 검증 없이 `_farmCells`로 복사하므로, 인스펙터 슬롯이 비어 있으면 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:125)의 `cell.State` 접근에서 null 예외가 납니다냥. `Awake()` 또는 `OnValidate()`에서 배열 길이 4, null 슬롯 없음, `IFarmCell` 구현 여부를 검사하고 잘못된 셀은 명확히 로그로 알려야 합니다냥.

4. [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:98), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:104), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:110)의 작업 완료 콜백은 실제 작업 성공 여부와 무관하게 다음 상태로 전이합니다냥. 작업 대기 시간 동안 플레이어나 다른 시스템이 셀 상태를 바꾸면 `Till()`이나 `PlantSeed()`가 no-op이어도 AI는 Planting 또는 Boosting으로 넘어가고, `Harvest()`가 0을 반환해도 창고로 이동해 0개를 반납할 수 있습니다냥. 작업 실행 직전에 `CanTill/CanPlant/CanHarvest`를 재검증하고, 실패 시 `Idle`로 복귀하거나 인터페이스 메서드가 성공 여부를 반환하도록 바꾸는 편이 정확합니다냥.

5. 이전 리뷰의 핵심 지적 중 `FarmCellConfig` null 방어는 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:38), [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:41), [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:44), [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:70)의 `Can*` 가드로 반영됐고, 성장 타이머 초과분 이월도 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:87)부터 반영되어 해결된 것으로 보입니다냥. 다만 새로 추가된 `FarmerAI`가 Singleton 매니저에 직접 의존하므로 요구사항의 `IFarmCell/IWarehouse` 의존 방향은 행동 호출부에서는 지켜졌지만 초기 참조 획득 방식에서는 아직 씬 전역 객체 결합이 남아 있습니다냥.

리뷰 요약은 `AI_CONTEXT_Codex/review_log.md`에 기록했습니다냥.

## 리뷰 대상 코드

### 파일: Assets/Scripts/Actor/FarmerAI.cs
```
// AI 농부 에이전트. 밭 탐색 → 경작/파종/부스트 → 수확 → 창고 반납 사이클을 자율 수행한다.
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class FarmerAI : MonoBehaviour {
    [SerializeField] private FarmerConfig _config;

    private FarmerState _state = FarmerState.Idle;
    private IFarmCell _targetCell;
    private IWarehouse _warehouse;
    private float _workTimer;
    private float _boostTimer;
    private int _heldCrop;
    private System.Action _pendingWorkAction;

    // 컴포넌트를 초기화하고 _config 할당 여부를 검증한다.
    private void Awake() {
        if (_config == null) {
            Debug.LogError("[FarmerAI] FarmerConfig이 할당되지 않았습니다.", this);
            enabled = false;
        }
    }

    // 창고 및 농경지 관리자 참조를 캐시하고 유효성을 검증한다.
    private void Start() {
        _warehouse = WarehouseManager.Instance;
        if (_warehouse == null) {
            Debug.LogError("[FarmerAI] WarehouseManager를 찾을 수 없습니다.", this);
            enabled = false;
            return;
        }
        if (FarmlandManager.Instance == null) {
            Debug.LogError("[FarmerAI] FarmlandManager를 찾을 수 없습니다.", this);
            enabled = false;
        }
    }

    // 매 프레임 현재 상태에 맞는 처리를 수행한다.
    private void Update() {
        switch (_state) {
            case FarmerState.Idle:               HandleIdle();             break;
            case FarmerState.MovingToCell:       HandleMoveToCell();       break;
            case FarmerState.Tilling:
            case FarmerState.Planting:
            case FarmerState.Harvesting:         HandleWork();             break;
            case FarmerState.Boosting:           HandleBoosting();         break;
            case FarmerState.MovingToWarehouse:  HandleMoveToWarehouse();  break;
            case FarmerState.Depositing:         HandleDepositing();       break;
        }
    }

    // 작업할 밭 칸을 탐색해 이동을 시작한다. 우선순위: Grown > Seeded/Growing > Tilled > Untilled.
    private void HandleIdle() {
        var cells = FarmlandManager.Instance.AllCells;
        _targetCell = FindByState(cells, FarmCellState.Grown)
                   ?? FindBoostable(cells)
                   ?? FindByState(cells, FarmCellState.Tilled)
                   ?? FindByState(cells, FarmCellState.Untilled);

        if (_targetCell == null) return;
        TransitionTo(FarmerState.MovingToCell);
    }

    // 목표 밭 칸으로 이동한다. 도달 시 칸 상태에 맞는 작업으로 전이한다.
    private void HandleMoveToCell() {
        if (!MoveTowards(FarmlandManager.Instance.GetCellPosition(_targetCell))) return;

        if      (_targetCell.CanHarvest()) TransitionTo(FarmerState.Harvesting);
        else if (_targetCell.CanBoost())   TransitionTo(FarmerState.Boosting);
        else if (_targetCell.CanPlant())   TransitionTo(FarmerState.Planting);
        else if (_targetCell.CanTill())    TransitionTo(FarmerState.Tilling);
        else                               TransitionTo(FarmerState.Idle);
    }

    // _workTimer가 0이 되면 _pendingWorkAction을 실행한다.
    private void HandleWork() {
        _workTimer -= Time.deltaTime;
        if (_workTimer <= 0f) _pendingWorkAction?.Invoke();
    }

    // BoostIntervalSeconds 간격으로 Boost를 호출한다. Grown이 되면 Harvesting으로 전이한다.
    private void HandleBoosting() {
        if (_targetCell.CanHarvest()) { TransitionTo(FarmerState.Harvesting); return; }
        if (!_targetCell.CanBoost())  { TransitionTo(FarmerState.Idle);       return; }

        _boostTimer -= Time.deltaTime;
        if (_boostTimer <= 0f) {
            _targetCell.Boost();
            _boostTimer = _config.BoostIntervalSeconds;
        }
    }

    // 창고를 향해 이동한다. 도달 시 Depositing으로 전이한다.
    private void HandleMoveToWarehouse() {
        if (MoveTowards(_warehouse.Position)) TransitionTo(FarmerState.Depositing);
    }

    // 수확물을 창고에 보관하고 Idle로 전이해 다음 사이클을 시작한다.
    private void HandleDepositing() {
        _warehouse.Deposit(_heldCrop);
        _heldCrop = 0;
        TransitionTo(FarmerState.Idle);
    }

    // 칸을 경작하고 Planting으로 전이한다. 대기 중 상태가 바뀌었으면 Idle로 복귀한다.
    private void OnTillDone() {
        if (!_targetCell.CanTill()) { TransitionTo(FarmerState.Idle); return; }
        _targetCell.Till();
        TransitionTo(FarmerState.Planting);
    }

    // 기본 씨앗을 심고 Boosting으로 전이한다. 대기 중 상태가 바뀌었으면 Idle로 복귀한다.
    private void OnPlantDone() {
        if (!_targetCell.CanPlant()) { TransitionTo(FarmerState.Idle); return; }
        _targetCell.PlantSeed(FarmlandManager.Instance.DefaultSeed);
        TransitionTo(FarmerState.Boosting);
    }

    // 수확하고 수확물을 들고 창고로 이동한다. 대기 중 상태가 바뀌었으면 Idle로 복귀한다.
    private void OnHarvestDone() {
        if (!_targetCell.CanHarvest()) { TransitionTo(FarmerState.Idle); return; }
        _heldCrop = _targetCell.Harvest();
        TransitionTo(FarmerState.MovingToWarehouse);
    }

    // 지정 위치로 MoveTowards 이동한다. InteractionRange 이내 도달 시 true를 반환한다.
    private bool MoveTowards(Vector3 target) {
        target.z = transform.position.z;
        transform.position = Vector3.MoveTowards(transform.position, target, _config.MoveSpeed * Time.deltaTime);
        return Vector3.Distance(transform.position, target) <= _config.InteractionRange;
    }

    // 지정 상태의 첫 번째 칸을 반환한다.
    private IFarmCell FindByState(IReadOnlyList<IFarmCell> cells, FarmCellState state) {
        foreach (var cell in cells)
            if (cell.State == state) return cell;
        return null;
    }

    // Seeded 또는 Growing 상태인 첫 번째 칸을 반환한다.
    private IFarmCell FindBoostable(IReadOnlyList<IFarmCell> cells) {
        foreach (var cell in cells)
            if (cell.State == FarmCellState.Seeded || cell.State == FarmCellState.Growing) return cell;
        return null;
    }

    // 지정 상태로 전이하고 타이머와 대기 액션을 재설정한다.
    private void TransitionTo(FarmerState next) {
        Debug.Log($"[FarmerAI] {_state} → {next}");
        _state = next;
        _workTimer = _config.WorkDurationSeconds;
        _boostTimer = 0f;
        _pendingWorkAction = next switch {
            FarmerState.Tilling    => OnTillDone,
            FarmerState.Planting   => OnPlantDone,
            FarmerState.Harvesting => OnHarvestDone,
            _                      => null
        };
    }
}

```

### 파일: Assets/Scripts/Manager/FarmlandManager.cs
```
// 2x2 농경지 칸 배열을 관리하는 Singleton Manager. 칸 선택 상태와 하이라이트를 제어한다.
using System.Collections.Generic;
using UnityEngine;

public class FarmlandManager : MonoBehaviour {
    public static FarmlandManager Instance { get; private set; }

    [SerializeField] private FarmCell[] _cells = new FarmCell[4];
    [SerializeField] private SeedConfig _defaultSeed;

    private IFarmCell[] _farmCells;
    private int _selectedIndex = 0;

    // AI가 참조하는 기본 씨앗을 반환한다.
    public SeedConfig DefaultSeed => _defaultSeed;

    // 모든 농경지 칸을 읽기 전용으로 반환한다.
    public IReadOnlyList<IFarmCell> AllCells => _farmCells;

    // Singleton 중복 가드 및 IFarmCell 캐시 배열을 초기화한다.
    private void Awake() {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        _farmCells = new IFarmCell[_cells.Length];
        for (int i = 0; i < _cells.Length; i++) {
            if (_cells[i] == null)
                Debug.LogError($"[FarmlandManager] _cells[{i}]가 null입니다. Inspector에서 연결해주세요.", this);
            _farmCells[i] = _cells[i];
        }
    }

    // 초기 하이라이트 상태를 적용한다.
    private void Start() {
        RefreshHighlight();
    }

    // 선택 인덱스를 변경하고 하이라이트를 갱신한다.
    private void SelectCell(int index) {
        if (index < 0 || index >= _farmCells.Length) return;
        _selectedIndex = index;
        RefreshHighlight();
    }

    // 모든 칸의 하이라이트를 현재 선택 인덱스 기준으로 갱신한다.
    private void RefreshHighlight() {
        for (int i = 0; i < _farmCells.Length; i++) {
            _farmCells[i]?.SetHighlight(i == _selectedIndex);
        }
    }

    // 현재 선택된 칸을 IFarmCell로 반환한다.
    private IFarmCell SelectedCell() {
        if (_selectedIndex < 0 || _selectedIndex >= _farmCells.Length) return null;
        return _farmCells[_selectedIndex];
    }

    // 지정한 IFarmCell 칸의 월드 좌표를 반환한다. 존재하지 않으면 Vector3.zero.
    public Vector3 GetCellPosition(IFarmCell cell) {
        for (int i = 0; i < _farmCells.Length; i++) {
            if (_farmCells[i] == cell) return _cells[i].transform.position;
        }
        return Vector3.zero;
    }
}

```

### 파일: Assets/Scripts/Manager/WarehouseManager.cs
```
// 수확물을 보관하는 창고 Singleton. 총 보관량을 관리한다.
using UnityEngine;

public class WarehouseManager : MonoBehaviour, IWarehouse {
    public static WarehouseManager Instance { get; private set; }

    private int _storedCount;

    public Vector3 Position => transform.position;

    // Singleton 중복 가드를 수행한다.
    private void Awake() {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    // 수확물을 보관하고 로그를 출력한다.
    public void Deposit(int amount) {
        _storedCount += amount;
        Debug.Log($"[WarehouseManager] Deposited: {amount} (total={_storedCount})");
    }
}

```

### 파일: Assets/Scripts/Interface/IFarmCell.cs
```
// 농경지 칸과 상호작용하는 공용 계약. AI 에이전트와 플레이어 모두 이 인터페이스를 통해 칸을 조작한다.
public interface IFarmCell
{
    // 현재 상태를 반환한다.
    FarmCellState State { get; }

    // 경작 가능 여부를 반환한다. (Untilled 상태일 때만 true)
    bool CanTill();

    // 씨앗 심기 가능 여부를 반환한다. (Tilled 상태일 때만 true)
    bool CanPlant();

    // 성장 가속 가능 여부를 반환한다. (Seeded 또는 Growing 상태일 때만 true)
    bool CanBoost();

    // 칸을 경작해 Tilled 상태로 전이한다.
    void Till();

    // 씨앗을 받아 심고 Seeded 상태로 전이한다. 성장 타이밍은 seed가 결정한다.
    void PlantSeed(SeedConfig seed);

    // 성장 타이머를 BoostReductionSeconds만큼 감소시킨다.
    void Boost();

    // 선택 하이라이트 표시 여부를 설정한다.
    void SetHighlight(bool highlight);

    // 수확 가능 여부를 반환한다. (Grown 상태일 때만 true)
    bool CanHarvest();

    // 수확물을 수집하고 칸을 Untilled로 리셋한다. 수확량을 반환한다. 실패 시 0 반환.
    int Harvest();
}

```

### 파일: Assets/Scripts/Actor/FarmCell.cs
```
// 농경지 칸 하나의 상태 머신. 상태 전이, 시간 기반 자동 성장, 선택 하이라이트 시각 표현을 담당한다.
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class FarmCell : MonoBehaviour, IFarmCell {
    [SerializeField] private FarmCellConfig _config;

    private SpriteRenderer _spriteRenderer;
    private FarmCellState _state = FarmCellState.Untilled;
    private SeedConfig _plantedSeed;
    private float _growthTimer;
    private bool _isHighlighted;

    public FarmCellState State => _state;

    // 컴포넌트를 초기화하고 _config 할당 여부를 검증한다.
    private void Awake() {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_config == null) {
            Debug.LogError($"[FarmCell] {name}: FarmCellConfig이 할당되지 않았습니다.", this);
            enabled = false;
            return;
        }
        ApplyStateVisual();
    }

    // 성장 타이머를 진행시키고, 완료 시 다음 상태로 전이한다.
    private void Update() {
        if (_state != FarmCellState.Seeded && _state != FarmCellState.Growing) return;

        _growthTimer -= Time.deltaTime;
        while (_growthTimer <= 0f && (_state == FarmCellState.Seeded || _state == FarmCellState.Growing)) {
            AdvanceGrowth();
        }
    }

    // 경작 가능 여부를 반환한다.
    public bool CanTill()  => _config && _state == FarmCellState.Untilled;

    // 씨앗 심기 가능 여부를 반환한다.
    public bool CanPlant() => _config && _state == FarmCellState.Tilled;

    // 성장 가속 가능 여부를 반환한다.
    public bool CanBoost() => _config && (_state == FarmCellState.Seeded || _state == FarmCellState.Growing);

    // 칸을 경작해 Tilled 상태로 전이한다.
    public void Till() {
        if (!CanTill()) return;
        SetState(FarmCellState.Tilled);
    }

    // 씨앗을 받아 심고 Seeded 상태로 전이한다. 성장 타이밍은 seed가 결정한다.
    public void PlantSeed(SeedConfig seed) {
        if (!CanPlant() || seed == null) return;
        _plantedSeed = seed;
        _growthTimer = seed.SeededToGrowingSeconds;
        SetState(FarmCellState.Seeded);
    }

    // 성장 타이머를 BoostReductionSeconds만큼 감소시키고, 완료 시 다음 상태로 전이한다.
    public void Boost() {
        if (!CanBoost()) return;
        _growthTimer -= _config.BoostReductionSeconds;
        while (_growthTimer <= 0f && (_state == FarmCellState.Seeded || _state == FarmCellState.Growing)) {
            AdvanceGrowth();
        }
    }

    // 수확 가능 여부를 반환한다.
    public bool CanHarvest() => _config && _state == FarmCellState.Grown;

    // 수확물을 수집하고 칸을 Untilled로 리셋한다. 수확량을 반환한다.
    public int Harvest() {
        if (!CanHarvest()) return 0;
        SetState(FarmCellState.Untilled);
        return 1;
    }

    // 하이라이트 플래그를 갱신하고 시각을 다시 적용한다.
    public void SetHighlight(bool highlight) {
        _isHighlighted = highlight;
        ApplyStateVisual();
    }

    // 현재 성장 단계를 다음 단계로 전이한다. 초과된 시간을 다음 타이머에 이월한다.
    private void AdvanceGrowth() {
        float overflow = -_growthTimer;
        if (_state == FarmCellState.Seeded) {
            _growthTimer = Mathf.Max(0f, _plantedSeed.GrowingToGrownSeconds - overflow);
            SetState(FarmCellState.Growing);
        } else if (_state == FarmCellState.Growing) {
            _plantedSeed = null;
            SetState(FarmCellState.Grown);
        }
    }

    // 상태를 변경하고 시각을 갱신한다.
    private void SetState(FarmCellState newState) {
        _state = newState;
        ApplyStateVisual();
    }

    // 현재 상태 색상과 하이라이트를 혼합해 SpriteRenderer에 적용한다.
    private void ApplyStateVisual() {
        if (_spriteRenderer == null || _config == null) return;
        Color baseColor = GetStateColor();
        _spriteRenderer.color = _isHighlighted ? Color.Lerp(baseColor, Color.white, 0.4f) : baseColor;
    }

    // 현재 상태에 대응하는 색상을 반환한다.
    private Color GetStateColor() {
        return _state switch {
            FarmCellState.Untilled => _config.UntilledColor,
            FarmCellState.Tilled   => _config.TilledColor,
            FarmCellState.Seeded   => _config.SeededColor,
            FarmCellState.Growing  => _config.GrowingColor,
            FarmCellState.Grown    => _config.GrownColor,
            _                      => Color.white
        };
    }
}

```

```

### Codex 응답

**Key Findings냥**

1. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:27)에서 `_cells[i] == null`을 로그로만 알린 뒤 [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:29)에 그대로 null을 `_farmCells`에 넣습니다냥. 이 상태에서 `FarmerAI`가 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:136)와 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:143)의 `cell.State`에 접근하면 `NullReferenceException`이 발생합니다냥. 1회차의 `_cells` null 체크와 로그 추가는 반영됐지만, 런타임 실패 경로는 아직 남아 있으므로 잘못된 슬롯이 있으면 매니저나 AI를 비활성화하거나, `_farmCells`에서 null을 제외하고 탐색부도 null을 건너뛰게 해야 합니다냥.

2. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:9)의 `_defaultSeed` 검증이 아직 없습니다냥. [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:116)에서 null 씨앗을 넘기면 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:54)가 조용히 no-op으로 빠지지만, AI는 곧바로 `Boosting`으로 전이해서 다음 프레임 Idle로 돌아가고 같은 `Tilled` 셀을 계속 재시도할 수 있습니다냥. `Start()`나 `FarmlandManager.Awake()`에서 `DefaultSeed` 누락 시 명확히 비활성화하거나, `PlantSeed` 성공 여부를 반환받아 성공할 때만 `Boosting`으로 전이하는 편이 안전합니다냥.

3. [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:123)에서 `Harvest()` 반환값이 0이어도 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:124)에서 항상 창고 이동으로 전이합니다냥. 현재 `FarmCell` 구현만 보면 직전 `CanHarvest()` 재검증으로 대부분 막히지만, `IFarmCell.Harvest()` 계약 자체가 실패 시 0 반환이므로 다른 구현체나 상태 경쟁을 고려하면 0 수확물 반납 경로가 남습니다냥. `_heldCrop <= 0`이면 Idle 복귀와 경고 로그를 남기거나, `Harvest`를 성공 여부와 수량을 함께 표현하는 계약으로 바꾸는 것이 더 견고합니다냥.

**반영 확인냥**

1회차 지적 중 `FarmerAI.Start()`의 `_warehouse`와 `FarmlandManager.Instance` null 시 `enabled = false` 처리는 반영됐습니다냥. `OnTillDone`, `OnPlantDone`, `OnHarvestDone`의 작업 직전 `CanXxx` 재검증 후 Idle 복귀도 반영됐습니다냥. `FarmlandManager.Awake()`의 `_cells` null 로그도 추가됐지만, null 항목이 `AllCells`에 그대로 노출되어 위 1번 문제가 남아 있습니다냥.

이번 2회차 리뷰 요약은 `AI_CONTEXT_Codex/review_log.md`에 기록했습니다냥.

---

## 3회차 리뷰 — 2026-04-20 19:38:02 (🔄 개선 필요)

### Claude → Codex 프롬프트

```
다음 코드를 한국어로 리뷰해주세요. 현재 3번째 리뷰입니다.

## 리뷰 기준
1. 버그 및 잠재적 오류
2. 성능 개선 가능 부분
3. 가독성 및 코드 구조
4. 모범 사례(Best Practice) 준수 여부
5. 이전 리뷰에서 지적된 사항이 반영되었는지 확인

## 중요 규칙
- 개선점이 없으면 응답 **마지막 줄**에 정확히 아래 문자열만 단독으로 출력하세요:
  REVIEW_DONE: NO_FURTHER_IMPROVEMENTS
- 개선점이 있으면 번호 목록으로 구체적으로 설명하세요.

## 요구사항/맥락
2회차 반영: FindByState/FindBoostable에서 null 항목 건너뜀, OnPlantDone에서 DefaultSeed null 시 경고 + Idle 복귀, OnHarvestDone에서 _heldCrop<=0 시 경고 + Idle 복귀.

## 이전 리뷰 히스토리

### 1회차 리뷰 (2026-04-20 01:48:01)
**Key Findings냥**

1. `Assets/Scripts/Actor/FarmCell.cs:5`, `:28`, `:35`, `:56`에서 `_config`가 할당되지 않은 경우 `Awake()`의 `ApplyStateVisual()` 또는 `PlantSeed()`, `Boost()`, `GetStateColor()`에서 즉시 `NullReferenceException`이 발생할 수 있습니다냥. `FarmCellConfig`는 필수 의존성이므로 `Awake()`에서 null 검증 후 비활성화하거나, 에디터 검증(`OnValidate`)과 명확한 에러 로그를 추가하는 것이 안전합니다냥.

2. `Assets/Scripts/Actor/FarmCell.cs:48`의 `SetState()`가 항상 `ApplyStateVisual()`을 호출해서 선택된 셀의 하이라이트가 행동 직후 사라집니다냥. 예를 들어 선택된 셀에서 `Q/W/E`를 누르면 상태 색상으로 덮어써지고, 다시 숫자키를 눌러 선택을 갱신하기 전까지 선택 표시가 유지되지 않습니다냥. `_isHighlighted` 상태를 `FarmCell` 내부에 저장하고 `ApplyStateVisual()`에서 하이라이트까지 함께 반영하거나, 상태 변경 후 매니저가 `RefreshHighlight()`를 호출하도록 구조를 맞추는 편이 좋습니다냥.

3. `Assets/Scripts/Manager/FarmlandManager.cs:5`, `:38`, `:48`에서 매니저가 `FarmCell` 구체 타입에 직접 의존하고 있어 요구사항의 `Interface DI`와 다소 어긋납니다냥. 행동 호출은 `IFarmCell`을 통해 처리하고, 하이라이트가 필요하다면 별도 뷰 인터페이스를 두거나 `IFarmCell`에 선택 표시 책임을 포함할지 명확히 분리하는 것이 구조적으로 더 낫습니다냥.

4. `Assets/Scripts/Actor/FarmCell.cs:35`와 `:41`의 성장 처리에서 부스트나 큰 `deltaTime`으로 타이머가 크게 음수가 되어도 한 단계만 진행하고 초과 감소분은 버려집니다냥. 현재 값이 의도라면 문제 없지만, “시간 단축” 의미라면 남은 초과 시간을 다음 성장 단계에 이월하거나 반복 advance 처리를 해야 예측 가능한 성장 속도가 됩니다냥.

5. 이전 리뷰는 현재 1번째 리뷰이므로 반영 여부를 비교할 이전 지적사항은 없습니다냥.

### 2회차 리뷰 (2026-04-20 01:50:34)
**Key Findings냥**

1. [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:16)의 `_config == null` 처리는 `Awake()`에서 로그만 남기고 끝나므로, 이후 `PlantSeed()`가 호출되면 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:43)에서 `_config.SeededToGrowingSeconds` 접근으로 여전히 `NullReferenceException`이 발생할 수 있습니다냥. 1회차의 null 가드 요구는 `ApplyStateVisual()` 쪽에는 반영됐지만, 공개 액션 메서드 전체에는 아직 완전히 반영되지 않았습니다냥. `Awake()`에서 `enabled = false`로 컴포넌트를 비활성화하거나, `PlantSeed()`, `Boost()`, `AdvanceGrowth()` 진입 전에 `_config` 유효성을 보장하는 처리가 필요합니다냥.

2. [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:26)와 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:49)의 `while` 루프는 추가됐지만, 초과 시간을 다음 성장 단계로 이월하지 못합니다냥. 예를 들어 Seeded 상태에서 `_growthTimer`가 `-3f`가 된 경우 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:62)에서 `GrowingToGrownSeconds`로 덮어써져 3초 초과분이 사라집니다냥. 1회차에서 지적된 “부스트나 큰 deltaTime의 초과 감소분 처리”는 루프만으로는 해결되지 않았으므로, `AdvanceGrowth()`가 남은 초과 시간을 받아 다음 타이머에서 차감하도록 구조를 바꾸는 편이 맞습니다냥.

3. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:6)의 `_cells`가 여전히 `FarmCell[]` 구체 타입이라 1회차의 Interface DI 지적은 부분 반영 상태입니다냥. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:32)에서 선택 셀을 `IFarmCell`로 받는 점은 개선됐지만, 직렬화 필드와 [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:52)의 캐스팅은 여전히 구현체에 묶여 있습니다냥. Unity 직렬화 제약을 고려하면 `MonoBehaviour[]`로 받고 `IFarmCell` 구현 여부를 `Awake()`나 `OnValidate()`에서 검증하거나, 별도 셀 뷰/모델 인터페이스를 분리하는 방식이 더 명확합니다냥.

**반영 확인냥**

1회차 지적 중 `IFarmCell.SetHighlight()` 추가, `FarmCell`의 `_isHighlighted` 플래그와 시각 갱신 유지, 행동 후 `RefreshHighlight()` 호출은 반영됐습니다냥. 다만 null 의존성 방어, 성장 타이머 초과분 이월, 구체 타입 의존 제거는 아직 개선 여지가 남아 있습니다냥. 이번 2회차 리뷰 요약은 `AI_CONTEXT_Codex/review_log.md`에 기록했습니다냥.

### 3회차 리뷰 (2026-04-20 01:52:34)
**Key Findings냥**

1. [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:16)의 `_config == null` 처리에서 `enabled = false`를 추가한 점은 2회차 지적을 일부 반영했지만, 외부에서 `IFarmCell` 참조로 public 메서드를 호출하는 경로는 여전히 막지 못합니다냥. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:17)에서 비활성화된 `FarmCell`도 그대로 `_farmCells`에 캐시되므로, 설정이 빠진 셀도 `Q`로 `Till()` 상태 전환 후 `W` 입력 시 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:44)의 `_config.SeededToGrowingSeconds` 접근에서 `NullReferenceException`이 발생할 수 있습니다냥. `CanTill()`, `CanPlant()`, `CanBoost()`가 `_config != null`을 함께 확인하게 하거나, public 액션 메서드 진입부에서 설정 유효성을 공통 가드로 막는 처리가 필요합니다냥.

**반영 확인냥**

2회차의 성장 타이머 초과분 이월 지적은 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:62)에서 `overflow`를 계산하고 다음 단계 타이머에 반영하도록 개선되어 해결된 것으로 보입니다냥. FarmlandManager도 [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:15)에서 `IFarmCell[]` 캐시를 구성한 뒤 [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:38)부터 캐스팅 없이 사용하는 형태로 반영되었습니다냥. 이번 3회차 리뷰 요약은 `AI_CONTEXT_Codex/review_log.md`에 기록했습니다냥.

### 1회차 리뷰 (2026-04-20 19:20:38)
**Key Findings냥**

1. [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:26)에서 `_warehouse = WarehouseManager.Instance`를 그대로 캐시한 뒤 null 검증이 없어서, 씬에 `WarehouseManager`가 없거나 비활성 상태면 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:87)와 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:92)에서 `NullReferenceException`이 발생합니다냥. `Start()`에서 `_warehouse == null`이면 에러 로그 후 `enabled = false` 처리하거나, `IWarehouse`를 구현한 `MonoBehaviour`를 직렬화 필드로 받아 검증하는 방식이 안전합니다냥.

2. [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:46)와 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:58)에서 `FarmlandManager.Instance`가 항상 존재한다고 가정합니다냥. 씬 조립 누락 시 Idle 진입 즉시 예외가 나므로, `FarmerAI` 초기화 시 `FarmlandManager.Instance`, `AllCells`, `DefaultSeed` 유효성을 한 번 검증하고 실패하면 AI를 비활성화하는 편이 좋습니다냥.

3. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:25)에서 `_cells` 배열을 검증 없이 `_farmCells`로 복사하므로, 인스펙터 슬롯이 비어 있으면 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:125)의 `cell.State` 접근에서 null 예외가 납니다냥. `Awake()` 또는 `OnValidate()`에서 배열 길이 4, null 슬롯 없음, `IFarmCell` 구현 여부를 검사하고 잘못된 셀은 명확히 로그로 알려야 합니다냥.

4. [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:98), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:104), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:110)의 작업 완료 콜백은 실제 작업 성공 여부와 무관하게 다음 상태로 전이합니다냥. 작업 대기 시간 동안 플레이어나 다른 시스템이 셀 상태를 바꾸면 `Till()`이나 `PlantSeed()`가 no-op이어도 AI는 Planting 또는 Boosting으로 넘어가고, `Harvest()`가 0을 반환해도 창고로 이동해 0개를 반납할 수 있습니다냥. 작업 실행 직전에 `CanTill/CanPlant/CanHarvest`를 재검증하고, 실패 시 `Idle`로 복귀하거나 인터페이스 메서드가 성공 여부를 반환하도록 바꾸는 편이 정확합니다냥.

5. 이전 리뷰의 핵심 지적 중 `FarmCellConfig` null 방어는 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:38), [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:41), [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:44), [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:70)의 `Can*` 가드로 반영됐고, 성장 타이머 초과분 이월도 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:87)부터 반영되어 해결된 것으로 보입니다냥. 다만 새로 추가된 `FarmerAI`가 Singleton 매니저에 직접 의존하므로 요구사항의 `IFarmCell/IWarehouse` 의존 방향은 행동 호출부에서는 지켜졌지만 초기 참조 획득 방식에서는 아직 씬 전역 객체 결합이 남아 있습니다냥.

리뷰 요약은 `AI_CONTEXT_Codex/review_log.md`에 기록했습니다냥.

### 2회차 리뷰 (2026-04-20 19:23:04)
**Key Findings냥**

1. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:27)에서 `_cells[i] == null`을 로그로만 알린 뒤 [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:29)에 그대로 null을 `_farmCells`에 넣습니다냥. 이 상태에서 `FarmerAI`가 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:136)와 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:143)의 `cell.State`에 접근하면 `NullReferenceException`이 발생합니다냥. 1회차의 `_cells` null 체크와 로그 추가는 반영됐지만, 런타임 실패 경로는 아직 남아 있으므로 잘못된 슬롯이 있으면 매니저나 AI를 비활성화하거나, `_farmCells`에서 null을 제외하고 탐색부도 null을 건너뛰게 해야 합니다냥.

2. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:9)의 `_defaultSeed` 검증이 아직 없습니다냥. [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:116)에서 null 씨앗을 넘기면 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:54)가 조용히 no-op으로 빠지지만, AI는 곧바로 `Boosting`으로 전이해서 다음 프레임 Idle로 돌아가고 같은 `Tilled` 셀을 계속 재시도할 수 있습니다냥. `Start()`나 `FarmlandManager.Awake()`에서 `DefaultSeed` 누락 시 명확히 비활성화하거나, `PlantSeed` 성공 여부를 반환받아 성공할 때만 `Boosting`으로 전이하는 편이 안전합니다냥.

3. [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:123)에서 `Harvest()` 반환값이 0이어도 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:124)에서 항상 창고 이동으로 전이합니다냥. 현재 `FarmCell` 구현만 보면 직전 `CanHarvest()` 재검증으로 대부분 막히지만, `IFarmCell.Harvest()` 계약 자체가 실패 시 0 반환이므로 다른 구현체나 상태 경쟁을 고려하면 0 수확물 반납 경로가 남습니다냥. `_heldCrop <= 0`이면 Idle 복귀와 경고 로그를 남기거나, `Harvest`를 성공 여부와 수량을 함께 표현하는 계약으로 바꾸는 것이 더 견고합니다냥.

**반영 확인냥**

1회차 지적 중 `FarmerAI.Start()`의 `_warehouse`와 `FarmlandManager.Instance` null 시 `enabled = false` 처리는 반영됐습니다냥. `OnTillDone`, `OnPlantDone`, `OnHarvestDone`의 작업 직전 `CanXxx` 재검증 후 Idle 복귀도 반영됐습니다냥. `FarmlandManager.Awake()`의 `_cells` null 로그도 추가됐지만, null 항목이 `AllCells`에 그대로 노출되어 위 1번 문제가 남아 있습니다냥.

이번 2회차 리뷰 요약은 `AI_CONTEXT_Codex/review_log.md`에 기록했습니다냥.

## 리뷰 대상 코드

### 파일: Assets/Scripts/Actor/FarmerAI.cs
```
// AI 농부 에이전트. 밭 탐색 → 경작/파종/부스트 → 수확 → 창고 반납 사이클을 자율 수행한다.
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class FarmerAI : MonoBehaviour {
    [SerializeField] private FarmerConfig _config;

    private FarmerState _state = FarmerState.Idle;
    private IFarmCell _targetCell;
    private IWarehouse _warehouse;
    private float _workTimer;
    private float _boostTimer;
    private int _heldCrop;
    private System.Action _pendingWorkAction;

    // 컴포넌트를 초기화하고 _config 할당 여부를 검증한다.
    private void Awake() {
        if (_config == null) {
            Debug.LogError("[FarmerAI] FarmerConfig이 할당되지 않았습니다.", this);
            enabled = false;
        }
    }

    // 창고 및 농경지 관리자 참조를 캐시하고 유효성을 검증한다.
    private void Start() {
        _warehouse = WarehouseManager.Instance;
        if (_warehouse == null) {
            Debug.LogError("[FarmerAI] WarehouseManager를 찾을 수 없습니다.", this);
            enabled = false;
            return;
        }
        if (FarmlandManager.Instance == null) {
            Debug.LogError("[FarmerAI] FarmlandManager를 찾을 수 없습니다.", this);
            enabled = false;
        }
    }

    // 매 프레임 현재 상태에 맞는 처리를 수행한다.
    private void Update() {
        switch (_state) {
            case FarmerState.Idle:               HandleIdle();             break;
            case FarmerState.MovingToCell:       HandleMoveToCell();       break;
            case FarmerState.Tilling:
            case FarmerState.Planting:
            case FarmerState.Harvesting:         HandleWork();             break;
            case FarmerState.Boosting:           HandleBoosting();         break;
            case FarmerState.MovingToWarehouse:  HandleMoveToWarehouse();  break;
            case FarmerState.Depositing:         HandleDepositing();       break;
        }
    }

    // 작업할 밭 칸을 탐색해 이동을 시작한다. 우선순위: Grown > Seeded/Growing > Tilled > Untilled.
    private void HandleIdle() {
        var cells = FarmlandManager.Instance.AllCells;
        _targetCell = FindByState(cells, FarmCellState.Grown)
                   ?? FindBoostable(cells)
                   ?? FindByState(cells, FarmCellState.Tilled)
                   ?? FindByState(cells, FarmCellState.Untilled);

        if (_targetCell == null) return;
        TransitionTo(FarmerState.MovingToCell);
    }

    // 목표 밭 칸으로 이동한다. 도달 시 칸 상태에 맞는 작업으로 전이한다.
    private void HandleMoveToCell() {
        if (!MoveTowards(FarmlandManager.Instance.GetCellPosition(_targetCell))) return;

        if      (_targetCell.CanHarvest()) TransitionTo(FarmerState.Harvesting);
        else if (_targetCell.CanBoost())   TransitionTo(FarmerState.Boosting);
        else if (_targetCell.CanPlant())   TransitionTo(FarmerState.Planting);
        else if (_targetCell.CanTill())    TransitionTo(FarmerState.Tilling);
        else                               TransitionTo(FarmerState.Idle);
    }

    // _workTimer가 0이 되면 _pendingWorkAction을 실행한다.
    private void HandleWork() {
        _workTimer -= Time.deltaTime;
        if (_workTimer <= 0f) _pendingWorkAction?.Invoke();
    }

    // BoostIntervalSeconds 간격으로 Boost를 호출한다. Grown이 되면 Harvesting으로 전이한다.
    private void HandleBoosting() {
        if (_targetCell.CanHarvest()) { TransitionTo(FarmerState.Harvesting); return; }
        if (!_targetCell.CanBoost())  { TransitionTo(FarmerState.Idle);       return; }

        _boostTimer -= Time.deltaTime;
        if (_boostTimer <= 0f) {
            _targetCell.Boost();
            _boostTimer = _config.BoostIntervalSeconds;
        }
    }

    // 창고를 향해 이동한다. 도달 시 Depositing으로 전이한다.
    private void HandleMoveToWarehouse() {
        if (MoveTowards(_warehouse.Position)) TransitionTo(FarmerState.Depositing);
    }

    // 수확물을 창고에 보관하고 Idle로 전이해 다음 사이클을 시작한다.
    private void HandleDepositing() {
        _warehouse.Deposit(_heldCrop);
        _heldCrop = 0;
        TransitionTo(FarmerState.Idle);
    }

    // 칸을 경작하고 Planting으로 전이한다. 대기 중 상태가 바뀌었으면 Idle로 복귀한다.
    private void OnTillDone() {
        if (!_targetCell.CanTill()) { TransitionTo(FarmerState.Idle); return; }
        _targetCell.Till();
        TransitionTo(FarmerState.Planting);
    }

    // 기본 씨앗을 심고 Boosting으로 전이한다. 대기 중 상태가 바뀌었거나 씨앗이 없으면 Idle로 복귀한다.
    private void OnPlantDone() {
        if (!_targetCell.CanPlant()) { TransitionTo(FarmerState.Idle); return; }
        var seed = FarmlandManager.Instance.DefaultSeed;
        if (seed == null) {
            Debug.LogWarning("[FarmerAI] DefaultSeed가 null입니다. FarmlandManager Inspector를 확인하세요.", this);
            TransitionTo(FarmerState.Idle);
            return;
        }
        _targetCell.PlantSeed(seed);
        TransitionTo(FarmerState.Boosting);
    }

    // 수확하고 수확물을 들고 창고로 이동한다. 대기 중 상태가 바뀌었거나 수확량이 없으면 Idle로 복귀한다.
    private void OnHarvestDone() {
        if (!_targetCell.CanHarvest()) { TransitionTo(FarmerState.Idle); return; }
        _heldCrop = _targetCell.Harvest();
        if (_heldCrop <= 0) {
            Debug.LogWarning("[FarmerAI] Harvest() 반환값이 0입니다. Idle로 복귀합니다.", this);
            TransitionTo(FarmerState.Idle);
            return;
        }
        TransitionTo(FarmerState.MovingToWarehouse);
    }

    // 지정 위치로 MoveTowards 이동한다. InteractionRange 이내 도달 시 true를 반환한다.
    private bool MoveTowards(Vector3 target) {
        target.z = transform.position.z;
        transform.position = Vector3.MoveTowards(transform.position, target, _config.MoveSpeed * Time.deltaTime);
        return Vector3.Distance(transform.position, target) <= _config.InteractionRange;
    }

    // 지정 상태의 첫 번째 칸을 반환한다. null 항목은 건너뛴다.
    private IFarmCell FindByState(IReadOnlyList<IFarmCell> cells, FarmCellState state) {
        foreach (var cell in cells)
            if (cell != null && cell.State == state) return cell;
        return null;
    }

    // Seeded 또는 Growing 상태인 첫 번째 칸을 반환한다. null 항목은 건너뛴다.
    private IFarmCell FindBoostable(IReadOnlyList<IFarmCell> cells) {
        foreach (var cell in cells)
            if (cell != null && (cell.State == FarmCellState.Seeded || cell.State == FarmCellState.Growing)) return cell;
        return null;
    }

    // 지정 상태로 전이하고 타이머와 대기 액션을 재설정한다.
    private void TransitionTo(FarmerState next) {
        Debug.Log($"[FarmerAI] {_state} → {next}");
        _state = next;
        _workTimer = _config.WorkDurationSeconds;
        _boostTimer = 0f;
        _pendingWorkAction = next switch {
            FarmerState.Tilling    => OnTillDone,
            FarmerState.Planting   => OnPlantDone,
            FarmerState.Harvesting => OnHarvestDone,
            _                      => null
        };
    }
}

```

### 파일: Assets/Scripts/Manager/FarmlandManager.cs
```
// 2x2 농경지 칸 배열을 관리하는 Singleton Manager. 칸 선택 상태와 하이라이트를 제어한다.
using System.Collections.Generic;
using UnityEngine;

public class FarmlandManager : MonoBehaviour {
    public static FarmlandManager Instance { get; private set; }

    [SerializeField] private FarmCell[] _cells = new FarmCell[4];
    [SerializeField] private SeedConfig _defaultSeed;

    private IFarmCell[] _farmCells;
    private int _selectedIndex = 0;

    // AI가 참조하는 기본 씨앗을 반환한다.
    public SeedConfig DefaultSeed => _defaultSeed;

    // 모든 농경지 칸을 읽기 전용으로 반환한다.
    public IReadOnlyList<IFarmCell> AllCells => _farmCells;

    // Singleton 중복 가드 및 IFarmCell 캐시 배열을 초기화한다.
    private void Awake() {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        _farmCells = new IFarmCell[_cells.Length];
        for (int i = 0; i < _cells.Length; i++) {
            if (_cells[i] == null)
                Debug.LogError($"[FarmlandManager] _cells[{i}]가 null입니다. Inspector에서 연결해주세요.", this);
            _farmCells[i] = _cells[i];
        }
    }

    // 초기 하이라이트 상태를 적용한다.
    private void Start() {
        RefreshHighlight();
    }

    // 선택 인덱스를 변경하고 하이라이트를 갱신한다.
    private void SelectCell(int index) {
        if (index < 0 || index >= _farmCells.Length) return;
        _selectedIndex = index;
        RefreshHighlight();
    }

    // 모든 칸의 하이라이트를 현재 선택 인덱스 기준으로 갱신한다.
    private void RefreshHighlight() {
        for (int i = 0; i < _farmCells.Length; i++) {
            _farmCells[i]?.SetHighlight(i == _selectedIndex);
        }
    }

    // 현재 선택된 칸을 IFarmCell로 반환한다.
    private IFarmCell SelectedCell() {
        if (_selectedIndex < 0 || _selectedIndex >= _farmCells.Length) return null;
        return _farmCells[_selectedIndex];
    }

    // 지정한 IFarmCell 칸의 월드 좌표를 반환한다. 존재하지 않으면 Vector3.zero.
    public Vector3 GetCellPosition(IFarmCell cell) {
        for (int i = 0; i < _farmCells.Length; i++) {
            if (_farmCells[i] == cell) return _cells[i].transform.position;
        }
        return Vector3.zero;
    }
}

```

```

### Codex 응답

**Key Findings냥**

1. [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:67), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:84), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:108), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:115), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:128)에서 `_targetCell`이 계속 유효하다고 가정합니다냥. `Idle`에서 셀을 잡은 뒤 이동이나 작업 대기 중 해당 셀이 파괴되거나 매니저 배열에서 빠지면 `GetCellPosition(_targetCell)` 또는 `_targetCell.CanXxx()` 호출에서 예외가 날 수 있습니다냥. 상태 처리 진입부마다 `_targetCell == null` 및 Unity 객체 파괴 여부를 확인해 `Idle`로 복귀하거나, `FarmlandManager.TryGetCellPosition(IFarmCell cell, out Vector3 position)`처럼 성공 여부를 반환하는 API로 바꾸는 편이 안전합니다냥.

2. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:59)의 `GetCellPosition()`이 대상 셀을 찾지 못하면 [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:63)에서 `Vector3.zero`를 반환합니다냥. 이 경우 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:67)의 AI가 실패를 감지하지 못하고 월드 원점으로 이동한 뒤 기존 `_targetCell`에 작업을 시도하는 침묵 실패가 됩니다냥. 존재하지 않는 셀은 명확히 실패로 표현하고, 호출부가 경고 후 `Idle`로 돌아가도록 처리하는 것이 디버깅과 런타임 안정성 면에서 낫습니다냥.

3. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:25)에서 `_cells.Length`를 그대로 신뢰하므로 2x2 매니저라는 주석과 달리 인스펙터에서 배열 크기가 4가 아니어도 런타임이 그대로 진행됩니다냥. 현재 AI 탐색은 null을 건너뛰므로 즉시 예외 가능성은 줄었지만, 씬 조립 오류를 초기에 잡지 못해 선택 인덱스, 하이라이트, AI 탐색 범위가 의도와 달라질 수 있습니다냥. `Awake()` 또는 `OnValidate()`에서 길이 4와 null 슬롯을 검증하고, 잘못된 구성은 명확히 로그 처리하는 편이 좋습니다냥.

**반영 확인냥**

2회차에서 지적된 `FindByState()`와 `FindBoostable()`의 null 항목 건너뛰기는 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:146)와 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:153)에 반영됐습니다냥.

`OnPlantDone()`에서 `DefaultSeed`가 null이면 경고 후 `Idle`로 복귀하는 처리도 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:116)에 반영됐습니다냥.

`OnHarvestDone()`에서 `_heldCrop <= 0`이면 경고 후 `Idle`로 복귀하는 처리도 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:129)에 반영됐습니다냥.

이번 3회차 리뷰 요약은 [review_log.md](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/AI_CONTEXT_Codex/review_log.md)에 기록했습니다냥.

---
