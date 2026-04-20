---
name: orchestrate_plan_build_review
description: 코드 작성 요청에 대해 Opus로 플랜 수립 → Sonnet으로 구현 → Codex 리뷰(최대 3회 반복) → 기록 종료 순서의 AI 오케스트레이션 파이프라인을 실행하는 스킬. 사용자가 어떤 기능 구현, 알고리즘 작성, 시스템 개발 등 코드 작성을 요청할 때 이 스킬을 발동한다. 단순 질문이나 설명 요청에는 사용하지 않는다.
---

# 오케스트레이션 파이프라인 스킬

Claude(Opus Plan + Sonnet Build)와 Codex(외부 AI 리뷰어) 간의 협업 파이프라인을 자동화한다.

## 전체 흐름

```
[1] Opus → Plan 수립 (Plan Mode)
[2] Sonnet → 코드 작성
[3~5] Codex → 리뷰 (최대 3회 반복)
    ├─ 개선점 있음 → Sonnet 수정 후 다시 리뷰
    └─ 개선점 없음 OR 3회 소진 → 종료
[6] Codex에게 기록 요청 (비동기, 기다리지 않음)
[7] Claude 자신의 작업 기록 (메모리)
[8] update_context 스킬 호출 → log.md / Architecture.md 갱신
```

## 헬퍼 스크립트

- **`run_review_loop.py`**: Codex 호출, 반복 제한, 종결 시그널 감지
- **`state/state.json`**: 반복 횟수 및 리뷰 히스토리 저장

스크립트 경로: `.claude/skills/orchestrate_plan_build_review/run_review_loop.py`

---

## 단계별 실행 지침

### [1단계] Plan 수립 (Opus Plan Mode)

모델이 `opusplan`으로 설정된 경우 플랜 모드에서 자동으로 Opus를 사용한다.

1. 사용자 요청을 분석해 구현 플랜을 작성한다.
2. `ExitPlanMode`를 호출해 사용자의 플랜 승인을 받는다.
3. 플랜이 승인되면 2단계로 진행한다.

### [2단계] 코드 작성 (Sonnet)

플랜에 따라 코드를 작성한다.

- 작성 완료 후, 수정된 파일 목록을 메모한다.
- state.json 초기화:

```bash
mkdir -p .claude/skills/orchestrate_plan_build_review/state
```

### [3단계] Codex 리뷰 루프 (최대 3회)

아래를 반복한다 (N = 1, 2, 3):

#### 3-A. Codex 리뷰 요청

```bash
python .claude/skills/orchestrate_plan_build_review/run_review_loop.py \
  --iteration <N> \
  --context "<요구사항 한 줄 요약 또는 context 파일 경로>" \
  --files <수정된_파일1> [<파일2> ...] \
  --state .claude/skills/orchestrate_plan_build_review/state/state.json \
  --project-root <프로젝트_루트_절대경로>
```

`--project-root`는 Codex의 작업 디렉토리를 고정한다. 이 경로에서 Codex가 `AGENTS.md`를 자동으로 읽어 프로젝트 규칙을 적용한다. 미지정 시 스크립트 실행 시점의 현재 디렉토리를 사용한다.

#### 3-B. 결과 파싱

출력은 JSON 형태다:

```json
{
  "iteration": 1,
  "done": false,
  "review": "Codex 리뷰 텍스트...",
  "status": "ok"
}
```

- `status == "loop_exhausted"`: 3회 초과 → **[5단계]**로 이동
- `done == true`: Codex가 개선 불필요 판단 → **[5단계]**로 이동
- `done == false`: 리뷰 내용 사용자에게 표시 → **3-C**로 이동

#### 3-C. 개선점 수정 (Sonnet)

Codex 리뷰를 바탕으로 코드를 수정한다. 완료 후 N을 증가시켜 3-A로 돌아간다.

### [4단계] 루프 종료 조건 확인

- `done == true` → "Codex가 추가 개선이 필요 없다고 판단했습니다"를 사용자에게 알린다.
- 3회 소진 → "최대 리뷰 횟수(3회)에 도달했습니다. 최종 코드를 제출합니다"를 알린다.

### [5단계] Codex에게 기록 요청 (비동기, 기다리지 않음)

Codex에게 작업 기록을 요청하는 명령을 발사한다. **응답을 기다리지 않는다.**

```bash
codex exec "지금까지 진행한 코드 리뷰 작업 내역을 기록해주세요. 리뷰 횟수, 각 회차별 주요 지적 사항, 최종 판정을 포함해주세요. 저장 위치는 스스로의 컨텍스트를 읽고 결정하세요."
```

> **주의**: 이 명령 실행 후 즉시 다음 단계로 이동한다. Codex의 완료를 기다리지 않는다.

### [6단계] Claude 작업 기록 후 종료

Claude 자신이 진행한 작업 내역을 메모리에 기록한다:

- 프로젝트 메모리(`project` 타입)에 아래 내용을 저장한다:
  - 작업 일시
  - 구현 기능 요약
  - 수정된 파일 목록
  - 리뷰 반복 횟수
  - 최종 결과 (Codex 승인 / 3회 소진)

기록 완료 후 `update_context` 스킬을 호출해 공용 문서를 갱신한다.

### [7단계] update_context 스킬 호출

`AI_CONTEXT_Public/log.md`와 `AI_CONTEXT_Public/Architecture.md`를 최신 상태로 업데이트한다.

이번 작업에서 생성·수정된 파일 목록과 변경 내용을 인수로 넘긴다.

```
/update_context <이번 작업 요약>
```

업데이트 완료 후 사용자에게 파이프라인 완료를 알리고 종료한다.

---

## 제약 사항 (CLAUDE.md 준수)

- `.codex/` 폴더와 `AGENT.md` 파일은 절대 확인하거나 열어보지 않는다.
- Codex의 기록 저장 위치는 Codex가 자율적으로 결정한다. Claude는 경로를 지정하거나 검증하지 않는다.
- **리뷰 호출** (`run_review_loop.py` 내부): `--sandbox read-only` 모드로 수행한다 (코드 읽기 전용).
- **기록 요청** ([5단계] Codex 기록 명령): `--sandbox read-only` 없이 호출한다. Codex가 자체 로그 파일에 써야 하기 때문이다.

---

## 종결 시그널 규약

Codex가 "더이상 개선 필요 없음"을 알릴 때 사용하는 문자열:

```
REVIEW_DONE: NO_FURTHER_IMPROVEMENTS
```

이 시그널은 `run_review_loop.py`가 자동으로 감지하며, `"done": true`로 반환된다.
