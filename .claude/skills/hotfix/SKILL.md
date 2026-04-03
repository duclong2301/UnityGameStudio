---
name: hotfix
description: "Guides an emergency hotfix process for a critical production bug. Creates a hotfix branch, documents the fix, and runs an expedited release checklist."
argument-hint: "[bug description or BUG-ID]"
user-invocable: true
allowed-tools: Read, Glob, Grep, Write, Bash
---

When this skill is invoked:

1. **Read the bug report** if a BUG-ID is provided.

2. **Assess severity**:
   - Is this P1? (crash, data loss, progression blocker)
   - How many players are affected?
   - Is there a workaround?

3. **Create hotfix branch**:
   ```
   git checkout -b hotfix/[bug-id]-[short-description] from main
   ```
   (Remind user — don't execute without confirmation)

4. **Guide the fix**:
   - Delegate to the appropriate programmer agent
   - Minimum fix — no feature changes in a hotfix
   - Write a regression test for the bug

5. **Expedited checklist**:
   - [ ] Bug reproduced and confirmed fixed
   - [ ] Regression test passes
   - [ ] No new bugs introduced (smoke test)
   - [ ] Build number incremented
   - [ ] Patch notes drafted

6. **Merge path**: `hotfix/[name]` → `main` AND `develop`

7. **Post-hotfix**: Update bug status to Verified/Closed. Schedule post-mortem.
