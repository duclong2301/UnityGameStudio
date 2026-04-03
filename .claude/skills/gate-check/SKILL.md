---
name: gate-check
description: "Runs a milestone gate check — validates that a project meets the criteria to advance to the next development stage (Pre-Alpha → Alpha → Beta → Gold)."
argument-hint: "[current stage: concept|pre-alpha|alpha|beta]"
user-invocable: true
allowed-tools: Read, Glob, Grep, Bash
---

When this skill is invoked:

1. **Determine the stage** and the gate criteria:

| Stage | Gate Criteria |
|---|---|
| **Concept → Pre-Alpha** | Pillars defined, GDD outline, engine configured, team formed |
| **Pre-Alpha → Alpha** | Core mechanics playable, no placeholder for pillar systems, team velocity established |
| **Alpha → Beta** | All features implemented (rough), content skeleton complete, 0 P1 bugs |
| **Beta → Gold** | Feature-complete, content-complete, 0 P1/P2 bugs, certified on all target platforms |

2. **Check each criterion**:
   - Does `design/pillars.md` exist?
   - Does `docs/engine-reference/unity/VERSION.md` exist?
   - Count open P1 and P2 bugs
   - Check feature completeness vs. design doc

3. **Output gate check report**:

```
## Gate Check: [Current Stage] → [Next Stage]

### Criteria
| Criterion | Status | Evidence |
|---|---|---|
| [Criterion] | ✅ Pass / ❌ Fail / ⚠️ Partial | [Evidence] |

### Open Blockers
[List of failing criteria that must be resolved]

### Verdict: [ADVANCE ✅ / NOT READY ❌]
[Required actions to pass]
```
