---
paths:
  - "src/AI/**/*.cs"
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
