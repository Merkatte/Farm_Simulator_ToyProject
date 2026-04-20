---
name: code_review_codex
description: Codex(OpenAI AI 에이전트)에게 코드 리뷰를 요청하고 결과를 반환하는 스킬. 작성된 코드의 품질 검토, 개선점 도출, 코드 검증이 필요할 때 사용. orchestrate_plan_build_review 스킬의 리뷰 단계에서 호출되며, 단독으로도 사용 가능. 사용자가 "Codex에게 리뷰", "코드 검토", "외부 리뷰" 등을 요청할 때 이 스킬을 활성화한다.
---

# Codex 코드 리뷰 스킬

Codex CLI를 이용해 현재 코드에 대한 리뷰를 요청하고, 결과를 파싱해 Claude에게 반환한다.

## 종결 시그널 규약 (중요)

Codex가 더 이상 개선이 필요 없다고 판단하면, **응답의 마지막 줄**에 아래 문자열을 정확히 출력하도록 프롬프트에 명시해야 한다:

```
REVIEW_DONE: NO_FURTHER_IMPROVEMENTS
```

이 시그널이 감지되면 리뷰 루프를 즉시 중단한다.

## 사용 방법

### 입력 파라미터
- **리뷰 대상 파일 목록**: 검토할 소스 코드 파일 경로들
- **요구사항/맥락**: 원래 코드 작성 목적, 기능 설명 (선택 사항이지만 권장)
- **반복 횟수** (orchestrate에서 호출 시): 몇 번째 리뷰인지 (1~3)

### Codex 호출 방법

아래 Bash 명령을 실행해 Codex에게 리뷰를 요청한다:

```bash
python .claude/skills/orchestrate_plan_build_review/run_review_loop.py \
  --iteration <N> \
  --context <요구사항_텍스트_또는_파일_경로> \
  --files <파일1> [<파일2> ...] \
  --state .claude/skills/orchestrate_plan_build_review/state/state.json
```

### 단독 사용 시 (orchestrate 없이)

단독으로 이 스킬이 호출된 경우, 다음과 같이 Codex를 직접 호출한다:

```bash
codex exec --sandbox read-only "$(cat <<'PROMPT'
다음 코드를 한국어로 리뷰해주세요.

리뷰 기준:
1. 버그 및 잠재적 오류
2. 성능 개선 가능 부분
3. 가독성 및 코드 구조
4. 모범 사례(Best Practice) 준수 여부

개선점이 없으면 마지막 줄에 정확히 아래 문자열만 출력하세요:
REVIEW_DONE: NO_FURTHER_IMPROVEMENTS

[요구사항/맥락]
{여기에 맥락 삽입}

[리뷰 대상 코드]
{여기에 코드 삽입}
PROMPT
)"
```

## 출력 처리

- Codex 응답 전체를 사용자에게 표시한다.
- 마지막 줄이 `REVIEW_DONE: NO_FURTHER_IMPROVEMENTS`이면 "Codex가 추가 개선이 필요 없다고 판단했습니다"라고 알린다.
- 개선점이 있으면 목록으로 정리해 사용자에게 보여준다.

## 주의사항

- Codex는 `read-only` 샌드박스 모드로만 호출한다 (파일 수정 권한 없음).
- `.codex/` 폴더와 `AGENT.md` 파일은 절대 열어보거나 참조하지 않는다.
- Codex의 응답을 기다리는 동안 타임아웃은 300초로 설정한다.
