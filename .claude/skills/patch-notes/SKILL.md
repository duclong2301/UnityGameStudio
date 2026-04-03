---
name: patch-notes
description: "Creates player-facing patch notes from internal bug fixes and changes. Translates technical jargon into player-friendly language."
argument-hint: "[version number]"
user-invocable: true
allowed-tools: Read, Glob, Grep, Write, Bash
---

When this skill is invoked:

1. **Read** recent resolved bugs from `production/bugs/BUG-*.md` (Status: Fixed).
2. **Read** the changelog for this version (or git log).
3. **Translate** technical fixes to player-facing language:
   - "Fixed NullReferenceException in PlayerHealthSystem" → "Fixed a crash that could occur when taking damage"
   - "Reduced GC allocations in Update loop" → "Improved frame rate consistency during combat"

4. **Generate patch notes**:

```markdown
# Patch Notes — v[Version] — [Date]

## What's New
[New features in player terms]

## Bug Fixes
- Fixed: [Player-facing description]

## Balance Changes
- [What changed]: [Old value → New value, with brief reasoning]

## Improvements
- [Quality of life improvements]

## Known Issues
- [Any unresolved issues players may encounter + workaround if available]

---
Thank you for playing and for your bug reports!
```

5. **Ask for approval**, then write to `production/patch-notes/v[version].md`.
