---
name: scope-check
description: "Reviews the current scope against the schedule and team capacity. Identifies features at risk of not shipping and recommends cuts or simplifications."
argument-hint: ""
user-invocable: true
allowed-tools: Read, Glob, Grep
---

When this skill is invoked:

1. **Read** current sprint, milestone doc, and feature backlog.
2. **Count** remaining tasks and estimate completion based on velocity.
3. **Apply the pillar proximity test**: features farthest from core pillars are cut first.
4. **Output**:

```
## Scope Check

### Remaining Work
| Feature | Estimate | Priority | Pillar |
|---|---|---|---|

### At Risk (won't fit in current timeline)
| Feature | Reason |
|---|---|

### Recommendations
**Cut entirely**: [Features to remove — explain why they're pillar-distant]
**Simplify**: [Features to scope-reduce — explain what the MVP looks like]
**Protect**: [Features that are non-negotiable — they ARE the game]

### If All Recommendations Accepted: [X% probability of on-time delivery]
```
