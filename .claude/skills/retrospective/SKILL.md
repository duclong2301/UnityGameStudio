---
name: retrospective
description: "Facilitates a sprint retrospective. Analyzes the sprint, identifies what went well, what didn't, and creates action items for the next sprint."
argument-hint: "[sprint number or 'last']"
user-invocable: true
allowed-tools: Read, Glob, Grep, Write
---

When this skill is invoked:

1. **Read** the sprint file for the specified sprint.
2. **Count** completed vs. committed tasks.
3. **Read** bugs filed during the sprint.

4. **Ask the user for input** (classic retrospective format):
   - "What went well this sprint?"
   - "What didn't go well?"
   - "What will we do differently next sprint?"

5. **Create retrospective document** at `production/retrospectives/retro-sprint-[N].md`:

```markdown
# Retrospective — Sprint [N]

## Sprint Summary
- Committed tasks: X | Completed: Y | Completion rate: Z%
- Bugs filed: X | Bugs resolved: Y

## What Went Well 🟢
- [Item]

## What Didn't Go Well 🔴
- [Item]

## Root Causes
| Issue | Root Cause | Category (process/tech/communication) |
|---|---|---|

## Action Items
| Action | Owner | By When |
|---|---|---|
| [Improvement] | [Person/Agent] | Sprint [N+1] |

## Carry Forward
[Any unresolved action items from previous retro]
```

6. **Ask for approval** before writing.
