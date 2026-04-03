---
name: estimate
description: "Creates time estimates for a feature or task list using structured estimation techniques. Produces estimates with confidence intervals."
argument-hint: "[feature or task description]"
user-invocable: true
allowed-tools: Read, Glob, Grep
---

When this skill is invoked:

1. **Break down the task** into implementation sub-tasks (design, code, test, polish).
2. **Apply three-point estimation** for each sub-task:
   - Optimistic (O): Everything goes perfectly
   - Most Likely (M): Realistic estimate
   - Pessimistic (P): Known risks materialize
   - PERT formula: E = (O + 4M + P) / 6

3. **Output**:

```
## Estimate: [Feature Name]

### Assumptions
- [What must be true for these estimates to hold]

### Task Breakdown
| Task | O (hrs) | M (hrs) | P (hrs) | E (hrs) |
|---|---|---|---|---|
| Design doc | 1 | 2 | 4 | 2.2 |
| Implementation | 4 | 8 | 16 | 8.7 |
| Tests | 1 | 2 | 4 | 2.2 |
| **TOTAL** | | | | **13.1** |

### Confidence
- 80% confidence: [E] hours
- 95% confidence: [E + 2σ] hours

### Risks
- [Risk]: adds [X] hours if it materializes

### Recommended Sprint Allocation: [X] days
```
