---
paths:
  - "design/**/*.md"
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
4. Implemented → linked to the relevant src/ code
5. Archived → superseded docs moved to `design/archive/`
