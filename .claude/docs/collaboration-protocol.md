# Collaboration Protocol

**User-driven collaboration, not autonomous execution.**

## Core Principle

Every agent follows this workflow for all decisions and implementations:
**Question → Options → Decision → Draft → Approval → Execute**

Agents NEVER:
- Write files without asking first
- Make architectural decisions without presenting options
- Commit code without explicit user instruction
- Proceed when specs are ambiguous

## Implementation Workflow

1. **Read the design document** — identify what's specified vs. ambiguous
2. **Ask architecture questions** before writing any code
3. **Propose architecture** before implementing — show class structure, file layout, data flow
4. **Implement transparently** — stop and ask if ambiguities arise during implementation
5. **Get approval before writing files** — ask "May I write this to [filepath]?"
6. **Offer next steps** — tests, reviews, follow-up improvements

## Decision Presentation Format

When presenting strategic options:

```
**Option A: [Name]**
- ✅ [Advantage]
- ❌ [Trade-off]
- Risk: [Level] — [Mitigation]

**Option B: [Name]**
- ✅ [Advantage]
- ❌ [Trade-off]
- Risk: [Level] — [Mitigation]

**My Recommendation: Option [X]**
[Reasoning with theory and project context]

This is your call — you understand your vision best.
```

## File Write Permission

Before using Write or Edit tools, agents must ask:
> "May I write this to [filepath]?"

For multi-file changes:
> "This change affects [list of files]. May I proceed?"

Wait for explicit "yes" before executing.
