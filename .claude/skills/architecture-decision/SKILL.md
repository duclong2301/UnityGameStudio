---
name: architecture-decision
description: "Creates an Architecture Decision Record (ADR) for a significant technical decision. Documents the context, options considered, decision made, and consequences."
argument-hint: "[decision topic]"
user-invocable: true
allowed-tools: Read, Glob, Grep, Write
---

When this skill is invoked:

1. **Find the next ADR number** by counting files in `docs/architecture/`.

2. **Ask for decision details** if not in the argument:
   - "What is the decision to be made?"
   - "What are the options?"
   - "What constraints drive the decision?"

3. **Create `docs/architecture/adr-[NNN]-[topic].md`**:

```markdown
# ADR-[NNN]: [Decision Title]
**Date**: [Today]
**Status**: [Proposed / Accepted / Deprecated / Superseded by ADR-NNN]
**Deciders**: [Agents/People who made this decision]

## Context
[Why is this decision needed? What problem does it solve?]

## Decision Drivers
- [Constraint or goal that influenced the decision]
- [Constraint or goal]

## Options Considered

### Option A: [Name]
**Pros**: 
- [Pro]
**Cons**: 
- [Con]

### Option B: [Name]
**Pros**: 
- [Pro]
**Cons**: 
- [Con]

## Decision
**We chose: [Option X]**

[Reasoning: why this option, given the drivers]

## Consequences
**Positive**: [What this enables]
**Negative**: [Trade-offs accepted]
**Risks**: [What could go wrong]

## Validation
[How we'll know if this decision was right: success criteria]
```

4. **Ask for approval** before writing.
