Zen Game Foundation - UIManager Architecture (SOLID)

## Mục lục

1. [Tổng quan & SOLID Principles](#1-tổng-quan--solid-principles)
2. [Class Hierarchy](#2-class-hierarchy)
3. [IUIAnim — Animation Interface](#3-iuianim--animation-interface)
4. [UIBase — Abstract Base Class](#4-uibase--abstract-base-class)
5. [Scene Layer (UISceneBase)](#5-scene-layer-uiscenebase)
6. [Popup Layer (UIPopupBase)](#6-popup-layer-uipopupbase)
7. [Dialog Layer (UIDialogBase)](#7-dialog-layer-uidialogbase)
8. [Toast Layer (UIToastBase)](#8-toast-layer-uitoastbase)
9. [Loading Layer (UILoadingBase)](#9-loading-layer-uiloadingbase)
10. [UIManager — Orchestrator](#10-uimanager--orchestrator)
11. [Tích hợp với GameStateManager](#11-tích-hợp-với-gamestatemanager)
12. [Sơ đồ kiến trúc tổng thể](#12-sơ-đồ-kiến-trúc-tổng-thể)
13. [Lifecycle & Rendering Order](#13-lifecycle--rendering-order)
14. [Quy tắc thiết kế (SOLID Checklist)](#14-quy-tắc-thiết-kế-solid-checklist)

---

## 1. Tổng quan & SOLID Principles

UIManager là hệ thống quản lý toàn bộ giao diện người dùng, được tổ chức theo **5 layer** xếp chồng với thứ tự render cố định. Kiến trúc áp dụng **SOLID** để dễ mở rộng, dễ test và tái sử dụng cho mọi thể loại game.

### SOLID trong UIManager

| Nguyên tắc | Áp dụng |
|------------|---------|
| **S** — Single Responsibility | Mỗi class một nhiệm vụ: `UIBase` lo lifecycle, `IUIAnim` lo animation, `UIManager` lo điều phối, mỗi Layer base lo chính sách hiển thị của layer đó |
| **O** — Open/Closed | `UIBase` là abstract class mở cho kế thừa (extend), đóng cho sửa đổi. Thêm layer mới = tạo class kế thừa `UIBase`, không sửa UIManager logic |
| **L** — Liskov Substitution | Mọi class kế thừa `UIBase` đều dùng được ở nơi yêu cầu `UIBase`. `SceneMain : UISceneBase` thay thế được `UISceneBase` mà không phá vỡ UIManager |
| **I** — Interface Segregation | `IUIAnim` chỉ có `Show`/`Hide`/`Status` — interface nhỏ, tập trung. Không ép UI element phải implement animation nếu không cần (nullable) |
| **D** — Dependency Inversion | UIManager làm việc với `UIBase`/`UISceneBase`/`UIPopupBase`, không biết class cụ thể. `UIBase` discover `IUIAnim` qua `GetComponent` — không import tựa vào field |

### Thành phần chính

| Thành phần | Vai trò |
|------------|---------|
| `IUIAnim` (interface) | Abstraction cho animation show/hide — bất kỳ implementation nào (DOTween, Animator, custom) đều được |
| `UIBase` (abstract MonoBehaviour) | Base class cho **mọi** UI element — lifecycle, animation delegation, visibility state |
| `UISceneBase : UIBase` | Base cho Scene Layer — không cho phép gọi Show/Hide public, chỉ UIManager điều khiển |
| `UIPopupBase : UIBase` | Base cho Popup Layer — singleton, static Show/Hide, stack-able |
| `UIDialogBase : UIBase` | Base cho Dialog Layer — modal, block input, queue-based |
| `UIToastBase : UIBase` | Base cho Toast Layer — non-blocking, auto-hide |
| `UILoadingBase : UIBase` | Base cho Loading Layer — top-most, progress tracking |
| `UIManager` (singleton) | Orchestrator — lắng nghe GameState, điều phối Scene Layer, quản lý registry |

### Nguyên tắc cốt lõi

- **Layer-based rendering** — 5 layer với sort order cố định, layer trên luôn che layer dưới.
- **State-driven Scene** — Scene Layer **chỉ** thay đổi thông qua `GameState`, không cho phép gọi trực tiếp.
- **On-demand Popup/Dialog/Toast** — các layer còn lại có thể gọi từ bất kỳ đâu trong code.
- **Interface-driven animation** — `IUIAnim` cho phép swap animation engine mà không ảnh hưởng UI logic.

---

## 2. Class Hierarchy

### Sơ đồ kế thừa

```
                        ┌─────────────┐
                        │  IUIAnim    │  (interface)
                        │ Show/Hide   │
                        │ Status      │
                        └──────┬──────┘
                               │ implements
                    ┌──────────┼──────────────┐
                    │          │              │
              ┌─────┴────┐ ┌──┴───────┐ ┌────┴──────┐
              │ UIAnim   │ │UIAnimDO  │ │UIAnimator │  (concrete)
              │ (legacy) │ │(DOTween) │ │(Animator) │
              └──────────┘ └──────────┘ └───────────┘

       ┌──────────────────────────────────────────────┐
       │  UIBase (abstract MonoBehaviour)              │
       │  ┌ IUIAnim (nullable, GetComponent detect)    │
       │  ┌ UILayer layer (enum)                      │
       │  ┌ bool IsVisible                            │
       │  ├ virtual OnShow(object data) / OnHide()    │
       │  ├ Show(object data) / Hide(Action onDone)   │
       │  └ virtual OnInitialize() / OnDispose()      │
       └──────────┬───────────────────────────────────┘
                  │ inherits
        ┌─────────┼──────────┬────────────┬──────────────┐
        │         │          │            │              │
  ┌─────┴──────┐ ┌┴────────┐ ┌┴──────────┐ ┌┴──────────┐ ┌┴───────────┐
  │UISceneBase │ │UIPopup  │ │UIDialog   │ │UIToast    │ │UILoading   │
  │            │ │  Base   │ │  Base     │ │  Base     │ │  Base      │
  │ protected  │ │ static  │ │ modal     │ │ auto-hide │ │ progress   │
  │ Show/Hide  │ │ Show()  │ │ queue     │ │ stack     │ │ singleton  │
  └─────┬──────┘ └───┬─────┘ └────┬──────┘ └────┬──────┘ └─────┬──────┘
        │            │            │              │              │
   SceneMain    PopupSettings  DialogConfirm   UIToast      UILoading
   ScenePlay    PopupReward    DialogPurchase  UIFloatToast   ...
   SceneResult  PopupShop      ...             ...
   ...          ...
```

### UILayer Enum

```csharp
public enum UILayer
{
    Scene   = 0,    // Sort order 0–99
    Popup   = 1,    // Sort order 100–199
    Dialog  = 2,    // Sort order 200–299
    Toast   = 3,    // Sort order 300–399
    Loading = 4,    // Sort order 400+
}
```

### So sánh đặc tính

| Đặc tính | UISceneBase | UIPopupBase | UIDialogBase | UIToastBase | UILoadingBase |
|----------|-------------|-------------|--------------|-------------|---------------|
| **Trigger** | GameState only | Code / Inspector | Code / Inspector | Code | Code / Inspector |
| **Show public** | ❌ protected | ✅ static | ✅ static | ✅ static | ✅ static |
| **Block input** | Không | Tùy chọn | ✅ Luôn luôn | Không | ✅ Luôn luôn |
| **Stack nhiều** | ❌ 1 tại 1 thời điểm | ✅ Có | ✅ Queue | ✅ Nhiều toast | ❌ 1 tại 1 thời điểm |
| **Tự động ẩn** | Không | Không | Không | ✅ Có duration | Không |
| **Pause game** | Không | Tùy chọn | ✅ TimeScale = 0 | Không | Không |
| **IUIAnim** | Tùy chọn | Khuyến khích | Khuyến khích | Tùy chọn | Tùy chọn |

---

## 3. IUIAnim — Animation Interface

> **Interface Segregation (I):** `IUIAnim` chỉ định nghĩa animation show/hide. UI element không cần animation thì **không cần cắm component** — `UIBase` tự dùng `SetActive`.
>
> **Loose coupling:** `UIBase` không import hay reference `IUIAnim` trực tiếp. Nó dùng `GetComponent<IUIAnim>()` tại runtime — component tự nguyện tham gia, không bị ép buộc.

### Interface Definition

```csharp
public enum UIAnimStatus
{
    Initial,  // Khởi tạo
    Hide,     // Ẩn hoàn toàn
    Hiding,   // Đang chạy animation ẩn
    Showing,  // Đang chạy animation hiện
    Show,     // Hiện hoàn toàn
}

/// <summary>
/// Abstraction cho animation show/hide của UI element.
/// Cắm component implement interface này lên cùng GameObject với UIBase
/// để bật animation. Không cắm = SetActive ngay lập tức.
/// </summary>
public interface IUIAnim
{
    UIAnimStatus Status { get; }
    bool IsAnimating { get; }

    void Show(Action onStart = null, Action onDone = null);
    void Hide(Action onDone = null);

    event Action<UIAnimStatus> OnStatusChanged;
}
```

### Concrete Implementations

```csharp
// ─── Legacy implementation (đã có trong project) ───
public class UIAnim : MonoBehaviour, IUIAnim
{
    public UIAnimStatus Status => status;
    public bool IsAnimating => status == UIAnimStatus.IsShowing
                            || status == UIAnimStatus.IsHiding;
    // ... implementation existing
}

// ─── DOTween implementation ───
public class UIAnimDOTween : MonoBehaviour, IUIAnim
{
    [SerializeField] float showDuration = 0.3f;
    [SerializeField] float hideDuration = 0.2f;
    [SerializeField] Ease showEase = Ease.OutBack;
    [SerializeField] Ease hideEase = Ease.InBack;
    // ... DOTween sequences
}

// ─── Animator implementation ───
public class UIAnimAnimator : MonoBehaviour, IUIAnim
{
    [SerializeField] Animator animator;
    [SerializeField] string showTrigger = "Show";
    [SerializeField] string hideTrigger = "Hide";
    // ... Animator state machine
}
```

### Lựa chọn `IUIAnim` implementation

| Implementation | Khi nào dùng |
|----------------|-------------|
| `UIAnim` (legacy) | Code cũ, tương thích ngược |
| `UIAnimDOTween` | Cần animation mượt, custom easing |
| `UIAnimAnimator` | UI phức tạp, animation từ Animator Controller |
| *(không cắm)* | Show/Hide = `SetActive` ngay lập tức, không cần class thêm |

---

## 4. UIBase — Abstract Base Class

> **Single Responsibility (S):** `UIBase` chỉ lo lifecycle (init, show, hide, dispose). Animation là trách nhiệm của component riêng.
>
> **Open/Closed (O):** Thêm loại UI mới = tạo class kế thừa `UIBase` + override extension points. Không sửa `UIBase`.
>
> **Loose coupling:** `UIBase` không import `IUIAnim` làm field. Nó gọi `GetComponent<IUIAnim>()` trong `Awake` — component tự nguyện tham gia. Không có component = show/hide ngay lập tức.

### Implementation

```csharp
public abstract class UIBase : MonoBehaviour
{
    // ═══ SERIALIZED ═══
    [Header("UIBase")]
    [SerializeField] protected UILayer layer = UILayer.Scene;

    // ═══ RUNTIME ═══
    // Không khai báo kiểu IUIAnim — resolve qua GetComponent để tránh coupling
    private Component animComponent;  // giữ reference để tránh GetComponent lặp lại
    public UILayer Layer => layer;
    public bool IsVisible { get; protected set; }

    // ─── Lifecycle ───
    protected virtual void Awake()
    {
        // Tự discover IUIAnim nếu có cắm trên GameObject — không bắt buộc
        animComponent = GetComponent(typeof(IUIAnim)) as Component;
        OnInitialize();
    }

    protected virtual void OnDestroy() => OnDispose();

    // ─── Template Method Pattern ───
    public virtual void Show(object data = null)
    {
        if (IsVisible) return;
        IsVisible = true;
        gameObject.SetActive(true);

        var anim = animComponent as IUIAnim;
        if (anim != null)
            anim.Show(() => OnShowStart(data), () => OnShowDone(data));
        else
        {
            // Không có IUIAnim component → show ngay lập tức
            OnShowStart(data);
            OnShowDone(data);
        }
    }

    public virtual void Hide(Action onDone = null)
    {
        if (!IsVisible) { onDone?.Invoke(); return; }
        IsVisible = false;

        var anim = animComponent as IUIAnim;
        if (anim != null)
            anim.Hide(() => { OnHide(); gameObject.SetActive(false); onDone?.Invoke(); });
        else
        {
            // Không có IUIAnim component → hide ngay lập tức
            OnHide();
            gameObject.SetActive(false);
            onDone?.Invoke();
        }
    }

    // ─── Extension Points (override trong subclass) ───
    protected virtual void OnInitialize() { }
    protected virtual void OnDispose() { }
    protected virtual void OnShowStart(object data) { }
    protected virtual void OnShowDone(object data) { }
    protected virtual void OnHide() { }
}
```

### Quy tắc sử dụng

| Tình huống | Cách làm |
|------------|----------|
| Cần animation | Cắm `UIAnim` / `UIAnimDOTween` / `UIAnimAnimator` lên cùng GameObject |
| Không cần animation | Để trống — `UIBase` tự `SetActive` ngay lập tức |
| Đổi animation engine | Swap component trên GameObject, không sửa code |
| Tắt animation tạm thời | Disable component `IUIAnim` — `GetComponent` trả về null |

### Giải thích design pattern

| Pattern | Áp dụng |
|---------|---------|
| **Template Method** | `Show()` / `Hide()` kiểm soát flow (check state → activate → animate → callback). Subclass override `OnShowStart`/`OnShowDone`/`OnHide` để custom logic. |
| **Component** (Unity) | Animation là component độc lập, tự nguyện tham gia qua `GetComponent`. UIBase không cần biết loại cụ thể. |
| **Null Object** | Khi không có `IUIAnim` component, UIBase fallback về `SetActive` — không crash, không null check rải rác. |

---

## 5. Scene Layer (UISceneBase)

> **Quy tắc quan trọng nhất:** Scene Layer **hoàn toàn phụ thuộc vào GameState**. `Show()` / `Hide()` được kế thừa từ `UIBase` nhưng **sealed** — chỉ `UIManager` nội bộ gọi thông qua `Activate`/`Deactivate`. Bên ngoài muốn thay đổi Scene UI phải gọi `GameStateManager`.

### UISceneBase

```csharp
/// <summary>
/// Base cho mọi Scene UI. Show/Hide sealed, không public ra ngoài.
/// Chỉ UIManager được phép gọi thông qua Activate/Deactivate.
/// 
/// SOLID:
/// - S: Chỉ quản lý lifecycle của 1 scene screen
/// - O: Extend bằng override OnShowStart/OnHide, không sửa UISceneBase
/// - L: SceneMain, ScenePlay, SceneResult đều dùng được ở chỗ nhận UISceneBase
/// </summary>
public abstract class UISceneBase : UIBase
{
    public sealed override void Show(object data = null) => base.Show(data);
    public sealed override void Hide(Action onDone = null) => base.Hide(onDone);

    // ─── UIManager gọi qua đây ───
    internal void Activate(object data = null) => Show(data);
    internal void Deactivate(Action onDone = null) => Hide(onDone);
}
```

### Mapping GameState → Scene UI

| GameState | Scene UI | Mô tả |
|-----------|----------|-------|
| `None` | (không có UI) | App chưa khởi tạo |
| `Init` | `SceneLoading` | Đang khởi tạo level |
| `Main` | `SceneMain` | Màn hình chính / Lobby / Menu |
| `Ready` | `SceneReady` | Countdown, hướng dẫn trước khi chơi |
| `Play` | `ScenePlay` (HUD) | Giao diện gameplay: HP, score, minimap... |
| `WaitComplete` | `ScenePlay` (frozen) | Giữ HUD, dừng input |
| `WaitGameOver` | `ScenePlay` (frozen) | Giữ HUD, dừng input |
| `Complete` | `SceneResult` | Màn hình kết quả thắng |
| `GameOver` | `SceneResult` | Màn hình kết quả thua |
| `Revice` | `SceneRevice` | UI hồi sinh / tiếp tục |
| `Restart` | transition → `Init` | Chuyển tiếp nhanh |
| `Next` | transition → `Init` | Chuyển level tiếp |
| `Other` | `SceneLoading` | Loading nội dung mới |

### Ví dụ concrete class

```csharp
public class SceneMain : UISceneBase
{
    [SerializeField] Button playButton;
    [SerializeField] Button settingsButton;

    protected override void OnInitialize()
    {
        playButton.onClick.AddListener(() => GameStateManager.Init());
        settingsButton.onClick.AddListener(() => PopupSettings.Show());
    }

    protected override void OnShowDone(object data)
    {
        // Refresh UI data khi scene được hiển thị
    }
}

public class ScenePlay : UISceneBase
{
    [SerializeField] Text scoreText;
    [SerializeField] Slider hpBar;

    protected override void OnShowStart(object data)
    {
        scoreText.text = "0";
        hpBar.value = 1f;
    }

    protected override void OnHide()
    {
        // Cleanup gameplay HUD state
    }
}
```

### Flow: `GameStateManager.Main()` → SceneMain

```
1. Code gọi:          GameStateManager.Main()
2. GameStateManager:   CurrentState = Main
                       Fire OnGameStateChanged(Main, lastState, null)
3. UIManager nhận event:
   a) currentScene.Deactivate()    → ScenePlay.Hide() với IUIAnim
   b) CloseAllPopups()             → Ẩn tất cả popup
   c) sceneMain.Activate(data)     → SceneMain.Show() với IUIAnim
4. Các subscriber khác:
   - GameManager    → Load main menu scene
   - DataManager    → Save data
   - AudioManager   → Chuyển nhạc
```

---

## 6. Popup Layer (UIPopupBase)

> Popup hiển thị nội dung bổ sung **chồng lên** Scene Layer. Có thể gọi trực tiếp qua static API.

### UIPopupBase

```csharp
/// <summary>
/// Base cho mọi Popup. Generic Singleton + static Show/Hide.
/// 
/// SOLID:
/// - S: Quản lý 1 popup — show/hide + pause logic
/// - O: Override OnShowStart/OnHide để custom behavior
/// - L: Mọi PopupXxx : UIPopupBase đều dùng where UIPopupBase is expected
/// - D: Phụ thuộc IUIAnim, không phụ thuộc concrete animation
/// </summary>
public abstract class UIPopupBase<T> : UIBase where T : UIPopupBase<T>
{
    [Header("Popup Settings")]
    [SerializeField] protected bool pauseOnShow = false;
    [SerializeField] protected GameObject blockInputOverlay = null;

    protected static T instance;

    protected override void Awake()
    {
        base.Awake();
        instance = this as T;
        layer = UILayer.Popup;
    }

    // ─── Public Static API ───
    public static new void Show(object data = null) => instance?.ShowInternal(data);
    public static new void Hide(Action onDone = null) => instance?.HideInternal(onDone);
    public static bool IsShowing => instance != null && instance.IsVisible;

    private void ShowInternal(object data)
    {
        if (blockInputOverlay) blockInputOverlay.SetActive(true);
        if (pauseOnShow) Time.timeScale = 0f;
        UIManager.RegisterPopup(this);
        base.Show(data);
    }

    private void HideInternal(Action onDone)
    {
        base.Hide(() =>
        {
            if (blockInputOverlay) blockInputOverlay.SetActive(false);
            if (pauseOnShow) Time.timeScale = 1f;
            UIManager.UnregisterPopup(this);
            onDone?.Invoke();
        });
    }

    protected override void OnDispose()
    {
        if (instance == this as T) instance = null;
    }
}
```

### Ví dụ concrete class

```csharp
public class PopupSettings : UIPopupBase<PopupSettings>
{
    [SerializeField] Slider musicSlider;
    [SerializeField] Slider sfxSlider;

    protected override void OnShowStart(object data)
    {
        musicSlider.value = AudioManager.MusicVolume;
        sfxSlider.value = AudioManager.SFXVolume;
    }

    public void OnMusicChanged(float value) => AudioManager.MusicVolume = value;
    public void OnSFXChanged(float value) => AudioManager.SFXVolume = value;
}

// Sử dụng:
PopupSettings.Show();
PopupSettings.Hide();
```

### Đặc tính

- **Stack-able** — nhiều popup mở đồng thời, sort order tăng dần.
- **Không phụ thuộc GameState** — gọi `Show()` bất kỳ lúc nào.
- **Pause tùy chọn** — `pauseOnShow = true` tự động set `Time.timeScale = 0`.
- **Tự đăng ký** — `UIManager.RegisterPopup` / `UnregisterPopup` để UIManager biết cần close khi chuyển scene.

---

## 7. Dialog Layer (UIDialogBase)

> Dialog là **hộp thoại xác nhận**. Luôn block input, yêu cầu phản hồi trước khi tiếp tục. Queue-based.

### UIDialogBase

```csharp
/// <summary>
/// Base cho mọi Dialog. Modal, block input, queue-based.
///
/// SOLID:
/// - S: Quản lý 1 dialog — content + user response
/// - I: IUIAnim nullable — dialog đơn giản không cần animation
/// </summary>
public abstract class UIDialogBase<T> : UIBase where T : UIDialogBase<T>
{
    [Header("Dialog")]
    [SerializeField] protected GameObject modalOverlay = null;

    protected static T instance;
    private static readonly Queue<Action> dialogQueue = new Queue<Action>();
    private static bool isShowingDialog = false;

    protected Action onConfirmCallback;
    protected Action onCancelCallback;

    protected override void Awake()
    {
        base.Awake();
        instance = this as T;
        layer = UILayer.Dialog;
    }

    protected static void Enqueue(Action showAction)
    {
        dialogQueue.Enqueue(showAction);
        if (!isShowingDialog) ProcessQueue();
    }

    private static void ProcessQueue()
    {
        if (dialogQueue.Count == 0) { isShowingDialog = false; return; }
        isShowingDialog = true;
        dialogQueue.Dequeue()?.Invoke();
    }

    protected virtual void ShowDialog()
    {
        if (modalOverlay) modalOverlay.SetActive(true);
        Time.timeScale = 0f;
        base.Show();
    }

    protected virtual void CloseDialog(Action callback)
    {
        if (modalOverlay) modalOverlay.SetActive(false);
        Time.timeScale = 1f;
        base.Hide(() => { callback?.Invoke(); ProcessQueue(); });
    }

    protected override void OnDispose()
    {
        if (instance == this as T) instance = null;
    }
}
```

### Ví dụ concrete class

```csharp
public class DialogConfirm : UIDialogBase<DialogConfirm>
{
    [SerializeField] Text titleText;
    [SerializeField] Text messageText;
    [SerializeField] Button confirmButton;
    [SerializeField] Button cancelButton;

    protected override void OnInitialize()
    {
        confirmButton.onClick.AddListener(() => CloseDialog(onConfirmCallback));
        cancelButton.onClick.AddListener(() => CloseDialog(onCancelCallback));
    }

    public static void Show(string title, string message,
                            Action onConfirm = null, Action onCancel = null)
    {
        Enqueue(() =>
        {
            if (instance == null) return;
            instance.titleText.text = title;
            instance.messageText.text = message;
            instance.onConfirmCallback = onConfirm;
            instance.onCancelCallback = onCancel;
            instance.ShowDialog();
        });
    }
}

// Sử dụng:
DialogConfirm.Show("Thoát game?", "Tiến trình chưa được lưu.",
    onConfirm: () => GameStateManager.Main(),
    onCancel: () => { /* tiếp tục chơi */ });
```

### Phân biệt Dialog vs Popup

| | UIPopupBase | UIDialogBase |
|-|-------------|--------------|
| **Mục đích** | Hiển thị nội dung | Yêu cầu quyết định |
| **Block input** | Tùy chọn (`pauseOnShow`) | ✅ Luôn (`modalOverlay`) |
| **Đóng** | Nút X, back, tap outside | Chỉ qua button action |
| **Queue** | Không (stack) | ✅ Có (tuần tự) |
| **Pause** | Tùy chọn | ✅ Luôn `Time.timeScale = 0` |
| **Ví dụ** | Shop, Settings, Reward | "Bạn có chắc?", "Mua item?" |

---

## 8. Toast Layer (UIToastBase)

> Toast là **thông báo nhỏ**, không block input, tự biến mất sau duration.

### UIToastBase

```csharp
/// <summary>
/// Base cho mọi Toast notification.
///
/// SOLID:
/// - S: Chỉ lo hiển thị thông báo tạm thời
/// - O: Thêm toast type mới = kế thừa UIToastBase
/// - I: IUIAnim optional — toast đơn giản dùng SetActive
/// </summary>
public abstract class UIToastBase<T> : UIBase where T : UIToastBase<T>
{
    [Header("Toast")]
    [SerializeField] protected float defaultDuration = 3f;

    protected static T instance;
    protected Coroutine autoHideCoroutine;

    protected override void Awake()
    {
        base.Awake();
        instance = this as T;
        layer = UILayer.Toast;
    }

    protected void ShowWithAutoHide(object data, float duration)
    {
        if (autoHideCoroutine != null) StopCoroutine(autoHideCoroutine);
        base.Show(data);
        autoHideCoroutine = StartCoroutine(AutoHideAfter(duration));
    }

    private IEnumerator AutoHideAfter(float duration)
    {
        yield return new WaitForSecondsRealtime(duration);
        base.Hide();
    }

    protected override void OnDispose()
    {
        if (instance == this as T) instance = null;
    }
}
```

### Ví dụ concrete class (tương thích API cũ)

```csharp
public enum ToastType { Notice, Loading, Error }

public class UIToast : UIToastBase<UIToast>
{
    [SerializeField] Image icon;
    [SerializeField] Text message;
    [SerializeField] Sprite iconNotice, iconLoading, iconError;

    public static ToastType Type { get; private set; }

    public static void ShowNotice(string msg, float duration = 3f, Sprite customIcon = null)
    {
        if (instance == null) return;
        Type = ToastType.Notice;
        instance.message.text = msg;
        instance.icon.sprite = customIcon ?? instance.iconNotice;
        instance.ShowWithAutoHide(null, duration);
    }

    public static void ShowError(string msg, float duration = 3f, Sprite customIcon = null)
    {
        if (instance == null) return;
        Type = ToastType.Error;
        instance.message.text = msg;
        instance.icon.sprite = customIcon ?? instance.iconError;
        instance.ShowWithAutoHide(null, duration);
    }

    public static void ShowLoading(string msg, float duration = 3f, Sprite customIcon = null)
    {
        if (instance == null) return;
        Type = ToastType.Loading;
        instance.message.text = msg;
        instance.icon.sprite = customIcon ?? instance.iconLoading;
        instance.ShowWithAutoHide(null, duration);
    }
}

// Sử dụng:
UIToast.ShowNotice("Đã lưu thành công!");
UIToast.ShowError("Kết nối thất bại.");
UIToast.ShowLoading("Đang tải...", 5f);
```

---

## 9. Loading Layer (UILoadingBase)

> Loading hiển thị **toàn màn hình**, che tất cả layer khác. Dùng cho scene transition.

### UILoadingBase

```csharp
/// <summary>
/// Base cho Loading screen. Top-most layer, singleton.
///
/// SOLID:
/// - S: Chỉ lo hiển thị loading progress
/// - D: Phụ thuộc IUIAnim, không hard-code animation engine
/// </summary>
public abstract class UILoadingBase<T> : UIBase where T : UILoadingBase<T>
{
    [Header("Loading")]
    [SerializeField] protected Slider progressSlider;
    [SerializeField] protected Text statusText;

    protected static T instance;

    protected override void Awake()
    {
        base.Awake();
        instance = this as T;
        layer = UILayer.Loading;
    }

    public static void ShowLoading(float targetValue = 1f, float duration = 1f, string status = null)
    {
        if (instance == null) return;
        instance.SetupLoading(targetValue, duration, status);
        instance.Show(null);
    }

    public static void HideLoading() => instance?.Hide();

    protected abstract void SetupLoading(float targetValue, float duration, string status);

    protected override void OnDispose()
    {
        if (instance == this as T) instance = null;
    }
}
```

### Ví dụ concrete class

```csharp
public class UILoading : UILoadingBase<UILoading>
{
    [SerializeField] string[] tipTricks;
    [SerializeField] Image background;
    [SerializeField] string[] backgroundSprites;
    private static int bgIndex = 0;

    protected override void SetupLoading(float targetValue, float duration, string status)
    {
        if (progressSlider) progressSlider.value = 0;
        if (statusText) statusText.text = string.IsNullOrEmpty(status)
            ? (tipTricks.Length > 0 ? tipTricks[Random.Range(0, tipTricks.Length)] : "Loading...")
            : status;

        if (backgroundSprites != null && backgroundSprites.Length > 0)
        {
            bgIndex = bgIndex % backgroundSprites.Length;
            background.sprite = Resources.Load<Sprite>(backgroundSprites[bgIndex++]);
        }

        if (progressSlider)
            StartCoroutine(AnimateProgress(targetValue, duration));
    }

    private IEnumerator AnimateProgress(float target, float duration)
    {
        float elapsed = 0, start = progressSlider.value;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            progressSlider.value = Mathf.Lerp(start, target, elapsed / duration);
            yield return null;
        }
        progressSlider.value = target;
    }
}
```

### Flow Loading trong chuyển scene

```
GameStateManager.Init()
       │
       ▼
UIManager → UILoading.ShowLoading(0.5f, 2f)
       │
       ▼
GameManager → LoadScene async → progress → UILoading slider
       │
       └── Load complete → UILoading.HideLoading()
                               │
                               ▼
                    GameStateManager.Ready()
```

---

## 10. UIManager — Orchestrator

> **S:** UIManager chỉ lo **điều phối** — lắng nghe GameState, chuyển Scene, quản lý registry.
>
> **D:** UIManager làm việc với `UISceneBase`, `UIBase` — không biết đến class cụ thể.

### Implementation

```csharp
public class UIManager : MonoBehaviour
{
    // ═══ SINGLETON ═══
    protected static UIManager instance;
    public static UIManager Instance => instance;

    // ═══ SCENE REGISTRY (Inspector) ═══
    [Header("Scene UI Mapping")]
    [SerializeField] UISceneBase sceneMain;
    [SerializeField] UISceneBase sceneReady;
    [SerializeField] UISceneBase scenePlay;
    [SerializeField] UISceneBase sceneResult;
    [SerializeField] UISceneBase sceneRevice;
    [SerializeField] UISceneBase sceneLoading;

    // ═══ RUNTIME ═══
    private UISceneBase currentScene;
    private readonly List<UIBase> activePopups = new List<UIBase>();

    private void Awake()
    {
        instance = this;
        GameStateManager.OnGameStateChanged += OnGameStateChanged;
    }

    private void OnDestroy()
    {
        GameStateManager.OnGameStateChanged -= OnGameStateChanged;
    }

    // ═══ GAMESTATE → SCENE LAYER ═══
    private void OnGameStateChanged(GameState current, GameState last, object data)
    {
        switch (current)
        {
            case GameState.Main:     TransitionScene(sceneMain, data); break;
            case GameState.Init:
            case GameState.Other:    TransitionScene(sceneLoading, data); break;
            case GameState.Ready:    TransitionScene(sceneReady, data); break;
            case GameState.Play:     TransitionScene(scenePlay, data); break;
            case GameState.Complete:
            case GameState.GameOver: TransitionScene(sceneResult, data); break;
            case GameState.Revice:   TransitionScene(sceneRevice, data); break;
            case GameState.Restart:
            case GameState.Next:     CloseAllPopups(); break;
        }

        if (IsSceneTransition(current, last))
            CloseAllPopups();
    }

    private void TransitionScene(UISceneBase target, object data = null)
    {
        if (currentScene == target) return;
        currentScene?.Deactivate();
        currentScene = target;
        currentScene?.Activate(data);
    }

    // ═══ POPUP MANAGEMENT ═══
    public static void RegisterPopup(UIBase popup)   => instance?.activePopups.Add(popup);
    public static void UnregisterPopup(UIBase popup)  => instance?.activePopups.Remove(popup);

    public static void CloseAllPopups()
    {
        if (instance == null) return;
        for (int i = instance.activePopups.Count - 1; i >= 0; i--)
            instance.activePopups[i]?.Hide();
        instance.activePopups.Clear();
    }

    private bool IsSceneTransition(GameState current, GameState last)
    {
        return current == GameState.Main || current == GameState.Init
            || current == GameState.Restart || current == GameState.Next
            || current == GameState.Other;
    }
}
```

---

## 11. Tích hợp với GameStateManager

### Sơ đồ tích hợp (SOLID)

```
┌──────────────────────┐
│   GameStateManager   │
│   (static class)     │
└──────────┬───────────┘
           │ OnGameStateChanged
           │
    ┌──────┴──────────────────────────────────┐
    │                                         │
    ▼                                         ▼
┌──────────────────────┐            ┌──────────────────┐
│    UIManager         │            │  GameManager     │
│    (Orchestrator)    │            │  DataManager     │
│                      │            │  AudioManager    │
│  Depends on:         │            │  ...             │
│  ├ UISceneBase (abs) │            └──────────────────┘
│  ├ UIPopupBase (abs) │
│  ├ UIBase      (abs) │
│  └ IUIAnim(interface)│
│                      │
│  Does NOT know:      │
│  ├ SceneMain         │
│  ├ PopupSettings     │
│  ├ UIAnimDOTween     │
│  └ Any concrete class│
└──────────────────────┘
```

### Bảng tương tác Layer × GameState

| GameState | Scene Layer | Popup Layer | Dialog Layer | Toast Layer | Loading Layer |
|-----------|-------------|-------------|--------------|-------------|---------------|
| `None` | — | — | — | — | — |
| `Init` | SceneLoading | Force close all | — | Giữ nguyên | Show |
| `Main` | **SceneMain** | Force close all | — | Giữ nguyên | Hide |
| `Ready` | SceneReady | Cho phép mở | Cho phép | Giữ nguyên | Hide |
| `Play` | **ScenePlay** | Cho phép mở | Cho phép | Giữ nguyên | Hide |
| `WaitComplete` | ScenePlay (freeze) | Block mới | — | Giữ nguyên | — |
| `WaitGameOver` | ScenePlay (freeze) | Block mới | — | Giữ nguyên | — |
| `Complete` | **SceneResult** | Cho phép mở | Cho phép | Giữ nguyên | — |
| `GameOver` | **SceneResult** | Cho phép mở | Cho phép | Giữ nguyên | — |
| `Revice` | SceneRevice | Block mới | — | Giữ nguyên | — |
| `Restart` | transition | Force close all | Force close | — | Show |
| `Next` | transition | Force close all | Force close | — | Show |
| `Other` | SceneLoading | Force close all | — | Giữ nguyên | Show |

---

## 12. Sơ đồ kiến trúc tổng thể

```
                         ┌─────────────────────────┐
                         │    GameStateManager      │
                         │    (static, event hub)   │
                         └────────────┬────────────┘
                                      │
                          OnGameStateChanged
                                      │
              ┌───────────────────────┼───────────────────────┐
              │                       │                       │
              ▼                       ▼                       ▼
    ┌─────────────────┐    ┌──────────────────┐    ┌──────────────────┐
    │    UIManager     │    │   GameManager    │    │   Other Systems  │
    │                  │    │                  │    │   (Audio, Data,  │
    │  ┌────────────┐  │    │  Scene loading   │    │    Camera...)    │
    │  │UISceneBase │◄─── GameState ONLY      │    └──────────────────┘
    │  │ (sort: 0+) │  │    │  Init game       │
    │  └────────────┘  │    └──────────────────┘
    │                  │
    │  ┌────────────┐  │     ┌──────────┐
    │  │UIPopupBase │◄────── │ IUIAnim  │ (interface)
    │  │(sort:100+) │  │     │          │
    │  └────────────┘  │     │ ┌────────┴───────────┐
    │                  │     │ │ UIAnim (legacy)     │
    │  ┌────────────┐  │     │ │ UIAnimDOTween       │
    │  │UIDialogBase│  │     │ │ UIAnimAnimator      │
    │  │(sort:200+) │  │     │ │ (không cắm)=SetActiv│
    │  └────────────┘  │     │ └────────────────────┘
    │                  │     │
    │  ┌────────────┐  │     │ Cắm lên cùng GameObject
    │  │UIToastBase │  │     │ UIBase tự GetComponent
    │  │(sort:300+) │  │     └──────────┘
    │  └────────────┘  │
    │                  │
    │  ┌────────────┐  │
    │  │UILoadingBas│  │
    │  │(sort:400+) │  │
    │  └────────────┘  │
    └─────────────────┘

    ┌─────────────────────── Class Hierarchy ──────────────────┐
    │                                                          │
    │  UIBase (abstract)                                       │
    │  ├── UISceneBase (sealed Show/Hide, internal Activate)   │
    │  │   ├── SceneMain                                       │
    │  │   ├── ScenePlay                                       │
    │  │   ├── SceneResult                                     │
    │  │   └── ...                                             │
    │  ├── UIPopupBase<T> (static Show/Hide, singleton)        │
    │  │   ├── PopupSettings                                   │
    │  │   ├── PopupReward                                     │
    │  │   └── ...                                             │
    │  ├── UIDialogBase<T> (modal, queue-based)                │
    │  │   ├── DialogConfirm                                   │
    │  │   └── ...                                             │
    │  ├── UIToastBase<T> (auto-hide, non-blocking)            │
    │  │   ├── UIToast                                         │
    │  │   ├── UIFloatToast                                    │
    │  │   └── ...                                             │
    │  └── UILoadingBase<T> (progress, top-most)               │
    │      └── UILoading                                       │
    └──────────────────────────────────────────────────────────┘
```

---

## 13. Lifecycle & Rendering Order

### Canvas Sort Order

| Layer | Sort Order Range | Base Class | Ý nghĩa |
|-------|-----------------|------------|---------|
| Scene | 0 – 99 | `UISceneBase` | Nền tảng, luôn render đầu tiên |
| Popup | 100 – 199 | `UIPopupBase<T>` | Popup chồng lên scene, mỗi popup +1 |
| Dialog | 200 – 299 | `UIDialogBase<T>` | Dialog luôn trên popup |
| Toast | 300 – 399 | `UIToastBase<T>` | Toast hiển thị trên dialog |
| Loading | 400+ | `UILoadingBase<T>` | Loading che tất cả |

### Lifecycle mẫu: Vào game từ Main Menu

```
Thời gian ──────────────────────────────────────────────────►

[Main]                [Init]           [Ready]        [Play]
   │                    │                 │              │
   ▼                    ▼                 ▼              ▼

Scene:  SceneMain ──► (hide) ─────────► SceneReady ──► ScenePlay
Popup:  PopupStage     (force close)     —              —
Loading:  —            Show ──────────► Hide            —
Toast:    —              —               —             (available)
```

### Lifecycle mẫu: Hoàn thành level

```
[Play]          [WaitComplete]       [Complete]
  │                  │                   │
  ▼                  ▼                   ▼

Scene:  ScenePlay   ScenePlay(frozen)   SceneResult
Popup:    —         (block new)         PopupReward → (available)
Dialog:   —           —                 (available)
Toast:  (active)    (keep)              (keep)
```

---

## 14. Quy tắc thiết kế (SOLID Checklist)

### DO ✅

1. **Kế thừa đúng base class** — Scene → `UISceneBase`, Popup → `UIPopupBase<T>`, Dialog → `UIDialogBase<T>`, Toast → `UIToastBase<T>`, Loading → `UILoadingBase<T>`.
2. **Override extension points** — `OnInitialize()`, `OnShowStart()`, `OnShowDone()`, `OnHide()`, `OnDispose()`. Không override `Awake/OnDestroy` trực tiếp (gọi `base.Awake()` nếu bắt buộc).
3. **Cắm `IUIAnim` component lên cùng GameObject** — chọn `UIAnim`, `UIAnimDOTween`, hoặc `UIAnimAnimator` tùy nhu cầu. Để trống nếu không cần animation.
4. **Scene UI luôn đi qua GameState** — muốn hiện SceneMain? Gọi `GameStateManager.Main()`.
5. **Toast cho feedback nhẹ** — thông báo nhỏ, tự ẩn, không yêu cầu action.
6. **Dialog cho quyết định quan trọng** — block input, yêu cầu confirm/cancel.
7. **Popup/Dialog dùng generic singleton** — `UIPopupBase<T>` tự quản lý instance.

### DON'T ❌

1. **Không gọi `SceneMain.Show()` trực tiếp** — `sealed override` ngăn chặn, phải qua `GameStateManager`.
2. **Không phụ thuộc vào concrete `UIAnim`** — dùng `IUIAnim` interface. Nếu cần đổi animation engine, chỉ swap component.
3. **Không đặt logic UI trong `UIManager`** — UIManager chỉ điều phối. Logic cụ thể thuộc về từng layer base / concrete class.
4. **Không mở Popup khi đang WaitComplete/WaitGameOver** — game đang xử lý kết quả.
5. **Không dùng Dialog cho thông báo đơn giản** — dùng Toast thay thế.
6. **Không set `Time.timeScale` từ nhiều nơi** — chỉ `UIDialogBase` và `UIPopupBase` (khi `pauseOnShow = true`) quản lý.
7. **Không tạo nhiều base class ngoài 5 layer** — nếu cần UI mới, xác định nó thuộc layer nào rồi kế thừa base tương ứng.

### SOLID Checklist cho class mới

```
Khi tạo UI class mới:

1. [S] Class này chỉ lo 1 việc?
   └── Nếu có cả animation + data logic → tách IUIAnim ra component riêng

2. [O] Có cần sửa base class không?
   └── Nếu có → sai hướng, override extension point thay vì sửa base

3. [L] Thay thế được base class ở mọi nơi?
   └── PopupShop phải dùng được ở chỗ nhận UIPopupBase<PopupShop>

4. [I] Có ép implement method không cần?
   └── IUIAnim chỉ 3 member → OK. Nếu interface > 5 member → tách nhỏ

5. [D] Phụ thuộc abstraction hay concrete?
   └── Dùng IUIAnim, UIBase, UISceneBase — không dùng UIAnim, SceneMain trực tiếp
```

### Cách chọn Layer phù hợp

```
Cần hiển thị UI?
    │
    ├── Phụ thuộc vào game state? ──► UISceneBase
    │   (main menu, HUD, result...)
    │
    ├── Hiển thị nội dung để xem? ──► UIPopupBase<T>
    │   (shop, settings, reward...)
    │
    ├── Cần người chơi quyết định? ──► UIDialogBase<T>
    │   (xác nhận, mua, thoát...)
    │
    ├── Thông báo nhỏ, tự ẩn? ──► UIToastBase<T>
    │   (save OK, error, bonus...)
    │
    └── Che toàn bộ khi tải? ──► UILoadingBase<T>
        (scene transition, download...)
```
