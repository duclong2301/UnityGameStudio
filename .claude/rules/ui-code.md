---
paths:
  - "src/UI/**/*.cs"
  - "src/UI/**/*.uxml"
  - "src/UI/**/*.uss"
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
