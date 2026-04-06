---
applyTo:
  - "Assets/Scripts/Core/**/*.cs"
  - "Assets/Scripts/Engine/**/*.cs"
  - "Assets/Scripts/Infrastructure/**/*.cs"
  - "Assets/Scripts/Bootstrap/**/*.cs"
---

# Unity Engine/Core Code Rules

- Engine-layer code must be engine-agnostic where possible — no MonoBehaviour references in pure data/logic classes
- Use assembly definitions (`.asmdef`) for ALL code folders — never rely on implicit compilation order
- `Awake()` for component initialization and caching references; `Start()` only when inter-component init order matters
- `OnEnable()` / `OnDisable()` for subscribing and unsubscribing events — prevent memory leaks
- `OnDestroy()` must unsubscribe all events and release all handles (Addressable handles, native arrays)
- Use `[RuntimeInitializeOnLoadMethod]` for low-level system bootstrapping, not `Awake()` in scene objects
- Respect script execution order — define it explicitly via `[DefaultExecutionOrder]` attribute
- Engine systems must not depend on gameplay systems (dependency goes gameplay → engine, not reverse)

## Memory and GC Rules

- Avoid heap allocations in hot paths: no `new List<>()` / `new string()` in `Update()`
- Use `NativeArray<T>`, `NativeList<T>` for bulk data in performance-critical paths
- String formatting: use `StringBuilder` or interpolated strings with `$"..."` — avoid `string.Concat` in loops
- Pool all frequently created objects — use `UnityEngine.Pool.ObjectPool<T>`

## Assembly Definition Rules

- Every `Assets/Scripts/` subfolder must have a `.asmdef` file
- Assemblies must declare their dependencies explicitly — no `allowUnsafeCode` unless required
- Editor-only code goes in `Editor/` folders with editor-only `.asmdef`

## Dependency Direction

```
Core (Engine Layer)
  ↑ depends on
Gameplay Systems
  ↑ depends on
UI / Audio / Network
```

Nothing above Core should know about UI or gameplay-specific systems.

## Role Context

You are acting as an **Engine Programmer** when working with these files.

Engine-layer changes have wide impact — propose carefully, document the bootstrap sequence, and ensure no circular dependencies are introduced.

## 🚨 CRITICAL: Approval Required Protocol

You MUST follow this workflow:

1. **Read First** — Understand existing architecture and assembly structure
2. **Ask Questions** — Clarify requirements before proposing infrastructure changes
3. **Propose Implementation** — Show class structure and dependency graph, get user approval
4. **Request Permission** — "May I write to [filepath]?" — WAIT for explicit approval
5. **Implement** — Only after user says "yes" or "proceed"

⛔ NEVER write or modify files without explicit user approval.
⛔ NEVER assume approval — always ask first.
