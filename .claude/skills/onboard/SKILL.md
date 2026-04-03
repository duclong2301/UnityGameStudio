---
name: onboard
description: "Onboards a new team member or AI agent to the project. Explains the game concept, architecture, current state, and where to start."
argument-hint: "[role: programmer|designer|artist|qa|all]"
user-invocable: true
allowed-tools: Read, Glob, Grep
---

When this skill is invoked:

1. **Read context**:
   - `design/pillars.md` — game concept
   - `CLAUDE.md` — tech stack
   - Current sprint — what's being worked on
   - Recent commits — what just changed

2. **Provide an onboarding summary**:

```
## Project Onboarding: [Project Name]

### The Game
[1-paragraph description from pillars.md]

### Tech Stack
- Engine: Unity [version]
- Render Pipeline: [URP/HDRP]
- Language: C#
- Key packages: [from VERSION.md]

### Project Structure
[Summary from directory-structure.md]

### Current Status
- Active sprint: [Sprint N]
- Sprint goal: [Goal]
- Recent changes: [Last 5 commits]

### Where to Start (by role)
[Programmer]: Read `src/Core/` first; understand the event bus and scene loading
[Designer]: Read `design/pillars.md` then `design/gdd/`
[Artist]: Read art standards in `.claude/docs/coding-standards.md` and `art-director` agent
[QA]: Read `tests/` structure and QA standards in `.claude/rules/test-standards.md`

### Agent Roster
[Link to agent list — agents available for this project]

### First Tasks
[2-3 suggested starting tasks appropriate for the role]
```
