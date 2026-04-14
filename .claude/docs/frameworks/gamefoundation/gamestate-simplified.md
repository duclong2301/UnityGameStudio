Zen Game Foundation - GameState & GameStateManager Architecture

## Mục lục

1. [Tổng quan hệ thống](#1-tổng-quan-hệ-thống)
2. [GameState Enum](#2-gamestate-enum)
3. [GameStateManager](#3-gamestatemanager)
4. [Sơ đồ chuyển trạng thái](#4-sơ-đồ-chuyển-trạng-thái)
5. [GameStateListeners](#5-gamestatelisteners)
6. [Subscriber Map](#6-subscriber-map)
7. [Tóm tắt](#7-tóm-tắt)

---

## 1. Tổng quan hệ thống

Dự án V06 được xây dựng trên Unity. Lớp **Game Foundation** quản lý toàn bộ vòng đời ứng dụng thông qua `GameStateManager` — một static class điều phối trạng thái ở cấp độ application.

| Thành phần | Vai trò |
|------------|---------|
| `GameState` (enum) | Định nghĩa tất cả trạng thái có thể của application |
| `GameStateManager` (static class) | Điều phối chuyển trạng thái, phát event cho toàn bộ hệ thống |
| `GameStateListeners` (MonoBehaviour) | Cho phép cấu hình phản ứng với GameState bằng Inspector |

### Nguyên tắc thiết kế

- **Event-driven** — mọi component đăng ký lắng nghe `GameStateManager.OnGameStateChanged` để phản ứng khi trạng thái thay đổi.
- **Static access** — `GameStateManager` truy cập từ mọi nơi mà không cần reference.
- **Data passing** — hỗ trợ truyền `object data` kèm theo mỗi lần chuyển state để cung cấp ngữ cảnh cho listener.

---

## 2. GameState Enum

> **Lưu ý:** Source code của `GameState` enum và `GameStateManager` class **không nằm trong các file .cs** trong Assets folder — chúng được compile trực tiếp trong `Assembly-CSharp.dll`. Tài liệu này được xây dựng từ phân tích cách sử dụng thực tế trên toàn bộ codebase.

```csharp
public enum GameState
{
    None         = 0,   // Chưa khởi tạo
    Init         = 1,   // Đang khởi tạo game/level
    Main         = 2,   // Ở màn hình chính (Main Menu / Lobby)
    Ready        = 3,   // Đã chuẩn bị xong, sẵn sàng chơi
    Play         = 4,   // Đang chơi (gameplay active)
    Restart      = 5,   // Yêu cầu chơi lại level
    Complete     = 6,   // Level hoàn thành (đã xử lý kết quả)
    GameOver     = 7,   // Level thất bại (đã xử lý kết quả)
    Next         = 8,   // Chuyển sang level tiếp theo
    WaitComplete = 9,   // Đang chờ/xử lý kết quả hoàn thành
    WaitGameOver = 10,  // Đang chờ/xử lý kết quả thất bại
    Other        = 11,  // Trạng thái trung gian (đang tải mission mới)
    Revice       = 12,  // Hồi sinh / Tiếp tục sau khi thất bại
}
```

### Mô tả chi tiết từng trạng thái

| GameState | Mô tả | Khi nào xảy ra |
|-----------|-------|-----------------|
| `None` | Trạng thái mặc định ban đầu | Khi app vừa khởi động, trước khi Bootstrap load xong MainScene |
| `Main` | Ở màn hình chính/lobby | Ngay sau Bootstrap hoàn tất load MainScene, hoặc khi quay về menu |
| `Init` | Đang load GameplayScene, khởi tạo game objects và các hệ thống | User click Play → `GameStateManager.Init()` → `GameplayController.InitGame()` |
| `Ready` | Scene đã load xong, player đã sẵn sàng, countdown | Sau `Init` hoàn tất, trước khi cho phép chơi |
| `Play` | Player đang chơi, thực hiện mục tiêu | Sau countdown kết thúc |
| `Restart` | Yêu cầu chơi lại level hiện tại | Player chọn Restart từ UI Result |
| `Complete` | Level thành công, hiển thị UI kết quả win | Sau delay khi `WaitComplete` xử lý xong |
| `GameOver` | Level thất bại, hiển thị UI kết quả lose | Sau delay khi `WaitGameOver` xử lý xong |
| `Next` | Chuyển sang level kế tiếp | Player chọn Next từ màn hình kết quả |
| `WaitComplete` | Trạng thái trung gian — dừng gameplay, tính toán kết quả | Khi điều kiện thắng được kích hoạt |
| `WaitGameOver` | Trạng thái trung gian — dừng gameplay, tính toán kết quả | Khi điều kiện thua được kích hoạt |
| `Other` | Trạng thái loading chuyển tiếp | Khi đang load nội dung game mới |
| `Revice` | Hồi sinh / tiếp tục sau khi game over | Player xem ads/trả phí để tiếp tục |

---

## 3. GameStateManager

### API (Inferred từ codebase)

```csharp
public static class GameStateManager
{
    // ─── Properties ───
    public static GameState CurrentState { get; }

    // ─── Event ───
    public delegate void OnGameStateChangedDelegate(GameState current, GameState last, object data);
    public static event OnGameStateChangedDelegate OnGameStateChanged;

    // ─── State Transition Methods ───
    public static void Main();                    // → GameState.Main
    public static void Init();                    // → GameState.Init
    public static void Init(object data);         // → GameState.Init (kèm data)
    public static void Ready();                   // → GameState.Ready
    public static void Ready(object data);        // → GameState.Ready (kèm data)
    public static void Play();                    // → GameState.Play
    public static void Play(object data);         // → GameState.Play (kèm data)
    public static void Restart();                 // → GameState.Restart
    public static void Complete();                // → GameState.Complete
    public static void Complete(object data);     // → GameState.Complete (kèm data)
    public static void GameOver();                // → GameState.GameOver
    public static void GameOver(object data);     // → GameState.GameOver (kèm data)
    public static void Next();                    // → GameState.Next
    public static void WaitComplete();            // → GameState.WaitComplete
    public static void WaitGameOver();            // → GameState.WaitGameOver
    public static void Other();                   // → GameState.Other
    public static void Revice();                  // → GameState.Revice
}
```

### Đặc điểm kiến trúc

- **Static class** — truy cập từ mọi nơi mà không cần reference.
- **Event-driven** — tất cả subscriber đăng ký qua `OnGameStateChanged += handler`.
- **Data passing** — hầu hết method hỗ trợ truyền `object data` để các listener nhận thông tin bổ sung.
- **Source code không public** — class này tồn tại trong DLL đã compile, cho thấy đây là **framework/engine code** thuộc Base layer.

### Cách sử dụng

**Đăng ký lắng nghe:**
```csharp
void Awake()
{
    GameStateManager.OnGameStateChanged += OnGameStateChanged;
}

void OnDestroy()
{
    GameStateManager.OnGameStateChanged -= OnGameStateChanged;
}

void OnGameStateChanged(GameState current, GameState last, object data)
{
    switch (current)
    {
        case GameState.Init:
            // Xử lý khởi tạo
            break;
        case GameState.Play:
            // Bắt đầu gameplay
            break;
        case GameState.Complete:
            // Hiển thị kết quả thắng
            break;
        case GameState.GameOver:
            // Hiển thị kết quả thua
            break;
    }
}
```

**Gọi chuyển trạng thái:**
```csharp
GameStateManager.Init();                    // Bắt đầu khởi tạo
GameStateManager.Ready();                   // Sẵn sàng chơi
GameStateManager.Play();                    // Bắt đầu gameplay
GameStateManager.WaitComplete();            // Chờ xử lý hoàn thành
GameStateManager.Complete();                // Hoàn thành level
GameStateManager.Main();                    // Quay về menu
```

---

## 4. Sơ đồ chuyển trạng thái

```
                    ┌─────────────────────────────────────┐
                    │              APP START               │
                    └─────────────┬───────────────────────┘
                                  │
                                  ▼
                         ┌─────────────────┐
                         │  GameState.None  │  ← Default (app just launched)
                         └────────┬────────┘
                                  │
                     Bootstrap loads MainScene
                                  │
                      GameStateManager.Main()
                                  │
                                  ▼
                    ┌─────────────────────────┐
                    │     GameState.Main       │  ← Main Menu / Lobby
                    └────────────┬────────────┘
                                 │
                    User clicks Play button
                    GameStateManager.Init()
                    + SceneManager.LoadScene("GameplayScene")
                                 │
                                 ▼
                    ┌─────────────────────────┐
                    │     GameState.Init       │  ← GameplayScene loads
                    │   (GameplayController    │     GameplayController.InitGame()
                    │     .InitGame runs)      │     spawn objects, load data, etc.
                    └────────────┬────────────┘
                                 │
                    InitGame complete
                    GameStateManager.Ready()
                                 │
                                 ▼
                    ┌─────────────────────────┐
                    │     GameState.Ready      │  ← UI shows "READY" / countdown
                    └────────────┬────────────┘
                                 │
                    Countdown done (default 1s)
                    GameStateManager.Play()
                                 │
                                 ▼
               ┌─────────────────────────────────────┐
               │          GameState.Play              │  ← GAMEPLAY ACTIVE
               └──┬──────────┬──────────┬───────┬────┘
                  │          │          │       │
         Điều kiện│  Điều kiện│  Restart │       │ Load level mới
            thắng │     thua  │          │       │
                  │          │          │       │
                  ▼          ▼          ▼       ▼
    ┌──────────────┐ ┌──────────────┐ ┌────┐ ┌──────────────┐
    │ WaitComplete │ │ WaitGameOver │ │Init│ │ GameState     │
    │ (stop game,  │ │ (stop game,  │ └──┬─┘ │   .Other      │
    │  calc result)│ │  calc result)│    │   │ (loading...)  │
    └──────┬───────┘ └──────┬───────┘    │   └──────┬───────┘
           │                │        InitGame   Khi load xong
     delay │          delay │            │           │
           ▼                ▼            ▼           ▼
    ┌──────────────┐ ┌──────────────┐ ┌──────────┐ ┌──────────┐
    │  Complete    │ │  GameOver    │ │  Ready   │ │  Ready   │
    │ (show UI     │ │ (show UI     │ └────┬─────┘ └────┬─────┘
    │  win result) │ │  lose result)│      │Play()       │Play()
    └──┬───┬───────┘ └──┬────┬──────┘      ▼            ▼
       │   │            │    │          ┌──────┐    ┌──────┐
  Next │   │ Main   Revice  │ Main     │ Play │    │ Play │
       │   │            │    │          └──────┘    └──────┘
       ▼   │            ▼    │
 ┌─────────┤     ┌──────────┐│
 │  Next   │     │  Revice  ││
 │(next lv)│     │(continue)││
 └────┬────┘     └────┬─────┘│
      │               │Play() │
      │               ▼       │
      │          ┌────────┐   │
      │          │  Play  │   │
      │          └────────┘   │
      ▼                       ▼
 ┌──────────────────────────────────┐
 │         GameState.Main           │
 │     (lobby / main menu)          │
 └──────────────┬───────────────────┘
                │
        User clicks Play again
        GameStateManager.Init() ──→ (quay lại Init)
```

---

## 5. GameStateListeners

**File:** `Assets/Foundation/GameStateManager/GameStateListeners.cs`

Component cho phép **cấu hình phản ứng với GameState bằng Inspector** mà không cần viết code:

```csharp
public class GameStateListeners : MonoBehaviour
{
    public bool actionOnEnable = false;
    public List<GameStateEvent> events;          // Cấu hình trong Inspector

    [Serializable]
    public class GameStateEvent
    {
        public GameState state = GameState.None; // Trạng thái cần lắng nghe
        public float delay = 0.5f;               // Delay trước khi thực thi
        public UnityEvent action = null;          // Action thực thi (UnityEvent)
    }
}
```

### Cơ chế hoạt động

1. `Awake()` — đăng ký `GameStateManager.OnGameStateChanged`.
2. Khi GameState thay đổi → tìm `GameStateEvent` tương ứng trong danh sách `events`.
3. Nếu tìm thấy → delay theo `GameStateEvent.delay` → thực thi `UnityEvent action`.
4. Sử dụng `DOTween.Kill(GetInstanceID())` để cancel delay cũ nếu state thay đổi liên tục.
5. `actionOnEnable = true` → tự động trigger khi `OnEnable` theo `CurrentState` hiện tại (dùng cho object bật/tắt theo state).
6. `OnDestroy()` — hủy đăng ký event.

### Use case

- Bật/tắt UI panels khi chuyển state.
- Kích hoạt hiệu ứng, âm thanh, camera tại các thời điểm cụ thể.
- Designer cấu hình logic phản ứng trực tiếp trên Inspector mà không cần lập trình.

---

## 6. Subscriber Map

Các component chính lắng nghe `GameStateManager.OnGameStateChanged`:

| Subscriber | File | Phản ứng chính |
|------------|------|----------------|
| `GameManager` | GameManager.cs | Scene loading, Init/Main/Next flow |
| `GameplayController` | GameplayController.cs | Gameplay state transitions |
| `GameStateListeners` | GameStateListeners.cs | Inspector-configured UnityEvents |
| `DataManager` | DataManager.cs | Data save/load |
| `LoadScene` | LoadScene.cs | Scene transitions |

---

## 7. Tóm tắt

Lớp Game Foundation của V06 được xây dựng trên:

1. **GameStateManager (static)** — điều phối toàn bộ vòng đời ứng dụng với 13 trạng thái. Flow chính: `None` → `Main` (Bootstrap) → `Init` (user click Play, `GameplayController.InitGame()`) → `Ready` (countdown) → `Play` (gameplay) → `WaitComplete`/`WaitGameOver` → `Complete`/`GameOver`. Kiến trúc này phù hợp với mọi thể loại game: casual, puzzle, action, RPG, racing, v.v.
2. **Event-driven architecture** — giao tiếp thông qua `OnGameStateChanged` event, giảm coupling giữa các component.
3. **GameStateListeners** — component Inspector-friendly cho phép designer cấu hình phản ứng với state mà không cần code.
4. **Data passing** — mỗi transition hỗ trợ truyền `object data` để cung cấp ngữ cảnh cho listener.
