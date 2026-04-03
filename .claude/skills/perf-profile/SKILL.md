---
name: perf-profile
description: "Guides a performance profiling session for a Unity project. Identifies performance targets, profiles the specified area, and produces an optimization report with prioritized recommendations."
argument-hint: "[area: gameplay|rendering|memory|audio|loading|all]"
user-invocable: true
allowed-tools: Read, Glob, Grep, Bash
---

When this skill is invoked:

1. **Read performance budgets** from `docs/engine-reference/unity/VERSION.md` (or defaults from `.claude/docs/technical-preferences.md`).

2. **Identify the target area** from the argument (or ask if not specified):
   - `gameplay` — CPU: MonoBehaviour Update loops, AI, physics
   - `rendering` — GPU: draw calls, overdraw, shader complexity
   - `memory` — heap allocations, GC pressure, asset memory
   - `audio` — audio voice count, audio memory, streaming
   - `loading` — Addressables load times, scene transition time
   - `all` — full profile pass

3. **For each area, provide a profiling guide**:

### Gameplay CPU
```
Unity Profiler Steps:
1. Window > Analysis > Profiler > CPU Usage
2. Deep Profile: enable for method-level breakdown
3. Look for: Update() methods consuming > 1ms
4. Check GC.Alloc column — should be 0 during gameplay
5. Filter by: PlayerLoop > Update > ScriptRunBehaviourUpdate
```

### Rendering GPU
```
Frame Debugger Steps:
1. Window > Analysis > Frame Debugger
2. Check draw call count (target: < 2000 PC, < 500 mobile)
3. Look for: overdraw (transparent layers), shadow casters, redundant passes
Unity Profiler > GPU Usage for timing
```

### Memory
```
Memory Profiler (package: com.unity.memoryprofiler):
1. Take snapshot during gameplay (not in menu)
2. Compare snapshot before and after a gameplay session
3. Look for: leaked assets, growing native allocations
4. GC.Alloc in CPU Profiler = managed heap allocations
```

4. **Request Profiler results** from the user (paste key findings).

5. **Analyze and output a report**:

```
## Performance Profile Report: [Area]

### Targets vs. Actual
| Metric | Target | Actual | Status |
|---|---|---|---|
| Frame time | 16.67ms | Xms | ✅/⚠️/❌ |

### Top Bottlenecks
1. [Bottleneck]: [Impact] — [Root cause]
2. [Bottleneck]: [Impact] — [Root cause]

### Optimization Recommendations (Priority Order)
1. **[Fix]** — Expected improvement: [X]ms / [X]MB
   Implementation: [How to fix, which agent to delegate to]

### Next Steps
[Which agent should handle each optimization]
```
