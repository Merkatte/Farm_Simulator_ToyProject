# Claude 작업 로그

## 2026-04-15 (2차 파이프라인 실행)

### CLAUDE.md 설정
- 문장 끝마다 "냥"을 붙이는 규칙 설정
- Codex 협업을 위한 접근 제한 규칙 추가 (`AGENT.md`, `.codex` 폴더 접근 금지)

### 오케스트레이션 스킬 제작 (`orchestrate_plan_build_review`)
- Opus Plan Mode → Sonnet Build → Codex Review(최대 3회) → 기록 종료 파이프라인 스킬 설계 및 구현
- 생성 파일:
  - `.claude/skills/orchestrate_plan_build_review/SKILL.md`
  - `.claude/skills/orchestrate_plan_build_review/run_review_loop.py`
  - `.claude/skills/code_review_codex/SKILL.md`

### PlayerMovement.cs 작성 (파이프라인 테스트)
- `Assets/Scripts/PlayerMovement.cs` 생성 — Unity WASD 캐릭터 이동 스크립트 (Rigidbody 기반)
- Codex 리뷰 2회 수행 후 승인(`REVIEW_DONE: NO_FURTHER_IMPROVEMENTS`) 수신
- 1회차 Codex 지적사항 반영:
  - `[RequireComponent(typeof(Rigidbody))]` 추가
  - `Awake()`에서 Rigidbody 캐싱
  - 입력 처리를 `Update()`, 이동을 `FixedUpdate()`로 분리
  - `public` → `[SerializeField] private` 캡슐화
- 2회차에서 Codex 최종 승인

### PlayerMovement.cs — 스페이스 대시(회피) 기능 추가
- `Assets/Scripts/PlayerMovement.cs` 수정 — 스페이스 키 대시 구현
- Rigidbody + Coroutine 방식. 이동 방향으로 대시, 정지 시 transform.forward(수평 보정)
- Codex 리뷰 3회 수행 → 3회 소진으로 파이프라인 종료
- 리뷰 반영 사항: `dashDirectionSnapshot` 주석 명시, `[Min]` 속성 추가, `Vector3.ProjectOnPlane`으로 forward Y성분 제거
- 미반영(scope 초과): MovePosition 물리 방식, 코루틴-FixedUpdate 타이밍 오차
- 리뷰 로그: `AI_CONTEXT_Claude/review_log.md` 참조

### PlayerMovement.cs — Debug.Log 추가 (테스트 목적)
- `Assets/Scripts/PlayerMovement.cs` 수정 — Debug.Log 3곳 추가
  - `Awake`: Rigidbody 초기화 확인
  - 대시 시작: 방향·속도 출력
  - 대시 종료 / 쿨다운 종료 시점 출력
- Codex 리뷰 **1회차에서 즉시 승인** (REVIEW_DONE)
