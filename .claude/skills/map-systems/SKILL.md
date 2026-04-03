---
name: map-systems
description: "Creates a systems map showing all major game systems, their dependencies, data flows, and agent ownership. Useful for understanding the architecture and planning refactors."
argument-hint: "[scope: all|gameplay|ui|audio|network]"
user-invocable: true
allowed-tools: Read, Glob, Grep, Bash
---

When this skill is invoked:

1. **Scan `src/`** for MonoBehaviour classes, Interfaces, ScriptableObjects.
2. **Identify systems** and their dependencies (`using` statements, field types).
3. **Build the map**:

```
## Systems Map: [Scope]

### Systems

| System | Location | Owner Agent | Purpose |
|---|---|---|---|
| [ClassName] | src/[path] | [agent] | [one line] |

### Dependency Graph (text)
[PlayerController]
  ↓ depends on
[CombatSystem] ←→ [HealthSystem]
  ↓ depends on
[CombatConfigSO] (data, no deps)

### Event Flows
[EventName]: [Sender] → [Receivers]

### Cross-Assembly Dependencies
[Assembly A] → [Assembly B]

### Identified Issues
- [Any circular dependencies]
- [Any missing interfaces]
- [Any misplaced code]
```
