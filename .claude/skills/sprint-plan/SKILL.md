---
name: sprint-plan
description: "Creates a sprint plan for the next development sprint. Reads current milestone, backlog, and team velocity to generate a realistic sprint with task breakdown, estimates, and risk flags."
argument-hint: "[sprint-number or 'next']"
user-invocable: true
allowed-tools: Read, Glob, Grep, Write, Edit
---

When this skill is invoked:

1. **Read context**:
   - Current milestone: `production/milestones/` (find most recent)
   - Previous sprint: `production/sprints/` (find most recent)
   - Backlog: `production/backlog.md` (if exists)
   - Open bugs: search for `BUG-*.md` in `production/` and `tests/`

2. **Determine the sprint number** — next after the most recent sprint, or from argument.

3. **Ask the user**:
   - "What is the primary goal for this sprint?"
   - "What is the team capacity in person-hours? (or just number of working days)"
   - "Are there any committed hard deadlines in this sprint?"

4. **Generate a sprint plan** following this format:

```markdown
# Sprint [N] — [Estimated Dates]

## Sprint Goal
[One sentence: what will be true at the end of this sprint?]

## Capacity
[X person-days / Y person-hours available]

## Committed Tasks

| # | Task | Owner | Estimate | Priority |
|---|---|---|---|---|
| 1 | [Feature/fix description] | [Agent/Person] | [Xh] | P[1-4] |

**Total Estimated**: [X]h / [Y]h available

## Carryover from Previous Sprint
[Any unfinished tasks from previous sprint]

## Risks
| Risk | Likelihood | Impact | Mitigation |
|---|---|---|---|
| [Risk description] | High/Med/Low | High/Med/Low | [Mitigation strategy] |

## Definition of Done
- [ ] All committed tasks complete or explicitly descoped
- [ ] QA sign-off on all new features
- [ ] No new P1 bugs introduced
- [ ] Retrospective held

## Out of Scope (Backlog)
- [Features explicitly NOT in this sprint]
```

5. **Ask for approval** before writing the sprint file to `production/sprints/sprint-[N].md`.
