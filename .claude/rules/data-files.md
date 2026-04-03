---
paths:
  - "assets/data/**"
  - "design/**/*.json"
  - "design/**/*.csv"
---

# Data Files Rules

- Game balance data (enemy stats, item properties, level configs) MUST live in ScriptableObjects or JSON/CSV in `assets/data/`
- Never hard-code game balance values in C# code
- All data files must have a schema comment or companion `_schema.md` file explaining each field
- CSV files: first row is always the header; all values trimmed of whitespace
- JSON files: use camelCase keys; arrays use consistent element types
- ScriptableObjects: group by domain in `assets/data/[Domain]/` folder
- Data changes require design review — balance values are design decisions, not implementation details
- Version-stamp data files when they affect saved game compatibility
