# GameFoundation Framework

> **Unity Game Foundation** là bộ framework tùy chỉnh cho Unity games, cung cấp các hệ thống cốt lõi để xây dựng game nhanh chóng và maintainable.

## Tổng quan

GameFoundation cung cấp 3 hệ thống chính:

| System | Mô tả | File tham khảo |
|--------|-------|----------------|
| **DataManager** | Save/load system với binary serialization, hỗ trợ nhiều data collections | [datamanager-architecture.md](datamanager-architecture.md) |
| **GameStateManager** | Global state machine quản lý vòng đời application | [gamestate-architecture.md](gamestate-architecture.md) |
| **UIManager** | UI layer management theo SOLID principles | [uimanager-architecture.md](uimanager-architecture.md) |

## Kiến trúc tổng thể

```
GameFoundation
├── DataManager          ← Persistence layer
│   ├── GameData         ← Main save container
│   ├── UserData         ← User profile
│   ├── GameConfig       ← Game configuration
│   └── Collections      ← Extensible data collections
│
├── GameStateManager     ← Application lifecycle
│   ├── GameState enum   ← 13 states (None → Revice)
│   └── Events           ← OnGameStateChanged
│
└── UIManager            ← Presentation layer
    ├── Scene Layer      ← Full-screen scenes
    ├── Popup Layer      ← Modal popups
    ├── Dialog Layer     ← Blocking dialogs
    ├── Toast Layer      ← Notifications
    └── Loading Layer    ← Loading screens
```

## Nguyên tắc thiết kế

### 1. Event-Driven Architecture
Tất cả các hệ thống giao tiếp qua events, không reference trực tiếp:
- `GameStateManager.OnGameStateChanged` → UI react
- `DataManager` → Collections serialize/deserialize
- `UIBase.OnShow/OnHide` → Subclasses override

### 2. SOLID Principles
- **Single Responsibility**: Mỗi class một nhiệm vụ duy nhất
- **Open/Closed**: Extend qua inheritance, không modify base classes
- **Liskov Substitution**: Subclasses thay thế được base classes
- **Interface Segregation**: Interfaces nhỏ gọn (`IUIAnim`, `ISavableCollection`)
- **Dependency Inversion**: Depend on abstractions, không depend on concretions

### 3. Separation of Concerns
- **DataManager**: KHÔNG biết về UI hoặc GameState
- **GameStateManager**: KHÔNG biết về implementation của UI hoặc Data
- **UIManager**: Chỉ react với GameState, KHÔNG gọi DataManager trực tiếp

## Cách sử dụng với Unity Game Studio

### Khi nào dùng GameFoundation?

✅ **Dùng khi:**
- Cần save/load system phức tạp với nhiều data types
- Game có nhiều screens/scenes cần quản lý lifecycle
- Cần UI system với nhiều layers (popup, dialog, toast)
- Muốn architecture rõ ràng, dễ test, dễ maintain

❌ **Không dùng khi:**
- Prototype đơn giản không cần persistency
- Game quá nhỏ (1 scene, không có save/load)
- Đã có framework khác (Game Foundation by Unity, PlayFab)

### Integration với Unity Game Studio

GameFoundation được thiết kế để tích hợp với Unity Game Studio architecture:

| Unity Game Studio | GameFoundation | Ghi chú |
|-------------------|----------------|---------|
| `GameManager` (Engine layer) | `GameStateManager` | GameManager có thể sử dụng GameStateManager để quản lý state |
| `SceneController` | `UIManager.Scene Layer` | UIManager quản lý UI scenes, SceneController quản lý Unity scenes |
| `SaveSystem` | `DataManager` | DataManager cung cấp implementation cho SaveSystem interface |
| `ServiceLocator` | Registry pattern | DataManager/UIManager có thể register vào ServiceLocator |

## Quy ước đặt tên

Khi implement GameFoundation trong Unity Game Studio:

