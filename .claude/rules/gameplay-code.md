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

## Animation & Tweening (DOTween)

- Use **DOTween Pro** as the default for all gameplay animations and tweening — unless user explicitly requests an alternative
- DOTween is for **declarative animations** (move to position, fade, scale) — NOT for continuous per-frame logic
- **Use DOTween for:**
  - Object movement along paths or to specific positions (enemy patrol, item pickups)
  - Camera shakes, zoom effects, field-of-view transitions
  - VFX scaling, fading, or color changes
  - Health bar fills, damage number animations
  - Platform movement (if not physics-based)
  - Object spawning/despawning with scale/fade effects
- **DO NOT use DOTween for:**
  - Physics-based movement (use `Rigidbody.MovePosition` or forces instead)
  - Player input-driven movement (use direct transform or Rigidbody manipulation)
  - Continuous per-frame calculations in `Update()` (use standard Unity update loop)
  - Anything requiring precise collision detection (use FixedUpdate + physics)

### DOTween Best Practices

- Always call `.SetEase()` — prefer `Ease.InOutQuad`, `Ease.OutCubic`, or custom curves
- Use `.SetUpdate(UpdateType.Normal)` for gameplay effects that should pause with game time
- Use `.SetUpdate(true)` only for UI or effects that must continue during pause
- **Always kill tweens** when object is destroyed or disabled:
  ```csharp
  private Tween _moveTween;
  
  void OnDisable()
  {
      _moveTween?.Kill();
  }
  ```
- Cache tween references if you need to control them (pause, speedup, rewind)
- Use `DOTween.Sequence()` for complex multi-step animations
- Use `.OnComplete()` callbacks instead of coroutines for post-animation logic
- Pull animation parameters (duration, ease, curve) from ScriptableObject configs, not hardcoded

### DOTween Examples

**Correct** (declarative, data-driven):

```csharp
[SerializeField] private MovementConfig _config;
private Tween _moveTween;

public void MoveToTarget(Vector3 target)
{
    _moveTween?.Kill();
    _moveTween = transform.DOMove(target, _config.MoveDuration)
        .SetEase(_config.MoveEase)
        .OnComplete(OnReachedTarget);
}

void OnDisable() => _moveTween?.Kill();
```

**Enemy patrol example:**

```csharp
[SerializeField] private Transform[] _patrolPoints;
[SerializeField] private float _patrolSpeed = 2f;

private Sequence _patrolSequence;

void Start()
{
    _patrolSequence = DOTween.Sequence();
    foreach (var point in _patrolPoints)
    {
        float duration = Vector3.Distance(transform.position, point.position) / _patrolSpeed;
        _patrolSequence.Append(transform.DOMove(point.position, duration).SetEase(Ease.Linear));
    }
    _patrolSequence.SetLoops(-1, LoopType.Yoyo);
}

void OnDestroy() => _patrolSequence?.Kill();
```

**Incorrect** (misuse of DOTween):

```csharp
// WRONG: Using DOTween for continuous player movement
void Update()
{
    Vector3 movement = Input.GetAxis("Horizontal") * Vector3.right;
    transform.DOMove(transform.position + movement, 0.016f); // DON'T DO THIS - use direct transform or Rigidbody
}

// WRONG: Using DOTween for physics-based projectile
void FireProjectile()
{
    transform.DOMove(target, 1f); // DON'T DO THIS - use Rigidbody.AddForce or velocity
}
```

## Examples

**Correct** (data-driven, Unity best practices, DOTween for animations):

```csharp
[SerializeField] private CombatConfig _config;
private Rigidbody _rb;
private Tween _hitRecoilTween;

private void Awake() => _rb = GetComponent<Rigidbody>();

private void Update()
{
    // Continuous physics-based movement (NOT using DOTween)
    float speed = _config.MovementSpeed * Time.deltaTime;
    _rb.MovePosition(transform.position + direction * speed);
}

public void PlayHitRecoil()
{
    // Declarative animation (USING DOTween)
    _hitRecoilTween?.Kill();
    _hitRecoilTween = transform.DOPunchScale(Vector3.one * 0.2f, 0.3f)
        .SetEase(_config.HitRecoilEase);
}

void OnDisable() => _hitRecoilTween?.Kill();
```

**Incorrect** (hardcoded, bad patterns):

```csharp
void Update()
{
    float speed = 5.0f;  // VIOLATION: hardcoded value
    GetComponent<Rigidbody>().MovePosition(transform.position + direction * speed); // VIOLATION: GetComponent in Update
    
    // VIOLATION: Using DOTween for continuous per-frame movement
    transform.DOMove(transform.position + direction * speed, Time.deltaTime);
}
```
