---
name: changelog
description: "Generates a changelog entry from recent git commits. Groups changes by category (features, fixes, improvements, breaking changes)."
argument-hint: "[version number or git range]"
user-invocable: true
allowed-tools: Read, Glob, Grep, Bash
---

When this skill is invoked:

1. **Get recent commits** via `git log --oneline` since last tag or provided range.
2. **Categorize commits** by conventional commit type:
   - `feat:` → New Features
   - `fix:` → Bug Fixes
   - `perf:`, `refactor:` → Improvements
   - `break:` → Breaking Changes
   - `docs:`, `chore:` → Internal (omit from player-facing changelog)
3. **Generate changelog entry**:

```markdown
## [Version] — [Date]

### New Features
- [Feature description for players]

### Bug Fixes
- Fixed: [Bug description]

### Improvements
- [Performance/UX improvement]

### Breaking Changes (developer/modder facing)
- [API changes if applicable]
```

4. **Ask for approval**, then append to `CHANGELOG.md` (create if not exists).
