---
name: balance-check
description: "Reviews game balance for a specified system (combat, economy, progression, difficulty). Analyzes data, identifies imbalances, and proposes tuning adjustments."
argument-hint: "[system: combat|economy|progression|difficulty]"
user-invocable: true
allowed-tools: Read, Glob, Grep
---

When this skill is invoked:

1. **Read relevant data files** from `assets/data/[system]/`.
2. **Read design doc** for the system's intended balance goals.
3. **Analyze**:
   - Are there dominant strategies with no meaningful counters?
   - Are any options clearly inferior (never chosen by rational players)?
   - Is the difficulty curve smooth, or are there sudden spikes/valleys?
   - Do rewards match the effort/risk required?
4. **Output**:

```
## Balance Check: [System]

### Current State
[Summary of the system's current parameters]

### Issues Found
| Issue | Severity | Impact |
|---|---|---|
| [Description] | High/Med/Low | [What it breaks] |

### Recommendations
| Parameter | Current | Suggested | Reasoning |
|---|---|---|---|
| [Param] | X | Y | [Why] |

### Tuning Priority
1. [Most impactful change]
2. [Second most impactful]
```