```
Assets/
├── Scripts/
│   ├── Core/                      ← Engine layer
│   │   ├── GameFoundation/
│   │   │   ├── DataManager/       ← DataManager system
│   │   │   │   ├── Core/
│   │   │   │   │   ├── SaveData.cs
│   │   │   │   │   ├── ISavableCollection.cs
│   │   │   │   │   └── DataManager.cs
│   │   │   │   └── Collections/   ← Game-specific collections
│   │   │   │       ├── PlayerInventoryCollection.cs
│   │   │   │       └── ProgressCollection.cs
│   │   │   │
│   │   │   ├── GameState/         ← GameStateManager
│   │   │   │   ├── GameState.cs
│   │   │   │   ├── GameStateManager.cs
│   │   │   │   └── GameStateListeners.cs
│   │   │   │
│   │   │   └── UI/                ← UIManager system
│   │   │       ├── Core/
│   │   │       │   ├── IUIAnim.cs
│   │   │       │   ├── UIBase.cs
│   │   │       │   ├── UISceneBase.cs
│   │   │       │   ├── UIPopupBase.cs
│   │   │       │   ├── UIDialogBase.cs
│   │   │       │   ├── UIToastBase.cs
│   │   │       │   ├── UILoadingBase.cs
│   │   │       │   └── UIManager.cs
│   │   │       └── Animations/
│   │   │           ├── UIAnim.cs
│   │   │           ├── UIAnimDO.cs (DOTween)
│   │   │           └── UIAnimator.cs
│   │   │
│   └── UI/                        ← Concrete UI implementations
│       ├── Scenes/
│       │   ├── SceneMain.cs       : UISceneBase
│       │   ├── ScenePlay.cs       : UISceneBase
│       │   └── SceneResult.cs     : UISceneBase
│       ├── Popups/
│       │   ├── PopupSettings.cs   : UIPopupBase
│       │   └── PopupShop.cs       : UIPopupBase
│       └── Dialogs/
│           └── DialogConfirm.cs   : UIDialogBase
│
└── Data/                          ← ScriptableObjects
    ├── GameConfig.asset           ← GameConfig instance
    └── Collections/
```

## Best Practices

### DataManager
- ✅ Mỗi collection tự định nghĩa `SaveData` class
- ✅ Implement `ISavableCollection` interface
- ✅ Register collection qua Inspector list (không hardcode)
- ✅ Sử dụng `BinaryFormatter` cho polymorphic data
- ❌ KHÔNG serialize Unity objects (GameObject, Component)
- ❌ KHÔNG lưu references giữa collections

### GameStateManager
- ✅ Transition states qua `GameStateManager.Play()`, `Ready()`, etc.
- ✅ Lắng nghe `OnGameStateChanged` event
- ✅ Truyền data qua `object data` parameter khi cần context
- ❌ KHÔNG set `CurrentState` trực tiếp
- ❌ KHÔNG skip states (phải theo flow: Init → Ready → Play)

### UIManager
- ✅ Scene UI kế thừa `UISceneBase` — chỉ UIManager gọi Show/Hide
- ✅ Popup kế thừa `UIPopupBase` — static `Show()` gọi từ mọi nơi
- ✅ Implement `IUIAnim` cho custom animations
- ✅ Override `OnShow(object data)` / `OnHide()` cho logic
- ❌ KHÔNG gọi `SceneMain.Show()` trực tiếp — dùng `GameStateManager.Main()`
- ❌ KHÔNG nest scenes trong scenes

## Tham khảo chi tiết

- [DataManager Architecture](datamanager-architecture.md) — Save/load system implementation
- [GameState Architecture](gamestate-architecture.md) — State machine với GameplayController
- [GameState Simplified](gamestate-simplified.md) — Chỉ có GameStateManager (đơn giản hơn)
- [UIManager Architecture](uimanager-architecture.md) — UI layer system với SOLID

## Liên hệ & Support

Tài liệu này được extract từ dự án **V06N-Main**. Nếu cần clarification về implementation cụ thể, tham khảo source code gốc hoặc liên hệ team lead.

---

**Version:** 1.0  
**Last Updated:** April 14, 2026  
**Maintained by:** Unity Game Studio Team
