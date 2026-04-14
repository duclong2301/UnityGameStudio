---
applyTo:
  - "Assets/Scripts/Core/GameFoundation/**/*.cs"
  - "Assets/Scripts/**/DataManager/**/*.cs"
  - "Assets/Scripts/**/GameState/**/*.cs"
  - "Assets/Scripts/**/UIManager/**/*.cs"
---

# GameFoundation Code Rules

> GameFoundation là framework cung cấp DataManager, GameStateManager, và UIManager.  
> Reference: `.claude/docs/frameworks/gamefoundation/README.md`

## Architecture Principles

- **Event-driven**: Tất cả systems giao tiếp qua events, KHÔNG reference trực tiếp
- **SOLID**: Tuân thủ Single Responsibility, Open/Closed, Liskov Substitution, Interface Segregation, Dependency Inversion
- **Separation of Concerns**: DataManager KHÔNG biết UI/GameState, UIManager KHÔNG gọi DataManager trực tiếp

## DataManager Rules

### Core Implementation

```csharp
// ✅ ĐÚNG: Collection tự định nghĩa SaveData class
[Serializable]
public class InventorySave : SaveData
{
    public List<ItemData> items;
}

public class PlayerInventoryCollection : ScriptableObject, ISavableCollection
{
    private List<ItemData> _items;
    
    public SaveData ToSaveData() => new InventorySave { items = _items };
    public void FromSaveData(SaveData data) {
        var save = data as InventorySave;
        _items = save.items;
    }
}

// ❌ SAI: Hardcode collection trong DataManager
public class DataManager {
    public PlayerInventory inventory; // ❌ Không extensible
}
```

### Best Practices

- ✅ Implement `ISavableCollection` cho mọi data collection
- ✅ Register collections qua Inspector list (assignable, không hardcode)
- ✅ Mỗi collection có concrete `SaveData` subclass riêng
- ✅ Sử dụng `BinaryFormatter` — native polymorphic serialization
- ❌ KHÔNG serialize Unity objects (`GameObject`, `Transform`, `Component`)
- ❌ KHÔNG lưu references giữa collections — mỗi collection độc lập
- ❌ KHÔNG dùng `[SerializeReference]` hoặc JSON wrapper strings

### Naming Conventions

| Element | Pattern | Example |
|---------|---------|---------|
| Collection class | `[Name]Collection : ScriptableObject, ISavableCollection` | `PlayerInventoryCollection` |
| Save data class | `[Name]Save : SaveData` | `InventorySave` |
| Collection key | lowercase + underscore | `"player_inventory"` |

## GameStateManager Rules

### State Transitions

```csharp
// ✅ ĐÚNG: Transition qua GameStateManager methods
GameStateManager.Init();
GameStateManager.Ready();
GameStateManager.Play();

// ✅ ĐÚNG: Lắng nghe state changes
void OnEnable() {
    GameStateManager.OnGameStateChanged += HandleStateChanged;
}

void OnDisable() {
    GameStateManager.OnGameStateChanged -= HandleStateChanged;
}

void HandleStateChanged(GameState current, GameState last, object data) {
    if (current == GameState.Play) {
        StartGameplay();
    }
}

// ❌ SAI: Set state trực tiếp
GameStateManager.CurrentState = GameState.Play; // ❌ Không có setter
```

### State Flow

Tuân thủ state flow chính xác:
```
AppStart → None → Main (Bootstrap loads MainScene)
                   │
         user clicks Play → Init (GameplayController.InitGame)
                               │
                             Ready (countdown)
                               │
                             Play ─────────────────────────┐
                            /    \                          │
                    WaitComplete  WaitGameOver   Restart ───┘
                         │              │
                      Complete       GameOver
                      /     \       /      \
                   Next     Main  Revice   Main
                    │              │
                  Init ──────────Play
```

**Quy tắc quan trọng:**
- `None → Main`: Bootstrap load xong MainScene → `GameStateManager.Main()`
- `Main → Init`: user click Play → `GameStateManager.Init()` → load GameplayScene
- `Init → Ready`: `GameplayController.InitGame()` hoàn tất → `GameStateManager.Ready()`
- `Ready → Play`: countdown xong → `GameStateManager.Play()`
- `GameplayController` PHẢI check `CurrentState == GameState.Init` trong `OnEnable` (scene load SAU khi Init event fire)

