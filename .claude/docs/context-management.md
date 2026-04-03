# Context Management

## Keeping Context Efficient

Context windows are finite. Follow these practices to maximize useful context per session.

## What to Load Early

At session start, load:
1. `CLAUDE.md` — master configuration
2. `design/pillars.md` — core creative direction
3. The current sprint file: `production/sprints/sprint-[N].md`
4. The specific design doc for the feature being worked on

Do NOT pre-load the entire codebase — use `Read` and `Glob` to load only relevant files.

## When Context Gets Long

Before context window fills:
1. Summarize completed work in `production/session-state/active.md`
2. Document open questions and blockers in the same file
3. Run `/pre-compact` to trigger the pre-compact hook

## File Read Order for Feature Work

1. Design doc for the feature
2. Interface definitions the feature implements
3. Related existing systems (for consistency)
4. Only then: the files you will edit

## Avoid Context Waste

- Don't read files you won't reference
- Don't ask agents to summarize files you haven't read yet
- Don't load large data files (JSON with 1000+ entries) unless specifically needed

## Session Recovery

If a session was interrupted, check:
- `production/session-state/active.md` — last active state
- `git status` — what was changed
- `git log --oneline -10` — recent commits
