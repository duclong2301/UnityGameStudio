---
name: playtest-report
description: "Creates a structured playtest report from session observations. Documents player behavior, friction points, and actionable feedback for the design and development team."
argument-hint: "[playtest session description or notes]"
user-invocable: true
allowed-tools: Read, Glob, Grep, Write
---

When this skill is invoked:

1. **Ask for playtest details** if not provided:
   - Who were the playtesters? (internal / external / target audience)
   - What build version?
   - What was the focus area?
   - What session notes or observations are available?

2. **Create the report** at `tests/playtests/playtest-[YYYY-MM-DD].md`:

```markdown
# Playtest Report — [Date]
**Build**: v[Version]
**Testers**: [Internal / External / N testers]
**Focus**: [Feature or milestone being tested]
**Duration**: [X minutes per session]

## Executive Summary
[2-3 sentences: overall finding, most critical issue, biggest positive]

## Player Behavior Observations
| Behavior | Frequency | Implication |
|---|---|---|
| [What players did] | Always/Often/Sometimes/Once | [What this tells us] |

## Friction Points
| Friction | Severity | Location | Root Cause (hypothesis) |
|---|---|---|---|
| [Issue] | High/Med/Low | [Screen/System] | [Design / UX / Bug] |

## What Worked Well
- [Positive observations with evidence]

## Action Items
| Action | Owner | Priority |
|---|---|---|
| [What to change] | [Agent/Person] | P[1-4] |

## Appendix
[Raw notes if available]
```

3. **Ask for approval** before writing the file.