### Best Practices

- ✅ Transition states qua methods (`Play()`, `Ready()`, `Complete()`)
- ✅ Mọi component lắng nghe `OnGameStateChanged` event
- ✅ Truyền context data qua `object data` parameter khi cần
- ✅ Unsubscribe event trong `OnDisable()` / `OnDestroy()`
- ❌ KHÔNG skip states trong flow (phải đi theo thứ tự)
- ❌ KHÔNG dùng `CurrentState` để trigger logic — dùng event
- ❌ KHÔNG assume state order — luôn check `current` và `last`

### GameState Enum Reference

| State | Khi nào xảy ra |
|-------|----------------|
| `None` | App vừa khởi động (trước khi Bootstrap load xong) |
| `Main` | Bootstrap load xong MainScene, hoặc quay lại menu |
| `Init` | User click Play → GameplayController.InitGame() chạy |
| `Ready` | Sẵn sàng chơi (countdown) |
| `Play` | Gameplay active |
| `Restart` | Chơi lại level |
| `Complete` | Level thành công |
| `GameOver` | Level thất bại |
| `Next` | Chuyển level tiếp theo |
| `WaitComplete` | Đang xử lý kết quả win |
| `WaitGameOver` | Đang xử lý kết quả lose |
| `Other` | Loading transition |
| `Revice` | Hồi sinh sau game over |

## UIManager Rules

### Layer Hierarchy

5 layers với sort order cố định (thấp → cao):
```
Scene Layer (0)    ← Full-screen scenes, chỉ UIManager điều khiển
Popup Layer (100)  ← Modal popups, static Show/Hide
Dialog Layer (200) ← Blocking dialogs, queue-based
Toast Layer (300)  ← Non-blocking notifications
Loading Layer (400) ← Top-most loading screens
```

### Scene Layer Implementation

```csharp
// ✅ ĐÚNG: Scene kế thừa UISceneBase, protected Show/Hide
public class SceneMain : UISceneBase
{
    protected override void OnShow(object data) {
        // Init scene-specific logic
    }
    
    protected override void OnHide() {
        // Cleanup
    }
}

// UIManager điều khiển scene theo GameState, KHÔNG gọi trực tiếp
GameStateManager.Main(); // UIManager sẽ show SceneMain

// ❌ SAI: Gọi Show scene trực tiếp
SceneMain.Instance.Show(); // ❌ Method là protected
```

### Popup Layer Implementation

```csharp
// ✅ ĐÚNG: Popup có static Show, stack-able
public class PopupSettings : UIPopupBase
{
    public static void Show(Action<bool> onComplete = null) {
        UIManager.ShowPopup<PopupSettings>(onComplete);
    }
    
    protected override void OnShow(object data) {
        // Setup popup
    }
}

// Gọi từ bất kỳ đâu
PopupSettings.Show();

// ❌ SAI: Popup không có static Show
var popup = Resources.Load<PopupSettings>("PopupSettings");
popup.Show(); // ❌ Không quản lý stack
```

### Dialog Layer Implementation

```csharp
// ✅ ĐÚNG: Dialog modal, block input, queue-based
public class DialogConfirm : UIDialogBase
{
    public static void Show(string message, Action<bool> onResult) {
        var dialog = UIManager.ShowDialog<DialogConfirm>(onResult);
        dialog.SetMessage(message);
    }
}

// ❌ SAI: Dialog không block input
// → Dùng Popup thay vì Dialog nếu không cần block
```

### Toast Layer Implementation

```csharp
// ✅ ĐÚNG: Toast auto-hide, non-blocking
public class UIToast : UIToastBase
{
    public static void Show(string message, float duration = 2f) {
        var toast = UIManager.ShowToast<UIToast>();
        toast.SetMessage(message);
        toast.AutoHide(duration);
    }
}
```

### Animation Interface

