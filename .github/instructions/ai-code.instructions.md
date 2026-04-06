---
applyTo:
  - "Assets/Scripts/AI/**/*.cs"
---

# Unity AI Code Rules

- AI behavior must be data-driven — behavior parameters come from ScriptableObjects
- State machines must have explicit, documented state tables
- No AI logic in MonoBehaviour `Update()` — use Unity's AI/NavMesh callbacks or a custom tick system
- AI must not directly access player data — use interfaces (`ITargetable`, `IDetectable`)
- NavMesh usage: always check `NavMeshAgent.pathStatus` before acting on a path
- Pathfinding calls are expensive — cache and reuse paths; recalculate only when target moves significantly
- AI Sensors (sight, hearing) must be profiled — raycasts from every AI every frame is prohibited
- Use Unity's Job System for parallelizable AI calculations (pathfinding pre-processing, FOV checks)
- Document the behavioral goal of each AI state with XML doc comments

## NavMesh Standards

- Always check `NavMeshAgent.pathStatus` before acting on a path
- `NavMeshAgent.isStopped` for pausing without destroying path
- Use `NavMeshObstacle` (not static colliders) for dynamic obstacles
- Bake NavMesh per quality tier — mobile may use simplified mesh

## AI Performance

- Tick rate LOD: nearby AI at 10Hz; distant AI at 1Hz; off-screen at 0.1Hz
- AI updates in a custom `AIManager` — not in individual MonoBehaviour `Update()`
- Profile with Unity Profiler: AI should consume < 2ms per frame (PC target)

## Role Context

You are acting as an **AI Programmer** when working with these files.

Focus on believable, data-driven AI behavior. Performance is critical — never allow uncapped per-frame AI raycasts.

## 🚨 CRITICAL: Approval Required Protocol

You MUST follow this workflow:

1. **Read First** — Understand existing AI architecture and design specs
2. **Ask Questions** — Clarify AI behavior requirements before proposing
3. **Propose Implementation** — Show behavior tree/state machine structure, get user approval
4. **Request Permission** — "May I write to [filepath]?" — WAIT for explicit approval
5. **Implement** — Only after user says "yes" or "proceed"

⛔ NEVER write or modify files without explicit user approval.
⛔ NEVER assume approval — always ask first.
