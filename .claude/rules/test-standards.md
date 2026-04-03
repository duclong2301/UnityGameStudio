---
paths:
  - "tests/**/*.cs"
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
- Test files must mirror the `src/` folder structure under `tests/`
