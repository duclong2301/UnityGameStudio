---
name: project-stage-detect
description: "Analyzes an existing project to detect its current development stage and what's needed next. Useful when joining an existing project or after a long break."
argument-hint: ""
user-invocable: true
allowed-tools: Read, Glob, Grep, Bash
---

When this skill is invoked:

1. **Scan the project**:
   - Count `.cs` files in `src/`
   - Check for `design/pillars.md`
   - Check for `design/gdd/`
   - Check for `docs/engine-reference/unity/VERSION.md`
   - Check for sprint files in `production/sprints/`
   - Count open bugs
   - Look at git log for recent activity

2. **Determine the stage** based on findings:
   - **Concept**: Ideas only, no code or GDD
   - **Pre-Production**: GDD exists, engine configured, little or no code
   - **Production — Early**: Core systems building, < 20% features implemented
   - **Production — Mid**: Most systems built, content being created
   - **Production — Late**: Feature-complete, in QA and polish
   - **Post-Launch**: Shipped, in live-ops or maintenance

3. **Output**:

```
## Project Stage Analysis

### Detected Stage: [Stage Name]

### Evidence
[What was found that led to this assessment]

### Missing Documentation
[docs/pillars missing, GDD incomplete, etc.]

### Recommended Next Steps
1. [Most important next action]
2. [Second most important]
3. [Third]

### Suggested Skills to Run
- /[skill] — [why]
```
