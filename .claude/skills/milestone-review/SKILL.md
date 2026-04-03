---
name: milestone-review
description: "Reviews milestone completion criteria against actual project state. Produces a go/no-go recommendation with specific criteria assessments."
argument-hint: "[milestone: alpha|beta|gold|release]"
user-invocable: true
allowed-tools: Read, Glob, Grep, Bash
---

When this skill is invoked:

1. **Read the milestone document** from `production/milestones/[milestone].md`.
2. **Read the current sprint** from `production/sprints/` (most recent).
3. **Count open bugs** from `BUG-*.md` files by priority.
4. **Scan src/ for TODO/FIXME** counts.
5. **Check design docs** — are all features documented?

6. **Output the milestone review**:

```
## Milestone Review: [Milestone Name]

### Feature Completeness
| Feature | Status | Notes |
|---|---|---|
| [Feature] | ✅ Complete / ⚠️ Partial / ❌ Missing | |

### Bug Status
| Priority | Count | Target |
|---|---|---|
| P1 Critical | X | 0 |
| P2 High | X | 0 |
| P3 Medium | X | < 10 |

### Technical Health
- TODO count in src/: X
- FIXME count in src/: X
- Test coverage: [estimated %]

### Documentation
- [ ] GDD complete
- [ ] ADRs for major decisions
- [ ] Sprint retrospective held

### Verdict: [GO ✅ / CONDITIONAL GO ⚠️ / NO-GO ❌]
[Reasoning and blockers if no-go]
[Conditions if conditional go]
```
