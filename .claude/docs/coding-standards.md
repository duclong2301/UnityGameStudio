# Coding Standards — Unity C#

## Naming Conventions

| Element | Convention | Example |
|---|---|---|
| Class | PascalCase | `PlayerController` |
| Interface | `I` + PascalCase | `IDamageable` |
| Enum | PascalCase | `GameState` |
| Public property | PascalCase | `Health` |
| Private field | `_camelCase` | `_currentHealth` |
| Local variable | camelCase | `damageAmount` |
| Constant | PascalCase | `MaxHealth` |
| Method | PascalCase | `TakeDamage()` |
| Parameter | camelCase | `damageAmount` |
| Event | PascalCase | `OnDeath` |
| ScriptableObject | PascalCase + `SO` suffix optional | `CombatConfigSO` |

## File Organization

- One class per file (exceptions: small private helper classes)
- File name must match class name
- Use `partial` classes only for auto-generated code (UI Toolkit, source generators)
- Assembly definitions required for every `src/` subdirectory

## C# Standards

```csharp
// Fields: [SerializeField] private, with Header and Tooltip
[Header("Combat Settings")]
[SerializeField, Tooltip("Base damage multiplier from config")]
private float _baseDamage;

// Properties: public getter, private/protected setter
public float Health { get; private set; }

// Events: use Action or UnityEvent
public event Action<float> OnHealthChanged;

// Methods: XML doc for public methods
/// <summary>
/// Applies damage to this character. Triggers OnDeath if health reaches zero.
/// </summary>
/// <param name="amount">Damage amount before armor reduction.</param>
public void TakeDamage(float amount) { ... }
```

## Unity-Specific Standards

- `Awake()`: initialize components, cache references
- `Start()`: post-Awake inter-component setup
- `OnEnable()` / `OnDisable()`: subscribe / unsubscribe events
- `OnDestroy()`: cleanup, release handles

## Code Review Checklist

- [ ] No `GetComponent<T>()` in `Update()`
- [ ] No hardcoded gameplay values
- [ ] Dependencies injected, not found via `Find()`
- [ ] Events subscribed/unsubscribed symmetrically
- [ ] `[SerializeField] private` instead of `public` for inspector fields
- [ ] Assembly definition exists for this folder
- [ ] No LINQ in hot paths

## Design Principles

### SOLID
- **Single Responsibility (SRP)**: each class has one reason to change — no monolithic MonoBehaviours
- **Open/Closed (OCP)**: systems are extendable via interfaces/inheritance without modifying existing code
- **Liskov Substitution (LSP)**: subtypes must be substitutable for their base types without breaking correctness
- **Interface Segregation (ISP)**: prefer small, focused interfaces (`IDamageable`, `IInteractable`) over large ones
- **Dependency Inversion (DIP)**: depend on abstractions (interfaces), not concretions — use DI or ScriptableObject events

### KISS (Keep It Simple, Stupid)
- Prefer the simplest solution that meets the requirements
- Avoid over-engineering — do not add abstractions until they are needed
- Favor readability over cleverness — code is read far more often than it is written
- If a system can be expressed with a ScriptableObject and an event, do not build a framework

### DRY (Don't Repeat Yourself)
- Extract shared logic into utility methods or base classes
- Reuse ScriptableObjects for common configuration data across systems
- Use USS variables and shared Sub Graphs instead of duplicating shader/style values
- If the same logic appears in 3+ places, refactor into a shared abstraction
