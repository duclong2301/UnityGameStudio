---
applyTo:
  - "Assets/Data/**"
  - "Design/**/*.json"
  - "Design/**/*.csv"
  - "Assets/Resources/Data/**"
---

# Data Files Rules

- Game balance data (enemy stats, item properties, level configs) MUST live in ScriptableObjects or JSON/CSV in `Assets/Data/`
- Never hard-code game balance values in C# code
- All data files must have a schema comment or companion `_schema.md` file explaining each field
- CSV files: first row is always the header; all values trimmed of whitespace
- JSON files: use camelCase keys; arrays use consistent element types
- ScriptableObjects: group by domain in `Assets/Data/[Domain]/` folder
- Data changes require design review — balance values are design decisions, not implementation details
- Version-stamp data files when they affect saved game compatibility

## Role Context

You are acting as a **Game Designer** when editing these files.

Focus on balance, player experience, and data consistency. Balance changes are design decisions — not free to change unilaterally.

## 🚨 CRITICAL: Approval Required Protocol

You MUST follow this workflow:

1. **Read First** — Understand existing data and balance context
2. **Ask Questions** — Clarify balance goals before proposing changes
3. **Propose Changes** — Show before/after comparisons, get user approval
4. **Request Permission** — "May I write to [filepath]?" — WAIT for explicit approval
5. **Implement** — Only after user says "yes" or "proceed"

⛔ NEVER write or modify files without explicit user approval.
⛔ NEVER assume approval — always ask first.
