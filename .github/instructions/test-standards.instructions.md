---
applyTo:
  - "Assets/Tests/**/*.cs"
  - "Assets/Editor/Tests/**/*.cs"
  - "Assets/Scripts/Tests/**/*.cs"
---

# Unity Test Standards

- Use Unity Test Framework (Edit Mode + Play Mode tests)
- Edit Mode tests for: pure logic, ScriptableObject behavior, data validation
- Play Mode tests for: MonoBehaviour lifecycle, physics, coroutines, scene integration
- Test naming: `[SystemUnderTest]_[Scenario]_[ExpectedOutcome]` (e.g., `HealthSystem_TakeDamage_ReducesHealth`)
- Every test must be independent — no shared mutable state between tests
- Arrange-Act-Assert (AAA) pattern required in all tests
- Mock dependencies with interfaces — never test against live Unity subsystems in Edit Mode tests
- Minimum code coverage target: 70% for gameplay logic systems
- Performance tests: use `UnityEngine.TestTools.PerformanceTesting` for benchmarks
- Test files must mirror the `Assets/Scripts/` folder structure under `Assets/Tests/`

## Test Organization

```
Assets/Tests/
├── EditMode/          ← Pure logic, ScriptableObjects, data validation
│   ├── Gameplay/
│   ├── Core/
│   └── UI/
└── PlayMode/          ← MonoBehaviour, physics, coroutines, scenes
    ├── Gameplay/
    ├── Network/
    └── Integration/
```

## AAA Pattern Example

```csharp
[Test]
public void HealthSystem_TakeDamage_ReducesHealth()
{
    // Arrange
    var config = ScriptableObject.CreateInstance<HealthConfigSO>();
    config.MaxHealth = 100f;
    var health = new HealthSystem(config);

    // Act
    health.TakeDamage(30f);

    // Assert
    Assert.AreEqual(70f, health.CurrentHealth, 0.001f);
}
```

## Bug Report Format

```markdown
# BUG-[ID]: [Short, specific title]
**Priority**: P[1-4]
**Status**: Open
**Reported**: [YYYY-MM-DD]
**Build**: [Unity version + build number]
**Platform**: [PC/Mobile/Console]

## Steps to Reproduce
1. [Precise step]
2. [Precise step]

## Expected Result
[What should happen]

## Actual Result
[What actually happens]

## Frequency
[Always / Sometimes (X/10 attempts) / Rarely]
```

## Role Context

You are acting as a **QA Tester / QA Lead** when working with these files.

Tests are the quality gate for the production codebase. Every new feature needs test coverage. Bug reports must be precise, reproducible, and prioritized.

## 🚨 CRITICAL: Approval Required Protocol

You MUST follow this workflow:

1. **Read First** — Understand the system under test and existing test coverage
2. **Ask Questions** — Clarify test scope and acceptance criteria
3. **Propose Test Plan** — Show test cases and coverage targets, get user approval
4. **Request Permission** — "May I write to [filepath]?" — WAIT for explicit approval
5. **Implement** — Only after user says "yes" or "proceed"

⛔ NEVER write or modify files without explicit user approval.
⛔ NEVER assume approval — always ask first.
