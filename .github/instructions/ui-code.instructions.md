---
applyTo:
  - "Assets/Scripts/UI/**/*.cs"
  - "Assets/UI/**/*.uxml"
  - "Assets/UI/**/*.uss"
  - "Assets/Scripts/UI/**/*.uxml"
  - "Assets/Scripts/UI/**/*.uss"
---

# Unity UI Code Rules

- Prefer **UI Toolkit** (UXML/USS) for all new screen-space UI
- Use **UGUI** only for world-space UI (NPC health bars, floating text) or features UI Toolkit doesn't support
- UI code must NEVER own or modify game state — UI reads data through bindings or events
- User actions dispatch events/commands that game systems process — UI does not call game logic directly
- Register `RegisterCallback<T>` in `OnEnable()` or `OnAttachToPanel`, unregister in `OnDisable()` or `OnDetachFromPanel`
- Use a screen stack manager for menu navigation (`Push/Pop/Replace/ClearTo`)

## UI Toolkit (UXML/USS) Specific

- One UXML file per screen or reusable component
- USS variables for all theme values (colors, fonts, spacing) — no hard-coded values in USS
- Support High Contrast and accessibility themes via USS class swapping
- Use runtime data bindings (`INotifyBindablePropertyChanged` ViewModels) instead of polling game state in Update

## UGUI (Canvas) Specific

- Separate Canvases for HUD (frequently changing) and static UI (rarely changing)
- Disable `Raycast Target` on all non-interactive Image/Text elements
- Never nest Layout Groups deeper than 2 levels — use anchors instead
- Cache `RectTransform` — never call `GetComponent<RectTransform>()` in Update

## ViewModel Pattern

```csharp
public class HealthViewModel : INotifyBindablePropertyChanged
{
    private float _health;
    public float Health
    {
        get => _health;
        set
        {
            _health = value;
            Notify(nameof(Health));
        }
    }

    public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;

    private void Notify(string propertyName) =>
        propertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(propertyName));
}
```

## Role Context

You are acting as a **UI Programmer / UX Designer** when working with these files.

UI code must never own game state. UI reads through bindings; user actions dispatch events. Register and unregister all callbacks symmetrically.

## 🚨 CRITICAL: Approval Required Protocol

You MUST follow this workflow:

1. **Read First** — Review UX specs and existing screen architecture
2. **Ask Questions** — Clarify UI flow and interaction requirements
3. **Propose Implementation** — Show ViewModel and screen structure, get user approval
4. **Request Permission** — "May I write to [filepath]?" — WAIT for explicit approval
5. **Implement** — Only after user says "yes" or "proceed"

⛔ NEVER write or modify files without explicit user approval.
⛔ NEVER assume approval — always ask first.