```csharp
// ✅ ĐÚNG: Implement IUIAnim cho custom animations
public class UIAnimDO : MonoBehaviour, IUIAnim
{
    public UIAnimStatus Status { get; private set; }
    
    public void Show(Action onComplete) {
        // DOTween animation
        transform.DOScale(1f, 0.3f).OnComplete(() => {
            Status = UIAnimStatus.ShowCompleted;
            onComplete?.Invoke();
        });
    }
    
    public void Hide(Action onComplete) {
        transform.DOScale(0f, 0.2f).OnComplete(() => {
            Status = UIAnimStatus.HideCompleted;
            onComplete?.Invoke();
        });
    }
}

// Attach IUIAnim component vào UI GameObject
// UIBase tự động detect qua GetComponent<IUIAnim>()
```

### UIBase Override Pattern

```csharp
public class MyUI : UISceneBase // hoặc UIPopupBase, UIDialogBase, etc.
{
    // ✅ Override lifecycle methods
    protected override void OnInitialize() {
        // One-time setup (cache references)
    }
    
    protected override void OnShow(object data) {
        // Show logic, data từ caller
    }
    
    protected override void OnHide() {
        // Hide logic, cleanup state
    }
    
    protected override void OnDispose() {
        // Final cleanup (unsubscribe, release resources)
    }
}
```

### Best Practices

- ✅ Scene kế thừa `UISceneBase` — chỉ UIManager Show/Hide theo GameState
- ✅ Popup kế thừa `UIPopupBase` — static `Show()`, stack-able
- ✅ Dialog kế thừa `UIDialogBase` — modal, block input, queue
- ✅ Toast kế thừa `UIToastBase` — auto-hide, non-blocking
- ✅ Implement `IUIAnim` trên separate component (không merge vào UIBase)
- ✅ Override `OnShow(object data)` / `OnHide()` cho logic
- ❌ KHÔNG gọi `SceneMain.Show()` trực tiếp — dùng `GameStateManager.Main()`
- ❌ KHÔNG nest scenes trong scenes
- ❌ KHÔNG reference UIManager từ DataManager hoặc GameStateManager
- ❌ KHÔNG create UI instances manually — UIManager quản lý lifecycle

## Cross-System Communication

### DataManager ↔ GameStateManager

```csharp
// ✅ ĐÚNG: GameStateManager trigger save, KHÔNG biết DataManager
GameStateManager.OnGameStateChanged += (current, last, data) => {
    if (current == GameState.Complete) {
        DataManager.Instance.Save(); // Component riêng lắng nghe
    }
};

// ❌ SAI: GameStateManager gọi DataManager trực tiếp
public static void Complete() {
    // ...
    DataManager.Instance.Save(); // ❌ Tight coupling
}
```

### GameStateManager ↔ UIManager

```csharp
// ✅ ĐÚNG: UIManager lắng nghe GameState để show UI
void Awake() {
    GameStateManager.OnGameStateChanged += HandleStateChange;
}

void HandleStateChange(GameState current, GameState last, object data) {
    switch (current) {
        case GameState.Main:
            ShowScene<SceneMain>();
            break;
        case GameState.Play:
            ShowScene<ScenePlay>();
            break;
    }
}

// ❌ SAI: UIManager set GameState
public void OnPlayButtonClick() {
    GameStateManager.CurrentState = GameState.Play; // ❌ Không có setter
}
```

### UIManager ↔ DataManager

```csharp
// ✅ ĐÚNG: UI gọi DataManager qua public API
public class SceneMain : UISceneBase
{
    void OnSaveButtonClick() {
        DataManager.Instance.Save();
        UIToast.Show("Game saved!");
    }
}

// ❌ SAI: DataManager reference UI
public class DataManager {
    void Save() {
        // ...
        UIToast.Show("Saved!"); // ❌ DataManager không biết UI
    }
}
```

## Naming Conventions

### Files and Classes

| Type | Pattern | Example |
|------|---------|---------|
| Collection | `[Name]Collection.cs` | `PlayerInventoryCollection.cs` |
| Save data | `[Name]Save.cs` | `InventorySave.cs` |
| Scene UI | `Scene[Name].cs` | `SceneMain.cs`, `ScenePlay.cs` |
| Popup UI | `Popup[Name].cs` | `PopupSettings.cs`, `PopupShop.cs` |
| Dialog UI | `Dialog[Name].cs` | `DialogConfirm.cs` |
| Toast UI | `UIToast[Type].cs` | `UIToast.cs`, `UIToastFloat.cs` |

