---
name: team-level
description: "Orchestrates a full team to design and build a complete game level. Coordinates level-designer, world-builder, ai-programmer, sound-designer, performance-analyst, and qa-tester."
argument-hint: "[level name or description]"
user-invocable: true
allowed-tools: Read, Glob, Grep, Write, Edit, Task
---

When this skill is invoked:

1. **Read** `design/pillars.md` and any existing level design docs.

2. **Confirm scope**:
   - "Is this a new level or updating an existing one?"
   - "Genre context: exploration, combat, puzzle, or mixed?"
   - "Approximate player time target?"

3. **Sequential phases** (level design must precede world building):

   **Phase 1 (design)**:
   - **level-designer**: Create `design/levels/[LevelName].md`
     - Layout, encounters, pacing, NavMesh notes
   
   **Phase 2 (parallel, after design approved)**:
   - **world-builder**: Scene construction, lighting, dressing
   - **ai-programmer**: Enemy spawn points and patrol routes
   - **sound-designer**: Ambient audio and encounter triggers
   
   **Phase 3 (validation)**:
   - **performance-analyst**: Profile the built level
   - **qa-tester**: Playthrough test — all encounters, collectibles, exits

4. **Summary** after each phase — get user approval before proceeding.
