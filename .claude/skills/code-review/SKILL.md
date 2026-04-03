---
name: code-review
description: "Performs an architectural and quality code review on a specified Unity C# file or set of files. Checks for coding standard compliance, Unity best practices, SOLID principles, testability, and performance concerns."
argument-hint: "[path-to-file-or-directory]"
user-invocable: true
allowed-tools: Read, Glob, Grep, Bash
---

When this skill is invoked:

1. **Read the target file(s)** in full.

2. **Read `.claude/docs/coding-standards.md`** for project coding standards.

3. **Identify the system category** (engine, gameplay, AI, networking, UI, tools)
   and apply category-specific rules from `.claude/rules/`.

4. **Evaluate against coding standards**:
   - [ ] `[SerializeField] private` used instead of `public` for inspector fields
   - [ ] No `GetComponent<T>()` in `Update()` — all cached in `Awake()`
   - [ ] No `Find()`, `FindObjectOfType()`, `SendMessage()` in production code
   - [ ] No hardcoded gameplay values — all from ScriptableObjects or config
   - [ ] Events subscribed/unsubscribed symmetrically (`OnEnable`/`OnDisable`)
   - [ ] Dependencies injected, not found via scene search
   - [ ] Assembly definition exists for this code folder

5. **Check Unity best practices**:
   - [ ] No LINQ allocations in hot paths (Update, physics callbacks)
   - [ ] Object pooling used for frequently instantiated objects
   - [ ] `NonAlloc` physics APIs used where applicable
   - [ ] No allocations in `Update()` (check for string concatenation, `new` in loops)
   - [ ] Coroutines: `StopCoroutine` called where needed

6. **Check architectural compliance**:
   - [ ] Correct dependency direction (engine ← gameplay, not reverse)
   - [ ] No circular dependencies between assemblies
   - [ ] UI does not own game state
   - [ ] Events/delegates used for cross-system communication
   - [ ] Interfaces expose system contracts

7. **Check SOLID compliance**:
   - [ ] Single Responsibility (SRP): each class has one reason to change
   - [ ] Open/Closed (OCP): extendable without modification (via interfaces/inheritance)
   - [ ] Liskov Substitution (LSP): subtypes must be substitutable for their base types without altering correctness
   - [ ] Interface Segregation (ISP): prefer small, focused interfaces over large monolithic ones
   - [ ] Dependency Inversion (DIP): depends on abstractions, not concretions

8. **Check KISS and DRY compliance**:
   - [ ] KISS: no over-engineered abstractions — simplest solution that meets requirements
   - [ ] KISS: no unnecessary complexity — favor readability over cleverness
   - [ ] DRY: no duplicated logic across 3+ locations — extract shared abstractions
   - [ ] DRY: configuration data reused via ScriptableObjects, not copy-pasted

9. **Output the review** in this format:

```
## Code Review: [File/System Name]

### Unity Standards: [X/7 passing]
[List any failures with line references]

### Architecture: [CLEAN / MINOR ISSUES / VIOLATIONS FOUND]
[List specific architectural concerns]

### SOLID: [COMPLIANT / ISSUES FOUND]
[List specific violations — check all 5 principles: SRP, OCP, LSP, ISP, DIP]

### KISS & DRY: [COMPLIANT / ISSUES FOUND]
[List over-engineering, unnecessary complexity, or duplicated logic]

### Performance Concerns
[List allocations, GC issues, hot path problems]

### Positive Observations
[What is done well — always include this section]

### Required Changes
[Must-fix items before approval]

### Suggestions
[Nice-to-have improvements]

### Verdict: [APPROVED / APPROVED WITH SUGGESTIONS / CHANGES REQUIRED]
```