### Events and Delegates

```csharp
// ✅ Pattern: On[Event]Delegate + On[Event]
public delegate void OnGameStateChangedDelegate(GameState current, GameState last, object data);
public static event OnGameStateChangedDelegate OnGameStateChanged;

// ✅ Pattern: On[Action] methods
protected virtual void OnShow(object data) { }
protected virtual void OnHide() { }
```

## Performance Considerations

### DataManager
- ✅ Save async khi có thể: `await DataManager.Instance.SaveAsync()`
- ✅ Cache save path, không tính toán lại mỗi lần save
- ❌ KHÔNG save mỗi frame — debounce hoặc save theo event

### UIManager
- ✅ Pool UI instances nếu Show/Hide thường xuyên
- ✅ Disable Canvas khi Hide thay vì Destroy
- ✅ Use `CanvasGroup.alpha = 0` + `blocksRaycasts = false` khi ẩn
- ❌ KHÔNG tạo mới UI mỗi lần Show — reuse instances

### GameStateManager
- ✅ Event subscribers nhẹ — không heavy logic trong callback
- ✅ Unsubscribe ngay khi không cần (trong `OnDisable`)
- ❌ KHÔNG subscribe nhiều lần cùng một handler

## Error Handling

```csharp
// ✅ ĐÚNG: Validate state transitions
public static void Play() {
    if (CurrentState != GameState.Ready) {
        Debug.LogWarning($"Cannot transition to Play from {CurrentState}");
        return;
    }
    SetState(GameState.Play);
}

// ✅ ĐÚNG: Validate data types trong event handlers
void HandleStateChange(GameState current, GameState last, object data) {
    if (current == GameState.Play && data is GameplayController.State state) {
        // Safe to use state
    }
}

// ✅ ĐÚNG: Try-catch save/load operations
public void Save() {
    try {
        BinaryFormatter bf = new BinaryFormatter();
        // ...
    }
    catch (Exception e) {
        Debug.LogError($"Save failed: {e.Message}");
    }
}
```

## Testing Patterns

```csharp
// ✅ Test state transitions
[Test]
public void TestStateTransition() {
    GameStateManager.Init();
    Assert.AreEqual(GameState.Init, GameStateManager.CurrentState);
    
    GameStateManager.Ready();
    Assert.AreEqual(GameState.Ready, GameStateManager.CurrentState);
}

// ✅ Test save/load round-trip
[Test]
public void TestSaveLoad() {
    var collection = CreateTestCollection();
    var saveData = collection.ToSaveData();
    collection.Clear();
    collection.FromSaveData(saveData);
    Assert.AreEqual(originalData, collection.Data);
}

// ✅ Test UI layer order
[Test]
public void TestUILayerOrder() {
    var scene = UIManager.ShowScene<SceneMain>();
    var popup = UIManager.ShowPopup<PopupSettings>();
    Assert.Greater(popup.Canvas.sortingOrder, scene.Canvas.sortingOrder);
}
```

## Role Context

When working with GameFoundation code, you are acting as:
- **Engine Programmer** for DataManager / GameStateManager core
- **UI Programmer** for UIManager / UI implementations
- **Gameplay Programmer** for game-specific collections

Always reference `.claude/docs/frameworks/gamefoundation/` for detailed architecture before implementing.

## Documentation References

- [README](.claude/docs/frameworks/gamefoundation/README.md) — Overview + integration guide
- [DataManager Architecture](.claude/docs/frameworks/gamefoundation/datamanager-architecture.md) — Detailed save/load system
- [GameState Architecture](.claude/docs/frameworks/gamefoundation/gamestate-architecture.md) — 2-layer state machine
- [GameState Simplified](.claude/docs/frameworks/gamefoundation/gamestate-simplified.md) — GameStateManager only
- [UIManager Architecture](.claude/docs/frameworks/gamefoundation/uimanager-architecture.md) — UI layer system

---

**🚨 CRITICAL:** Before implementing GameFoundation features, ALWAYS:
1. Read the corresponding architecture doc
2. Ask clarifying questions about which variant to use (2-layer GameState vs simplified)
3. Propose implementation structure
4. Get user approval before writing code
