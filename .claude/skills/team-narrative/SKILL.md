---
name: team-narrative
description: "Orchestrates a full team to create narrative content for a scene or feature. Coordinates narrative-director, writer, sound-designer, and localization-lead."
argument-hint: "[scene or narrative feature description]"
user-invocable: true
allowed-tools: Read, Glob, Grep, Write, Edit, Task
---

When this skill is invoked:

1. **Read** `design/narrative/` docs and character voice guides.

2. **Confirm with user**:
   - "What type of narrative content? (cutscene / dialogue / environmental / quest)"
   - "Which characters are involved?"
   - "What story beat does this serve?"

3. **Phase 1 — Narrative direction** (narrative-director):
   - Scene purpose, emotional beat, character goals
   - Story beat connection to main arc

4. **Phase 2 — Content creation** (parallel, after direction approved):
   - **writer**: Draft all dialogue and text content
   - **sound-designer**: VO direction notes, ambient audio needs
   - **localization-lead**: String key assignments, context documentation

5. **Phase 3 — Review**:
   - narrative-director reviews writer output for voice consistency
   - Present final draft to user for approval

6. **After approval**: content ready for implementation by `ui-programmer` (dialogue UI) and `sound-designer` (audio).
