---
name: start
description: "Guided onboarding workflow — determine where you are in the project lifecycle and route to the right workflow. Run this first if you have no game concept yet, or if you want to assess where your project is and what to do next."
argument-hint: ""
user-invocable: true
allowed-tools: Read, Glob, Grep, Write, Edit, Task
---

When this skill is invoked:

1. **Check the project state** — look for signs of existing work:
   - Does `design/pillars.md` exist? → Project has design direction
   - Does `docs/engine-reference/unity/VERSION.md` exist? → Engine is configured
   - Does `src/` have any `.cs` files? → Code work has started
   - Does `production/sprints/` have any sprint files? → Production is underway

2. **Ask the user where they are**:

   ```
   Where are you in your project?
   
   A) No idea — I want to brainstorm game concepts from scratch
   B) Vague concept — I have an idea but no documentation yet
   C) Clear design — I have a GDD but haven't started building
   D) Existing project — I have code/assets and want to continue or audit
   E) Specific task — I know what I need (tell me and I'll route you)
   ```

3. **Route based on answer**:

   - **A — No idea**: Run `/brainstorm` to explore game concepts
   - **B — Vague concept**: 
     - Create `design/pillars.md` (3-5 core pillars)
     - Create `design/gdd/game-design-document.md` outline
     - Then run `/setup-engine unity [version]`
   - **C — Clear design**:
     - Run `/setup-engine unity [version]` to configure the Unity environment
     - Then run `/sprint-plan` to plan the first sprint
   - **D — Existing project**:
     - Run `/project-stage-detect` to assess where you are
     - Run `/reverse-document` if docs are missing
   - **E — Specific task**: Ask what they need and delegate to the right agent

4. **After routing, confirm the next step** with the user before proceeding.
