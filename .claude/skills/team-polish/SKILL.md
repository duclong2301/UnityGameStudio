---
name: team-polish
description: "Orchestrates a polish pass across the game: game feel, visual polish, audio polish, UX improvements, and performance."
argument-hint: "[area: feel|visuals|audio|ux|performance|all]"
user-invocable: true
allowed-tools: Read, Glob, Grep, Write, Edit, Task
---

When this skill is invoked:

1. **Read** recent playtest reports and bug list for polish targets.

2. **Launch polish specialists in parallel**:

   - **gameplay-programmer**: Game feel
     - Hit stop, screen shake, input responsiveness, juice
     
   - **unity-shader-specialist**: Visual polish
     - VFX improvements, shader tweaks, post-processing
     
   - **sound-designer**: Audio polish
     - Missing SFX, audio mixing, impact sounds
     
   - **ux-designer**: UX polish
     - Menu transitions, tooltip improvements, feedback clarity
     
   - **performance-analyst**: Performance polish
     - Eliminate remaining frame spikes, GC spikes

3. **Each specialist produces a polish list** — prioritized by impact per effort.

4. **Consolidate** into a single sprint-ready polish backlog.

5. **Present to user** — approve the priority order before work begins.
