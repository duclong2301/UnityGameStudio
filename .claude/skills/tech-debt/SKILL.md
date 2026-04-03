---
name: tech-debt
description: "Audits the codebase for technical debt: code smells, architectural violations, missing tests, and outdated patterns. Produces a prioritized paydown plan."
argument-hint: "[path to audit, or 'all']"
user-invocable: true
allowed-tools: Read, Glob, Grep, Bash
---

When this skill is invoked:

1. **Scan** the target path for technical debt indicators:
   - `TODO` and `FIXME` comments
   - `FindObjectOfType`, `Find(`, `SendMessage` calls
   - `GetComponent` calls outside `Awake()`
   - `public` fields (should be `[SerializeField] private`)
   - Direct `Resources.Load` calls
   - Missing assembly definitions (`.asmdef`)
   - Files with > 300 lines of code
   - Classes with > 10 public methods

2. **Categorize by severity**:
   - **Critical**: Will cause bugs or performance issues
   - **High**: Architecture violations that will cause pain later
   - **Medium**: Code smells that reduce maintainability
   - **Low**: Style issues

3. **Output**:

```
## Technical Debt Audit

### Summary
- Total debt items: X
- Critical: X | High: X | Medium: X | Low: X

### Critical Issues
[File:Line — Description — Impact]

### High Priority
[File:Line — Description — Impact]

### Paydown Plan (Suggested Sprint Backlog)
| Task | Effort | Impact | Assign to |
|---|---|---|---|

### Estimated Paydown Time: [X sprint(s)]
```
