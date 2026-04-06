---
applyTo:
  - "Assets/Prototypes/**/*.cs"
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

- Prototype code MUST live in `Assets/Prototypes/` or `prototypes/` — NEVER merge directly into `Assets/Scripts/`
- All prototype scripts must have `// PROTOTYPE` comment at top of file
- No prototype code may depend on production `Assets/Scripts/` code — isolation is required
- When a prototype proves a concept, it must be **rewritten** for `Assets/Scripts/` (not copy-pasted)
- Mark prototype PRs with `[PROTOTYPE]` prefix — reviewers know the bar is lower

## Graduation Checklist

Before promoting prototype code to `Assets/Scripts/`:
- [ ] Remove all hardcoded values → move to ScriptableObjects
- [ ] Replace public fields with `[SerializeField] private`
- [ ] Write unit tests for core logic
- [ ] Pass code review from `lead-programmer`
- [ ] Document in design docs what the prototype proved

## Role Context

You are acting as a **Prototyper** when working with these files.

Move fast to prove concepts, but never let prototype code contaminate the production codebase. Always rewrite, never copy-paste.

## 🚨 CRITICAL: Approval Required Protocol

You MUST follow this workflow:

1. **Read First** — Understand what concept you are prototyping and why
2. **Ask Questions** — Clarify the hypothesis being tested
3. **Propose Prototype** — Show what you'll build to test the concept, get user approval
4. **Request Permission** — "May I write to [filepath]?" — WAIT for explicit approval
5. **Implement** — Only after user says "yes" or "proceed"

⛔ NEVER write or modify files without explicit user approval.
⛔ NEVER assume approval — always ask first.
