# Code Convention

## 개요

본 문서는 Project_FAD의 코드 작성 기준을 정의한다.
Claude와 Codex 두 AI 에이전트가 공동으로 참조하는 공용 컨벤션이며, 모든 C# 스크립트에 적용한다.

---

## 기초 컨벤션

### 괄호 스타일 — K&R

여는 괄호를 같은 줄에 붙인다.

```csharp
public class PlayerController : MonoBehaviour {
    private void Update() {
        if (_isGrounded) {
            Jump();
        }
    }
}
```

### 네이밍 규칙

| 대상 | 규칙 | 예시 |
|------|------|------|
| 상수 (`const`, `static readonly`) | SCREAMING_SNAKE_CASE | `MAX_HEALTH`, `BASE_MOVE_SPEED` |
| private 필드 | `_camelCase` | `_count`, `_moveSpeed` |
| public 필드, 프로퍼티, 메서드, 클래스 | PascalCase | `MoveSpeed`, `TakeDamage()` |
| 매개변수 (인자) | camelCase | `damageAmount`, `targetPosition` |

```csharp
public class Enemy : MonoBehaviour {
    private const float MAX_DETECT_RANGE = 10f;

    private int _currentHp;
    private float _moveSpeed;

    public int MaxHp { get; private set; }

    public void TakeDamage(int damageAmount) {
        _currentHp -= damageAmount;
    }
}
```

---

## 주석 규칙

### 1. 스크립트 역할 명시 (최상단)

모든 스크립트 파일의 첫 줄에 해당 스크립트의 역할을 한 줄로 명시한다.

```csharp
// 농경지 칸 하나의 상태 머신. 상태 전이, 시간 기반 성장, 시각 표현을 담당한다.
[RequireComponent(typeof(SpriteRenderer))]
public class FarmCell : MonoBehaviour, IFarmCell {
```

### 2. 메서드 주석

모든 메서드 위에 해당 메서드가 무슨 일을 하는지 한 줄로 명시한다.
구현 방법이 아닌 **목적**을 기술한다.

```csharp
// 성장 타이머를 진행시키고, 완료 시 다음 상태로 전이한다.
private void Update() { ... }

// 현재 상태에 맞는 색상과 하이라이트를 SpriteRenderer에 적용한다.
private void ApplyStateVisual() { ... }
```

### 3. 테스트/임시 코드 표시

프로토타입 검증용이거나 추후 제거 예정인 코드는 `// TEST ONLY:` 태그를 붙인다.

```csharp
// TEST ONLY: AI/플레이어 시스템 구현 후 제거한다.
private void HandleCellAction_Test() { ... }
```

---

## 세부 컨벤션

### 1. 데이터 외부화

게임 내 수치, 설정, 밸런스 값은 **반드시 CSV 또는 ScriptableObject로 분리**한다.
코드에 리터럴 수치를 직접 하드코딩하지 않는다.

적용 대상 예시:
- 유저: 이동속도, 공격력, 체력
- 적: 스폰 위치, 이동속도, 드롭 아이템
- 작물: 성장 시간, 판매 가격, 획득 경험치

```csharp
// 잘못된 예
private float _moveSpeed = 5.5f;

// 올바른 예 — ScriptableObject에서 주입
[SerializeField] private PlayerStatData _statData;
private float _moveSpeed => _statData.MoveSpeed;
```

### 2. SOLID 원칙

- 스크립트 간 **직접 참조는 최소화**한다.
- 의존이 필요한 경우 **Interface를 통해 의존성을 역전**한다.
- 각 클래스는 단일 책임을 가진다 (SRP).

```csharp
// Interface로 의존성 역전
public interface IDamageable {
    void TakeDamage(int amount);
}

public class Projectile : MonoBehaviour {
    private void OnTriggerEnter(Collider other) {
        if (other.TryGetComponent<IDamageable>(out var target)) {
            target.TakeDamage(_damage);
        }
    }
}
```

### 3. DI Container / Singleton

- **DI Container를 우선** 사용한다.
- Singleton은 **Manager 계열 최상위 클래스에서만** 허용한다.
- Singleton 사용은 지양하며, 불가피한 경우에만 도입한다.

```csharp
// Singleton 허용 범위 — Manager 최상위에 한정
public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }

    private void Awake() {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
```

---

## 폴더 구조

### 원칙

- 폴더는 **타입 단위로만** 구분한다.
- "Manager용 Interface", "UI용 Enum"처럼 사용처 기준으로 세분화하지 않는다.
- Interface는 Interface 폴더, Enum은 Enum 폴더에 한곳으로 모은다.

### 구조

```
Assets/
└── Scripts/
    ├── Data/          # ScriptableObject 정의, CSV 파서
    ├── Manager/       # Singleton Manager, 시스템 관리
    ├── Enum/          # 프로젝트 전체 열거형
    ├── Interface/     # 프로젝트 전체 인터페이스
    ├── Actor/         # Player, Enemy 등 게임 오브젝트 로직
    └── UI/            # UI 패널, HUD, 팝업
```

---

## 커밋 전 체크리스트

- [ ] 스크립트 최상단에 역할 설명 주석이 있는가?
- [ ] 모든 메서드에 목적을 설명하는 한 줄 주석이 있는가?
- [ ] 테스트/임시 코드에 `// TEST ONLY:` 태그가 붙어 있는가?
- [ ] private 필드에 `_` 접두사가 붙어 있는가?
- [ ] 상수에 SCREAMING_SNAKE_CASE가 적용되어 있는가?
- [ ] 게임 수치가 ScriptableObject 또는 CSV로 분리되어 있는가?
- [ ] 다른 클래스를 직접 참조하지 않고 Interface를 통하는가?
- [ ] Singleton이 Manager 계층에서만 사용되었는가?
- [ ] 새 스크립트가 올바른 폴더(`Data/`, `Manager/`, `Enum/`, `Interface/`, `Actor/`, `UI/`)에 위치하는가?
