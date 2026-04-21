"""
run_review_loop.py
------------------
Codex CLI를 호출해 코드 리뷰를 수행하고, 최대 3회 반복 제한과
종결 시그널 감지를 처리하는 오케스트레이션 헬퍼.

사용법:
    python run_review_loop.py \
        --iteration <1|2|3> \
        --context <맥락_텍스트_또는_파일_경로> \
        --files <파일1> [<파일2> ...] \
        --state <state.json_경로>

출력 (JSON, stdout):
    {
        "iteration": 1,
        "done": false,
        "review": "Codex의 리뷰 텍스트...",
        "status": "ok"  # "ok" | "loop_exhausted" | "error"
    }
"""

import argparse
import json
import os
import subprocess
import sys
from datetime import datetime
from pathlib import Path

DONE_SIGNAL = "REVIEW_DONE: NO_FURTHER_IMPROVEMENTS"
MAX_ITERATIONS = 3
CODEX_TIMEOUT = 300  # 초


def load_state(state_path: str) -> dict:
    """state.json을 로드한다. 없으면 초기 상태를 반환한다."""
    path = Path(state_path)
    if path.exists():
        with open(path, encoding="utf-8") as f:
            return json.load(f)
    return {"iteration": 0, "done": False, "history": []}


def save_state(state_path: str, state: dict) -> None:
    """state.json을 저장한다. 디렉터리가 없으면 생성한다."""
    path = Path(state_path)
    path.parent.mkdir(parents=True, exist_ok=True)
    with open(path, "w", encoding="utf-8") as f:
        json.dump(state, f, ensure_ascii=False, indent=2)


def read_file_content(file_path: str) -> str:
    """파일 내용을 읽어 반환한다."""
    try:
        with open(file_path, encoding="utf-8") as f:
            return f.read()
    except FileNotFoundError:
        return f"[파일 없음: {file_path}]"
    except Exception as e:
        return f"[파일 읽기 오류: {e}]"


def build_review_prompt(context: str, files: list[str], iteration: int, history: list) -> str:
    """Codex에게 전달할 리뷰 프롬프트를 생성한다."""
    # 파일 내용 수집
    files_section = ""
    for file_path in files:
        content = read_file_content(file_path)
        files_section += f"\n### 파일: {file_path}\n```\n{content}\n```\n"

    # 이전 리뷰 히스토리 포함 (2회차 이상)
    history_section = ""
    if history:
        history_section = "\n## 이전 리뷰 히스토리\n"
        for entry in history:
            history_section += (
                f"\n### {entry['iteration']}회차 리뷰 ({entry['timestamp']})\n"
                f"{entry['review']}\n"
            )

    prompt = f"""다음 코드를 한국어로 리뷰해주세요. 현재 {iteration}번째 리뷰입니다.

## 리뷰 기준
1. 버그 및 잠재적 오류
2. 성능 개선 가능 부분
3. 가독성 및 코드 구조
4. 모범 사례(Best Practice) 준수 여부
5. 이전 리뷰에서 지적된 사항이 반영되었는지 확인

## 중요 규칙
- 개선점이 없으면 응답 **마지막 줄**에 정확히 아래 문자열만 단독으로 출력하세요:
  {DONE_SIGNAL}
- 개선점이 있으면 번호 목록으로 구체적으로 설명하세요.

## 요구사항/맥락
{context}
{history_section}
## 리뷰 대상 코드
{files_section}"""

    return prompt


def invoke_codex(prompt: str, project_root: str | None = None) -> str:
    """Codex CLI를 호출하고 출력을 반환한다.
    프롬프트를 stdin으로 전달해 인수 파싱 문제(특히 한글/특수문자)를 회피한다.
    Windows 한글 환경(CP949)과 UTF-8 인코딩을 모두 시도한다.

    project_root: Codex의 작업 디렉토리. 지정하면 Codex가 해당 디렉토리에서 실행되어
                  AGENTS.md 등 프로젝트 설정 파일을 자동으로 읽는다.
    """
    try:
        # '-' 인수로 stdin에서 프롬프트를 읽도록 지시
        # --skip-git-repo-check: git 저장소 외부에서도 실행 허용
        result = subprocess.run(
            "codex exec --sandbox workspace-write --skip-git-repo-check -",
            input=prompt.encode("utf-8"),
            capture_output=True,
            timeout=CODEX_TIMEOUT,
            shell=True,
            cwd=project_root,  # Codex 작업 디렉토리 고정 → AGENTS.md 자동 탐지
        )
        # Windows 한글 환경에서 CP949로 먼저 디코딩 시도, 실패 시 UTF-8
        raw_out = result.stdout
        raw_err = result.stderr
        for enc in ("utf-8", "cp949", "euc-kr"):
            try:
                output = raw_out.decode(enc).strip()
                stderr_text = raw_err.decode(enc).strip()
                break
            except UnicodeDecodeError:
                continue
        else:
            output = raw_out.decode("utf-8", errors="replace").strip()
            stderr_text = raw_err.decode("utf-8", errors="replace").strip()

        if result.returncode != 0 and not output:
            return f"[Codex 오류 (exit {result.returncode})]: {stderr_text}"
        return output
    except subprocess.TimeoutExpired:
        return f"[Codex 타임아웃: {CODEX_TIMEOUT}초 초과]"
    except FileNotFoundError:
        return "[Codex CLI를 찾을 수 없습니다. 'codex'가 PATH에 있는지 확인하세요]"
    except Exception as e:
        return f"[Codex 호출 오류]: {e}"


