---
name: orchestrate-plan-build-review
description: Participate as the Codex review stage inside a Claude-led plan-build-review orchestration loop. Use when Claude has already planned and implemented code, then asks Codex to review iterations, verify whether prior feedback was addressed, emit the review termination signal, or record the review history.
---

# Orchestrate Plan Build Review

Act as the Codex side of a multi-agent pipeline where Claude plans and builds, and Codex reviews.

## Role

1. Assume planning and implementation were handled elsewhere unless the prompt says otherwise.
2. Focus on reviewing the changed code and validating whether the current iteration is ready.
3. Compare the current code against any prior review history included in the prompt.

## Review Loop Rules

1. On each iteration, inspect the supplied files and the provided context.
2. If earlier review comments are included, verify whether they were addressed.
3. Return concrete review feedback in Korean when more work is needed.
4. If the code is acceptable for the requested scope, end the final line exactly with:

`REVIEW_DONE: NO_FURTHER_IMPROVEMENTS`

5. Emit the signal only when no further meaningful action is required from the implementer for this loop.

## Recording Requests

If Claude later asks Codex to record the review history, summarize only what Codex itself handled:

1. review iteration count
2. key findings per iteration
3. final judgment

Store or format that record only if the caller explicitly asks for it.

## Constraints

1. Do not inspect `.claude/` or `CLAUDE.md`.
2. Do not depend on helper scripts under `.claude/`; the caller must provide the needed context directly.
3. Do not recursively invoke Codex CLI from inside the skill.
4. Stay in review mode unless the caller explicitly asks for code changes.

## Recommended Response Shape

1. Briefly state whether this iteration passes review.
2. If it fails, enumerate concrete findings with file references.
3. If it passes, state that no further improvements are required and place the termination signal on the last line.
