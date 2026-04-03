---
name: design-system
description: "Creates or reviews a complete design system document for a specified game system. Covers mechanics, data model, edge cases, and success criteria."
argument-hint: "[system name: combat|movement|inventory|progression|dialogue|etc.]"
user-invocable: true
allowed-tools: Read, Glob, Grep, Write, Edit
---

When this skill is invoked:

1. **Read `design/pillars.md`** — the system must serve at least one pillar.
2. **Check for existing documentation** in `design/gdd/`.
3. **Ask the user** for key design goals if no existing doc.

4. **Create or update `design/gdd/[system]-system.md`**:

```markdown
# [System Name] System Design
**Author**: [User]
**Date**: [Today]
**Status**: Draft
**Version**: 1.0

## Overview
[What is this system? What player need does it serve?]

## Design Goals
1. [Measurable outcome: "Player feels powerful when..."]
2. [Measurable outcome]
3. [Measurable outcome]

## Pillar Connection
[Which pillar(s) does this system serve? Quote from pillars.md]

## Core Mechanics
[Step-by-step: how does this work from the player's perspective?]

## Data Model
| Parameter | Type | Range | Default | Description |
|---|---|---|---|---|

## Edge Cases
| Scenario | Expected Behavior |
|---|---|

## Success Metrics
[How do we know this system is working? Playtest criteria]

## MVP Scope
[Minimum viable version for Alpha]

## Future Scope (post-MVP)
[Stretch features for later sprints]

## Open Questions
- [ ] [Unresolved design decision]
```

5. **Ask for approval** before writing.
