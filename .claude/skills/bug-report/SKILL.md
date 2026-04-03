---
name: bug-report
description: "Creates a structured bug report from a description. Guides the user through providing all necessary reproduction steps and context."
argument-hint: "[brief bug description]"
user-invocable: true
allowed-tools: Read, Glob, Grep, Write, Bash
---

When this skill is invoked:

1. **Generate a bug ID** — find highest existing BUG number in `production/` and `tests/`, increment by 1. Start at BUG-001 if none exist.

2. **Ask for details** (if not provided in argument):
   - "What steps reproduce this bug?"
   - "What did you expect to happen?"
   - "What actually happened?"
   - "How often does it occur? (always / sometimes / rarely)"
   - "Build number and platform?"
   - "Priority? (P1 crash/data loss | P2 major feature broken | P3 minor issue | P4 cosmetic)"

3. **Create the bug report** at `production/bugs/BUG-[ID].md`:

```markdown
# BUG-[ID]: [Short, specific title from argument]
**Priority**: P[1-4]
**Status**: Open
**Reported**: [Today's date]
**Build**: [Unity version + build number]
**Platform**: [PC/Mobile/Console]

## Steps to Reproduce
1. [Step]
2. [Step]
3. [Step]

## Expected Result
[What should happen]

## Actual Result
[What actually happens]

## Frequency
[Always / Sometimes (X/10 attempts) / Rarely]

## Notes
[Any additional context, screenshots requested, workarounds]
```

4. **Ask for approval** before writing the file.

5. **After writing**, suggest: "Route this to `qa-lead` for triage, or directly to the relevant programmer if you know the system involved."
