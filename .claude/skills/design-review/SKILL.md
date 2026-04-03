---
name: design-review
description: "Reviews a game design document or feature spec against the project's pillars, scope, and technical constraints. Checks completeness, clarity, feasibility, and pillar alignment."
argument-hint: "[path-to-design-doc]"
user-invocable: true
allowed-tools: Read, Glob, Grep
---

When this skill is invoked:

1. **Read the design document** provided as argument.

2. **Read `design/pillars.md`** — every design must trace to a pillar.

3. **Check document completeness**:
   - [ ] Has: Title, Author, Date, Status, Version
   - [ ] Has: Overview / what problem this solves
   - [ ] Has: Mechanic description (step by step)
   - [ ] Has: Edge cases documented
   - [ ] Has: Success metrics / acceptance criteria
   - [ ] Has: Scope (MVP vs. full vision)

4. **Check pillar alignment**:
   - For each design element: "Which pillar does this serve?"
   - Flag any elements that serve no pillar ("pillar orphan")
   - Flag if a pillar is not served by any feature in this doc

5. **Check technical feasibility**:
   - Does this require systems that don't exist? (flag for `lead-programmer`)
   - Does this require DOTS/ECS scale? (flag for `unity-dots-specialist`)
   - Does this require real-time multiplayer? (flag for `network-programmer`)
   - Are the UI requirements clear? (flag for `unity-ui-specialist`)

6. **Check scope**:
   - Is the MVP clearly scoped?
   - Are there gold-plating features that could be cut?
   - Is the design creeping beyond its stated scope?

7. **Output the review**:

```
## Design Review: [Document Name]

### Completeness: [X/6 sections present]
[Missing sections]

### Pillar Alignment: [STRONG / PARTIAL / MISALIGNED]
[List: Feature → Pillar mapping]
[List: Pillar orphans (features with no pillar)]
[List: Unserved pillars]

### Technical Feasibility: [FEASIBLE / CONCERNS / BLOCKERS]
[Technical flags for engineering review]

### Scope Assessment: [SCOPED / CREEP DETECTED]
[Overscoped elements to consider cutting]

### Positive Observations
[What is well-designed in this doc]

### Required Changes
[Must-fix before implementation can begin]

### Verdict: [APPROVED / REVISIONS NEEDED / FUNDAMENTAL RETHINK]
```
