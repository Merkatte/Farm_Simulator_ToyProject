# Project_FAD — 작업 로그

공용 작업 이력. Claude와 Codex 모두 이 파일에 작업 내용을 기록한다.

---

## 2026-04-19

### Claude

- `AI_CONTEXT_Public/code_convention.md` 작성
  - K&R 괄호, 네이밍 규칙(SCREAMING_SNAKE_CASE / _camelCase / PascalCase), 데이터 외부화(SO/CSV), SOLID/Interface DI, Singleton 제한, 폴더 구조 규칙 정의

- 2x2 농경지 프로토타입 스크립트 5개 구현 (orchestrate 파이프라인 — Codex 3회 리뷰)
  - `Assets/Scripts/Enum/FarmCellState.cs` — 5단계 상태 열거형
  - `Assets/Scripts/Interface/IFarmCell.cs` — 게임 로직 + SetHighlight 인터페이스
  - `Assets/Scripts/Data/FarmCellConfig.cs` — ScriptableObject (타이밍/색상 데이터)
  - `Assets/Scripts/Actor/FarmCell.cs` — 상태머신, 시간 기반 자동 성장, 하이라이트
  - `Assets/Scripts/Manager/FarmlandManager.cs` — Singleton, 키보드 디버그 입력 처리
  - Codex 3회 리뷰 소진 종료 (주요 개선: null 가드, 타이머 이월, IFarmCell[] 캐시)

- `SKILL.md` 수정 — [5단계] Codex 기록 요청 명령에서 `--sandbox read-only` 플래그 제거 (기록 시 파일 쓰기 필요)

- `Data/SeedConfig.cs` 신규 생성 — 씨앗별 성장 타이밍(SeededToGrowing, GrowingToGrown)을 별도 SO로 분리
- `Data/FarmCellConfig.cs` 수정 — 씨앗 타이밍 필드 제거. 칸 고유 데이터(BoostReductionSeconds, 상태 색상)만 유지
- `Interface/IFarmCell.cs` 수정 — `PlantSeed()` → `PlantSeed(SeedConfig seed)` 시그니처 변경
- `Actor/FarmCell.cs` 수정 — `_plantedSeed` 필드 추가. PlantSeed 호출 시 씨앗을 받아 보관하고 성장에 사용. 수확 시 null 처리
- `Manager/FarmlandManager.cs` 수정 — `_seedConfig` 타입을 `FarmCellConfig` → `SeedConfig`로 변경

- `AI_CONTEXT_Public/log.md` 신규 생성 — 공용 작업 이력 문서
- `AI_CONTEXT_Public/Architecture.md` 신규 생성 — 공용 코드 구조 문서
- `.claude/skills/update_context/SKILL.md` 신규 생성 — 공용 문서 갱신 스킬
- `.claude/skills/orchestrate_plan_build_review/SKILL.md` 수정 — 파이프라인 마지막에 update_context 호출 단계([8단계]) 추가

- `Manager/FarmlandManager.cs` 수정 — `UnityEngine.Input` → New Input System(`Keyboard.current`) 전환. 테스트 메서드명에 `_Test` 접미사 추가
- `AI_CONTEXT_Public/code_convention.md` 수정 — 주석 규칙 섹션 추가 (스크립트 최상단 역할 명시, 메서드 목적 주석, `// TEST ONLY:` 태그 규칙)
- 전체 스크립트 6개 주석 추가 — 컨벤션에 따라 최상단 역할 설명 및 메서드별 목적 주석 일괄 작성
  - `FarmCellState.cs`, `IFarmCell.cs`, `FarmCellConfig.cs`, `SeedConfig.cs`, `FarmCell.cs`, `FarmlandManager.cs`

---
