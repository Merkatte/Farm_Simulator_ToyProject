---
name: code-review-codex
description: Review local code as an external Codex reviewer for Claude-driven workflows. Use when Claude, another agent, or the user asks Codex to inspect changed files, find bugs or regressions, validate implementation quality, or provide an approval-style review with a termination signal.
---

# Code Review Codex

Review the provided files directly in the current workspace.

## Core Behavior

1. Treat the task as a code review, not an implementation pass, unless the caller explicitly asks for fixes.
2. Prioritize concrete findings in this order:
   - correctness bugs and regressions
   - missing edge-case handling
   - security or data-loss risks
   - performance issues with practical impact
   - maintainability problems that are likely to cause future defects
3. Reference file paths and line numbers whenever they can be identified.
4. Keep the review concise and actionable.

## Output Contract

1. If problems exist, list them clearly in Korean.
2. If no meaningful improvements are needed, make that explicit.
3. When the caller asks for orchestration-compatible output, or when the prompt requests a termination signal, end the final line exactly with:

`REVIEW_DONE: NO_FURTHER_IMPROVEMENTS`

4. Only emit that final signal when you judge that no further meaningful improvements are required for the requested review scope.

## Constraints

1. Do not inspect `.claude/` or `CLAUDE.md`.
2. Do not rely on recursive `codex exec` self-invocation.
3. Review only the files and context the caller supplied, plus directly related local files when needed to validate behavior.
4. Do not modify files during review unless the caller explicitly changes the task from review to fix.

## Suggested Review Structure

1. Use a `Key Findings` section ordered by severity.
2. Add `Residual Risks` or `Testing Gaps` only when they materially help the caller.
3. If no improvements are needed, give a short approval statement and place the termination signal on the last line.
