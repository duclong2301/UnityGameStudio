---
name: prototype
description: "Sets up a quick prototype in Unity to test a specific mechanic or concept. Creates the prototype folder structure, a findings document template, and guides rapid implementation."
argument-hint: "[mechanic or concept to prototype]"
user-invocable: true
allowed-tools: Read, Glob, Grep, Write, Bash
---

When this skill is invoked:

1. **Define the prototype question**:
   - "What specific thing are we testing with this prototype?"
   - "What would 'success' look like?"
   - "What would 'failure' look like?"

2. **Timebox**: "How long should we spend on this prototype? (Recommended: 1-2 days max)"

3. **Create the prototype folder structure**:
   ```
   prototypes/[FeatureName]-[YYYYMMDD]/
     README.md        (question being answered)
     FINDINGS.md      (template, filled after testing)
     Assets/          (Unity assets — scripts, scenes)
   ```

4. **Create README.md**:
```markdown
# Prototype: [Feature Name]
**Date**: [Today]
**Question**: [Specific thing we're testing]
**Success Criteria**: [What would make this worth implementing properly?]
**Failure Criteria**: [What would tell us this mechanic doesn't work?]
**Timebox**: [X hours/days]

## Rules
- All scripts must have `// PROTOTYPE` comment at top
- NO art — use Unity primitives only
- NO production code dependencies
- Hardcoded values are fine
```

5. **Delegate to `prototyper`** for implementation guidance.

6. **After prototyping**, prompt to fill `FINDINGS.md` and run `/design-review` if findings are positive.