def save_review_log(
    state_path: str,
    prompt: str,
    review_text: str,
    iteration: int,
    timestamp: str,
    done: bool,
) -> None:
    """AI_CONTEXT_Claude/review_log.md에 프롬프트와 Codex 응답을 누적 저장한다."""
    # state.json 위치 기준으로 프로젝트 루트 추정
    # 예: .claude/skills/.../state/state.json → 루트는 4단계 위
    state_dir = Path(state_path).resolve()
    project_root = state_dir
    for _ in range(5):
        project_root = project_root.parent
        if (project_root / "AI_CONTEXT_Claude").exists():
            break

    date_str = datetime.now().strftime("%Y-%m-%d")
    log_path = project_root / "AI_CONTEXT_Claude" / f"review_log_{date_str}.md"
    log_path.parent.mkdir(parents=True, exist_ok=True)

    status_label = "✅ 승인 (REVIEW_DONE)" if done else "🔄 개선 필요"
    separator = "---"

    entry = (
        f"\n## {iteration}회차 리뷰 — {timestamp} ({status_label})\n\n"
        f"### Claude → Codex 프롬프트\n\n"
        f"```\n{prompt}\n```\n\n"
        f"### Codex 응답\n\n"
        f"{review_text}\n\n"
        f"{separator}\n"
    )

    # 파일이 없으면 헤더 포함해서 새로 생성
    if not log_path.exists():
        header = f"# Codex 리뷰 로그 — {date_str}\n\nClaude가 Codex에게 보낸 프롬프트와 Codex의 응답을 회차별로 기록합니다.\n"
        log_path.write_text(header + entry, encoding="utf-8")
    else:
        with open(log_path, "a", encoding="utf-8") as f:
            f.write(entry)


def detect_done(review_text: str) -> bool:
    """종결 시그널이 있는지 감지한다 (마지막 줄 기준)."""
    lines = review_text.strip().splitlines()
    if not lines:
        return False
    return lines[-1].strip() == DONE_SIGNAL


def resolve_context(context_arg: str) -> str:
    """context 인자가 파일 경로면 읽고, 아니면 텍스트로 사용한다."""
    if os.path.isfile(context_arg):
        return read_file_content(context_arg)
    return context_arg


def main():
    parser = argparse.ArgumentParser(description="Codex 코드 리뷰 루프 헬퍼")
    parser.add_argument("--iteration", type=int, required=True, help="현재 반복 횟수 (1~3)")
    parser.add_argument("--context", type=str, required=True, help="요구사항 텍스트 또는 파일 경로")
    parser.add_argument("--files", nargs="+", required=True, help="리뷰 대상 파일 목록")
    parser.add_argument("--state", type=str, required=True, help="state.json 경로")
    parser.add_argument(
        "--project-root",
        type=str,
        default=None,
        help="Codex 작업 디렉토리 (미지정 시 현재 디렉토리). AGENTS.md 탐색 기준.",
    )
    args = parser.parse_args()

    # 프로젝트 루트 결정: 명시 지정 > 현재 디렉토리
    project_root = str(Path(args.project_root).resolve()) if args.project_root else os.getcwd()

    # 반복 횟수 초과 확인
    if args.iteration > MAX_ITERATIONS:
        result = {
            "iteration": args.iteration,
            "done": False,
            "review": "",
            "status": "loop_exhausted",
        }
        print(json.dumps(result, ensure_ascii=False))
        sys.exit(0)

    # 상태 로드
    state = load_state(args.state)

    # 맥락 해석
    context = resolve_context(args.context)

    # 프롬프트 생성
    prompt = build_review_prompt(context, args.files, args.iteration, state.get("history", []))

    # Codex 호출 (project_root를 cwd로 고정 → AGENTS.md 자동 탐지)
    review_text = invoke_codex(prompt, project_root=project_root)

    # 종결 시그널 감지
    done = detect_done(review_text)

    timestamp = datetime.now().strftime("%Y-%m-%d %H:%M:%S")

    # 상태 업데이트
    state["iteration"] = args.iteration
    state["done"] = done
    state.setdefault("history", []).append(
        {
            "iteration": args.iteration,
            "timestamp": timestamp,
            "review": review_text,
            "done": done,
        }
    )
    save_state(args.state, state)

    # AI_CONTEXT_Claude/review_log.md에 프롬프트 + 응답 누적 저장
    save_review_log(args.state, prompt, review_text, args.iteration, timestamp, done)

    # 결과 출력 (JSON)
    result = {
        "iteration": args.iteration,
        "done": done,
        "review": review_text,
        "status": "ok",
    }
    print(json.dumps(result, ensure_ascii=False))


if __name__ == "__main__":
    main()
