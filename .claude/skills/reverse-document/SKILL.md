---
name: reverse-document
description: "Generates design and technical documentation from existing code and assets. Useful when code was written without prior documentation."
argument-hint: "[path to system or feature to document]"
user-invocable: true
allowed-tools: Read, Glob, Grep, Write
---

When this skill is invoked:

1. **Read all code** in the target path.
2. **Infer the design intent** from the code:
   - What does this system do?
   - What are its inputs and outputs?
   - What patterns are used?
   - What dependencies does it have?

3. **Generate documentation**:

**Design doc** (if design/gdd/ entry is missing):
```markdown
# [System Name] — Reverse-Documented
**Status**: Inferred from code (not original design)
**Date**: [Today]

## What It Does
[Inferred from code analysis]

## How It Works
[Step-by-step as implemented]

## Key Classes
| Class | Purpose |
|---|---|

## Data
[ScriptableObjects, config files used]

## Known Gaps / Questions
[Things that couldn't be inferred — needs clarification]
```

**Technical notes** (if docs/architecture/ entry is missing):
```markdown
# [System Name] — Architecture Notes
[Dependencies, patterns, assembly, known tech debt]
```

4. **Ask for approval** before writing.
5. **Warn**: "This is inferred documentation — please review for accuracy."
