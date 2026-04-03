---
paths:
  - "src/**/*.cs"
---

# Unity C# Gameplay Code Rules

- ALL gameplay values (damage, speed, range, cooldowns) MUST come from ScriptableObjects or config files — NEVER hardcoded
- Use `Time.deltaTime` (or `Time.fixedDeltaTime` in FixedUpdate) for ALL time-dependent calculations
- NO direct references to UI from gameplay code — use C# events or UnityEvent for cross-system communication
- Every gameplay system must expose a clear interface (`IDamageable`, `IInteractable`, etc.)
- State machines must have explicit transition tables with documented states
- Write Play Mode or Edit Mode tests for all gameplay logic — separate logic from presentation
- Document which design doc section each feature implements via XML doc comments
- No static singletons for game state — use dependency injection or ScriptableObject events

## Design Principles (SOLID, KISS, DRY)

- **SRP**: each class has one responsibility — no monolithic MonoBehaviours handling movement, combat, and UI
- **OCP**: extend systems via new interface implementations, not by modifying existing classes
- **LSP**: any class implementing an interface (e.g., `IDamageable`) must be fully substitutable
- **ISP**: keep interfaces small and focused — `IDamageable` and `IHealable` are separate, not one `IHealthManageable`
- **DIP**: gameplay systems depend on abstractions (interfaces), not concrete implementations
- **KISS**: prefer the simplest working solution — do not add patterns or layers until they are justified
- **DRY**: extract repeated logic into shared utility methods or ScriptableObject-driven configurations

## Unity-Specific Rules

- Cache `GetComponent<T>()` calls in `Awake()` — never call in `Update()`
- Use `[SerializeField] private` instead of `public` for inspector-exposed fields
- Use `ObjectPool<T>` for frequently instantiated objects (projectiles, enemies, VFX)
- Avoid LINQ in hot paths (`Update`, physics callbacks) — allocates garbage
- Use `NonAlloc` physics APIs: `Physics.RaycastNonAlloc`, `Physics.OverlapSphereNonAlloc`

## Examples

**Correct** (data-driven, Unity best practices):

```csharp
[SerializeField] private CombatConfig _config;
private Rigidbody _rb;

private void Awake() => _rb = GetComponent<Rigidbody>();

private void Update()
{
    float speed = _config.MovementSpeed * Time.deltaTime;
    _rb.MovePosition(transform.position + direction * speed);
}
```

**Incorrect** (hardcoded, bad patterns):

```csharp
void Update()
{
    float speed = 5.0f;  // VIOLATION: hardcoded value
    GetComponent<Rigidbody>().MovePosition(transform.position + direction * speed); // VIOLATION: GetComponent in Update
}
```
