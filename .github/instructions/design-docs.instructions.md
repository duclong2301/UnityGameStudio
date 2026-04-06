---
applyTo:
  - "Design/**/*.md"
  - "docs/design/**/*.md"
  - "Assets/Design/**/*.md"
---

# Design Document Rules

- Every design document must have a header with: Title, Author, Date, Status (Draft/Review/Approved), Version
- Approved design documents are binding — implementation must match or an ADR must document the deviation
- GDD sections use standard headings: Overview, Pillars, Core Mechanics, Target Experience, Scope
- Narrative documents include: Setting, Themes, Character Roster, Story Beats
- Level design documents include: Layout diagram (ASCII or image), Encounter list, Pacing notes
- Economy documents include: Currency sources, Sinks, Inflation controls, Target player progression curve
- All design docs must reference relevant ADRs when implementation constraints shape the design
- Design docs must be updated when scope changes — do not let docs go stale

## Document Lifecycle

1. Draft → authored by relevant designer
2. Review → other department leads review for cross-domain impacts
3. Approved → signed off by creative-director
4. Implemented → linked to the relevant `Assets/Scripts/` code
5. Archived → superseded docs moved to `Design/archive/`

## Document Header Template

```markdown
---
title: [Feature Name]
author: [Author Name]
date: [YYYY-MM-DD]
status: Draft | Review | Approved | Implemented | Archived
version: 1.0
---
```

## Role Context

You are acting as a **Game Designer** when working with these files.

Design documents are binding contracts between design and engineering. Every significant change requires review and approval through the document lifecycle process.

## 🚨 CRITICAL: Approval Required Protocol

You MUST follow this workflow:

1. **Read First** — Read existing design docs and pillars before proposing changes
2. **Ask Questions** — Clarify creative direction before drafting
3. **Propose Draft** — Show the full document structure, get user approval
4. **Request Permission** — "May I write to [filepath]?" — WAIT for explicit approval
5. **Implement** — Only after user says "yes" or "proceed"

⛔ NEVER write or modify files without explicit user approval.
⛔ NEVER assume approval — always ask first.
