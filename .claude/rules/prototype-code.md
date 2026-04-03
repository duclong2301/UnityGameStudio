---
paths:
  - "prototypes/**/*.cs"
---

# Unity Prototype Code Rules

Prototype code is intentionally fast and loose. These rules protect the main codebase
from prototype contamination while allowing rapid iteration.

## Prototype Permissions (allowed ONLY in prototypes/)

- Hardcoded values are acceptable for speed
- Public fields instead of `[SerializeField] private` are acceptable
- Static singletons are acceptable for prototype scope
- Minimal error handling — fail fast and loudly
- No unit tests required (but Play Mode smoke tests are encouraged)

## Prototype Constraints (non-negotiable)

- Prototype code MUST live in `prototypes/` — NEVER merge directly into `src/`
- All prototype scripts must have `// PROTOTYPE` comment at top of file
- No prototype code may depend on production `src/` code — isolation is required
- When a prototype proves a concept, it must be **rewritten** for `src/` (not copy-pasted)
- Mark prototype PRs with `[PROTOTYPE]` prefix — reviewers know the bar is lower

## Graduation Checklist

Before promoting prototype code to `src/`:
- [ ] Remove all hardcoded values → move to ScriptableObjects
- [ ] Replace public fields with `[SerializeField] private`
- [ ] Write unit tests for core logic
- [ ] Pass code review from `lead-programmer`
- [ ] Document in design docs what the prototype proved
