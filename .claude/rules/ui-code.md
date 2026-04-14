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

### UGUI Animations (DOTween)

- Use **DOTween Pro** for all UGUI animations — never use Unity's built-in Animation system for UI
- Use DOTween Pro shortcuts for common animations:
  - `CanvasGroup.DOFade()` for fade in/out
  - `RectTransform.DOAnchorPos()` for position tweens
  - `RectTransform.DOScale()` for scale animations
  - `Image.DOColor()` / `Text.DOColor()` for color transitions
- Always call `.SetUpdate(true)` on UI tweens to make them independent of `Time.timeScale`
- Always call `.SetEase()` — prefer `Ease.OutCubic`, `Ease.InOutQuad`, or custom curves for polish
- Cache `Sequence` or `Tween` references if you need to control them later (pause, resume, kill)
- **Always kill tweens** in `OnDisable()` or `OnDestroy()` to prevent memory leaks:
  ```csharp
  private Tween _fadeTween;
  
  void OnDisable()
  {
      _fadeTween?.Kill();
  }
  ```
- Use `DOTween.Sequence()` to chain multiple animations instead of nested callbacks
- Use `.OnComplete()` for callbacks, never `while` loops polling `.IsPlaying()`
- Example pattern:
  ```csharp
  public class UIPanel : MonoBehaviour
  {
      [SerializeField] private CanvasGroup _canvasGroup;
      [SerializeField] private RectTransform _container;
      private Sequence _showSequence;
      
      public void Show()
      {
          _showSequence?.Kill();
          _showSequence = DOTween.Sequence()
              .Append(_canvasGroup.DOFade(1f, 0.3f))
              .Join(_container.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack))
              .SetUpdate(true);
      }
      
      void OnDestroy()
      {
          _showSequence?.Kill();
      }
  }
  ```
