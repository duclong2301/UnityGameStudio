# V06 Game Foundation - GameState & GameStateManager Architecture

## Mục lục

1. [Tổng quan hệ thống](#1-tổng-quan-hệ-thống)
2. [GameState Enum](#2-gamestate-enum)
3. [GameStateManager](#3-gamestatemanager)
4. [GameplayController & State nội bộ](#4-gameplaycontroller--state-nội-bộ)
5. [Sơ đồ chuyển trạng thái](#5-sơ-đồ-chuyển-trạng-thái)
6. [Hệ thống lắng nghe trạng thái (Listeners)](#6-hệ-thống-lắng-nghe-trạng-thái-listeners)
7. [Trigger hệ thống dựa trên GameState](#7-trigger-hệ-thống-dựa-trên-gamestate)
8. [Tích hợp với Mission System](#8-tích-hợp-với-mission-system)
9. [Tích hợp với UI System](#9-tích-hợp-với-ui-system)
10. [Luồng game chính (Game Flow)](#10-luồng-game-chính-game-flow)
11. [Kiến trúc tổng thể & Dependency Map](#11-kiến-trúc-tổng-thể--dependency-map)

---

## 1. Tổng quan hệ thống

Dự án V06 là một game đua xe/lái xe open-world trên Unity, sử dụng kiến trúc **hai lớp State Machine** để quản lý toàn bộ vòng đời game:

| Lớp | Class | Phạm vi | Giao tiếp |
|-----|-------|---------|------------|
| **Global State** | `GameStateManager` (static) | Toàn bộ application lifecycle | Event-driven (`OnGameStateChanged`) |
| **Gameplay State** | `GameplayController` (singleton) | Trong gameplay scene | Event-driven (`OnStateChanged`) |

### Nguyên tắc thiết kế

- **GameStateManager** điều phối trạng thái ở **cấp độ ứng dụng** — khởi tạo, menu chính, chơi game, kết thúc level.
- **GameplayController** điều phối trạng thái ở **cấp độ gameplay** — nhận/từ chối nhiệm vụ, đang làm nhiệm vụ, hoàn thành, thất bại.
- Hai hệ thống **giao tiếp hai chiều**: `GameplayController` lắng nghe `GameStateManager.OnGameStateChanged` và cũng gọi ngược lại `GameStateManager.Play()`, `GameStateManager.Complete()`, v.v.

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
| `None` | Trạng thái mặc định ban đầu | Khi app vừa khởi động |
| `Init` | Đang load scene gameplay, spawn player, khởi tạo traffic | `GameManager.InitGame()` gọi khi bắt đầu vào game |
| `Main` | Ở màn hình chính/lobby | Khi quay về menu, chọn xe, garage |
| `Ready` | Scene đã load xong, player đã spawn, đếm ngược | Sau `Init` hoàn tất, trước khi cho phép chơi |
| `Play` | Player đang lái xe, thực hiện nhiệm vụ | Sau countdown kết thúc |
| `Restart` | Yêu cầu chơi lại nhiệm vụ hiện tại | Player chọn Restart từ UI Result |
| `Complete` | Nhiệm vụ thành công, hiển thị UI kết quả win | Sau delay khi `WaitComplete` xử lý xong |
| `GameOver` | Nhiệm vụ thất bại, hiển thị UI kết quả lose | Sau delay khi `WaitGameOver` xử lý xong |
| `Next` | Chuyển sang nhiệm vụ kế tiếp | Player chọn Next từ màn hình kết quả |
| `WaitComplete` | Trạng thái trung gian — dừng player, tính toán kết quả | Trigger vùng hoàn thành hoặc mission complete |
| `WaitGameOver` | Trạng thái trung gian — dừng player, tính toán kết quả | Trigger vùng thất bại hoặc mission failed |
| `Other` | Trạng thái loading chuyển tiếp | Khi đang load prefab mission mới (`RequestMission`) |
| `Revice` | Hồi sinh sau khi game over | Player xem ads/trả phí để tiếp tục |

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
    public static void Ready(object data);        // → GameState.Ready (kèm data, thường là GameplayController.State)
    public static void Play();                    // → GameState.Play
    public static void Play(object data);         // → GameState.Play (kèm data, thường là GameplayController.State)
    public static void Restart();                 // → GameState.Restart
    public static void Complete();                // → GameState.Complete
    public static void Complete(object data);     // → GameState.Complete (kèm MissionResult)
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
- **Data passing** — hầu hết method hỗ trợ truyền `object data` để các listener nhận thông tin bổ sung (ví dụ: `GameplayController.State`, `MissionResult`).
- **Source code không public** — class này tồn tại trong DLL đã compile, cho thấy đây là **framework/engine code** thuộc Base layer.

---

## 4. GameplayController & State nội bộ

### File: `Assets/_FRG/Scripts/GameplayController.cs`

`GameplayController` là **singleton** quản lý toàn bộ logic gameplay trong scene, chạy song song với `GameStateManager`.

### State Enum (Gameplay-level)

```csharp
public enum State
{
    None,              // Chưa khởi tạo
    Init,              // Đang khởi tạo gameplay
    FreeRun,           // Lái xe tự do (không có nhiệm vụ)
    AcceptQuest,       // Đã chấp nhận nhiệm vụ, đang chuẩn bị
    DoingQuest,        // Đang thực hiện nhiệm vụ
    MissionCompleted,  // Nhiệm vụ hoàn thành
    CaptureSpeed,      // Đang capture tốc độ (mini-game)
    MissionFailed,     // Nhiệm vụ thất bại
    QuitMission,       // Bỏ nhiệm vụ giữa chừng
}
```

### Mapping giữa GameState ↔ GameplayController.State

```
GameState.Init          → State.Init
GameState.Ready         → PlayerState.Ready (player controller)
GameState.Play          → State.DoingQuest hoặc State.FreeRun (tùy context)
GameState.WaitComplete  → State.MissionCompleted
GameState.WaitGameOver  → State.MissionFailed
GameState.Restart       → RestartMainMission()
GameState.Next          → RequestMission() hoặc GameStateManager.Main()
```

### Lifecycle

```
Awake() → Đăng ký OnGameStateChanged
Start() → InitGame() coroutine
    → Tạo QuestMapHolder
    → Spawn player (player.PrepareCar)
    → Khởi tạo TrafficSystem
    → Nếu first-time → RequestMission(FTUE_Level)
    → Nếu không → GameStateManager.Ready() → GameStateManager.Play()
OnDestroy() → Hủy đăng ký, cleanup
```

---

## 5. Sơ đồ chuyển trạng thái

### 5.1 GameState — Luồng chính

```
                    ┌─────────────────────────────────────┐
                    │              APP START               │
                    └─────────────┬───────────────────────┘
                                  │
                                  ▼
                         ┌────────────────┐
                         │  GameState.None │
                         └───────┬────────┘
                                 │
                    GameStateManager.Init()
                                 │
                                 ▼
                    ┌────────────────────────┐
                    │    GameState.Init       │  ← Load gameplay scene
                    │  (GameManager.InitGame) │    Spawn player, traffic
                    └───────────┬────────────┘
                                │
                   GameStateManager.Ready()
                                │
                                ▼
                    ┌────────────────────────┐
                    │   GameState.Ready      │  ← Countdown (3, 2, 1, GO!)
                    │   Player positioned    │
                    └───────────┬────────────┘
                                │
                   GameStateManager.Play()
                                │
                                ▼
               ┌────────────────────────────────────┐
               │         GameState.Play              │  ← GAMEPLAY ACTIVE
               │   (FreeRun / DoingQuest)            │
               └──┬──────────┬──────────┬──────┬────┘
                  │          │          │      │
          Mission │   Mission│    Quit  │      │ RequestMission
         Complete │   Failed │   Mission│      │
                  │          │          │      │
                  ▼          ▼          │      ▼
    ┌──────────────┐ ┌──────────────┐  │  ┌──────────────┐
    │ WaitComplete │ │ WaitGameOver │  │  │ GameState     │
    │ (stop player │ │ (stop player │  │  │   .Other      │
    │  calc result)│ │  calc result)│  │  │ (loading...)  │
    └──────┬───────┘ └──────┬───────┘  │  └──────┬───────┘
           │                │          │         │
     delay │          delay │          │    Khi load xong
           ▼                ▼          │         │
    ┌──────────────┐ ┌──────────────┐  │         ▼
    │  Complete    │ │  GameOver    │  │    ┌──────────┐
    │ (show UI    │ │ (show UI     │  │    │  Ready   │
    │  win result)│ │  lose result)│  │    └────┬─────┘
    └──┬───┬──────┘ └──┬────┬──────┘  │         │
       │   │           │    │         │    Play()│
  Next │   │Main   Revice  │Main     │         ▼
       │   │           │    │         │    ┌─────────┐
       ▼   │           ▼    │         │    │  Play   │
 ┌─────────┤    ┌──────────┐│         │    └─────────┘
 │  Next   │    │  Revice  ││         │
 │(next lv)│    │(continue)││         │
 └────┬────┘    └────┬─────┘│         │
      │              │      │         │
      │         Play()│      │         │
      │              ▼      │         │
      │         ┌────────┐  │         │
      │         │  Play  │  │         │
      │         └────────┘  │         │
      │                     │         │
      ▼                     ▼         │
 ┌──────────────────────────────┐     │
 │        GameState.Main        │◄────┘
 │   (lobby / main menu)       │
 └──────────────┬───────────────┘
                │
           Init() ──→ (quay lại đầu)
```

### 5.2 GameplayController.State — Luồng nhiệm vụ

```
    ┌──────────┐
    │  None    │
    └────┬─────┘
         │ Init
         ▼
    ┌──────────┐     Suggest quest / Player enters zone
    │  Init    │─────────────────────────────────────┐
    └────┬─────┘                                     │
         │ Ready + Play                              │
         ▼                                           │
    ┌──────────────┐    AcceptQuest                   │
    │   FreeRun    │◄────────────────┐               │
    │ (lái xe tự do│    QuitMission  │               │
    │  timeout →   ├────────────────►│               │
    │  SuggestQuest│                 │               │
    └──────┬───────┘                 │               │
           │                         │               │
    RequestMission()                 │               │
           │                         │               │
           ▼                         │               │
    ┌──────────────┐                 │               │
    │ AcceptQuest  │─────────────────┤               │
    │ (load map,   │                 │               │
    │  setup)      │                 │               │
    └──────┬───────┘                 │               │
           │ Play(DoingQuest)        │               │
           ▼                         │               │
    ┌──────────────┐                 │               │
    │  DoingQuest  │                 │               │
    │ (mission     │                 │               │
    │  active)     │                 │               │
    └──┬───────┬───┘                 │               │
       │       │                     │               │
  Completed  Failed              QuitMission         │
       │       │                     │               │
       ▼       ▼                     │               │
 ┌──────────┐ ┌──────────────┐  ┌────────────┐      │
 │ Mission  │ │ Mission      │  │ QuitMission│──────►│
 │ Completed│ │ Failed       │  └────────────┘       │
 └──────────┘ └──────────────┘                       │
```

---

## 6. Hệ thống lắng nghe trạng thái (Listeners)

### 6.1 GameStateListeners

**File:** `Assets/Base/GameStateManager/GameStateListeners.cs`

Component này cho phép **cấu hình phản ứng với GameState bằng Inspector** (không cần code):

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

**Cơ chế:**
1. `Awake()` — đăng ký `GameStateManager.OnGameStateChanged`.
2. Khi GameState thay đổi → tìm event tương ứng trong danh sách → delay → thực thi `UnityEvent`.
3. Hỗ trợ `actionOnEnable` — tự động trigger khi `OnEnable` (dùng cho object được enable/disable theo state).

**Use case:** Bật/tắt UI panels, hiệu ứng, âm thanh, camera khi chuyển state mà không cần viết code.

### 6.2 GameplayStateListenner

**File:** `Assets/_FRG/Scripts/GameplayStateListenner.cs`

Tương tự `GameStateListeners` nhưng lắng nghe **GameplayController.State** (gameplay-level):

```csharp
public class GameplayStateListenner : MonoBehaviour
{
    public List<Data> listeners;

    [Serializable]
    public class Data
    {
        public GameplayController.State state;
        public UnityEvent<GameplayController.State> trigger;
    }
}
```

---

## 7. Trigger hệ thống dựa trên GameState

Các trigger là **collider-based components** đặt trong scene, khi player đi vào sẽ kích hoạt chuyển state.

### Base Class: TriggerGameState

**File:** `Assets/_FRG/Scripts/Elements/TriggerGameState.cs`

```
TriggerGameState (abstract base)
├── TriggerWaitComplete    → GameStateManager.WaitComplete()
├── TriggerWaitGameOver    → GameStateManager.WaitGameOver()
└── TriggerCheckingResult  → IMissionSummery.EndMission()
```

**Cơ chế hoạt động:**
1. Collider (`isTrigger = true`) phát hiện player đi vào.
2. Kiểm tra `GameStateManager.CurrentState == GameState.Play` (chỉ hoạt động khi đang chơi).
3. Xác minh tag `"Player"` và component `PlayerControllerBase`.
4. Chạy coroutine `IECheck()` (có delay `timeDetect`).
5. Subclass override `IECheck()` để gọi transition tương ứng.

### TriggerWaitComplete
Khi player lái xe vào vùng hoàn thành → `GameStateManager.WaitComplete()`.

### TriggerWaitGameOver  
Khi player lái xe vào vùng thất bại → `GameStateManager.WaitGameOver()`.

### TriggerCheckingResult
Xử lý logic phức tạp hơn — gọi `IMissionSummery.EndMission()` để mission tự quyết định kết quả.

---

## 8. Tích hợp với Mission System

### MissionState Enum

**File:** `Assets/_FRG/Scripts/MissionSystem/Core/MissionState.cs`

```csharp
public enum MissionState
{
    NotStarted,  // Nhiệm vụ tạo nhưng chưa bắt đầu
    Active,      // Đang thực hiện
    Paused,      // Tạm dừng
    Completed,   // Hoàn thành
    Failed,      // Thất bại
    Cancelled,   // Bị hủy bởi người chơi
}
```

### Luồng Mission ↔ GameState

```
RequestMission(StageLevel)
    │
    ├── GameStateManager.Other()           // Chuyển sang trạng thái loading
    ├── Tạo MissionData theo GameMode
    ├── MissionManager.RequestMission()
    │
    ▼ OnMissionRequested (callback)
    │
    ├── currentState = State.AcceptQuest
    ├── Load QuestMap prefab
    ├── Setup player position
    ├── GameStateManager.Ready()
    ├── UICountdown (3, 2, 1, GO!)
    └── GameStateManager.Play(State.DoingQuest)
         │
         ├── MissionManager.StartMission()
         │
    ┌────┴────────────────────────────────┐
    │                                      │
    ▼ OnMissionCompleted                   ▼ OnMissionFailed
    │                                      │
    GameStateManager.WaitComplete()        GameStateManager.WaitGameOver()
    │                                      │
    State.MissionCompleted                 State.MissionFailed
    │                                      │
    StopMissionUpdateComponent()           StopMissionUpdateComponent()
    │                                      │
    delay (2s)                             delay (2s)
    │                                      │
    GameStateManager.Complete()            GameStateManager.GameOver()
```

### GameMode — Các chế độ chơi

```csharp
public enum GameMode
{
    Time        = 0,   // Đua theo thời gian
    Destroy     = 1,   // Phá hủy mục tiêu
    Trial       = 2,   // Thử thách leo dốc
    JumpDistance = 3,   // Nhảy xa
    Pvp         = 4,   // Đua PvP với bot
    Free        = 5,   // Lái xe tự do
    Megaramp    = 6,   // Mega ramp (nhảy rampe khổng lồ)
    Parking     = 7,   // Đỗ xe
    EscapePolice= 8,   // Trốn cảnh sát
    SpeedCapture= 9,   // Bắn tốc độ
    Drift       = 10,  // Drift
    AirTime     = 11,  // Bay trên không
    JumpCrash   = 12,  // Nhảy & va chạm
    Taxi        = 13,  // Chở khách taxi
    All         = 999, // Tất cả mode
}
```

---

## 9. Tích hợp với UI System

### UIResultBase — Màn hình kết quả

**File:** `Assets/Samples/Scripts/UIResultBase.cs`

Xử lý hiển thị kết quả win/lose và điều hướng tiếp theo:

| Button Action | Gọi | Kết quả |
|---------------|------|---------|
| `Restart()` | `GameStateManager.Restart()` | Chơi lại nhiệm vụ |
| `Home()` | `GameStateManager.Main()` | Quay về menu chính |
| `NextStateAction(Next)` | `GameStateManager.Next()` | Chuyển level tiếp |
| `NextStateAction(Main)` | `GameStateManager.Main()` | Quay về menu chính |
| `NextStateAction(Play)` | `GameStateManager.Ready(State.QuitMission)` | Quay lại FreeRun |

### UIReviceBase — Hồi sinh

**File:** `Assets/Samples/Scripts/UIReviceBase.cs`

- Hiển thị khi `GameState.GameOver` hoặc `WaitGameOver`.
- Countdown timer cho phép hồi sinh.
- Hồi sinh miễn phí (giới hạn `reviceFree`) hoặc xem ads.
- Gọi `GameStateManager.Revice()` khi chấp nhận.
- Nếu từ chối → tiếp tục flow GameOver bình thường.

---

## 10. Luồng game chính (Game Flow)

### 10.1 Khởi động ứng dụng

```
App Launch
    │
    ▼
GameManager.Awake()
    ├── DontDestroyOnLoad
    ├── Đăng ký OnGameStateChanged
    └── Đăng ký OnQualityChanged
    │
    ▼
GameManager.Start()
    ├── Load GameConfig
    ├── Setup StageLevelParent
    └── Kiểm tra FTUE (First Time User Experience)
    │
    ▼
GameStateManager.Init()        ← Bắt đầu khởi tạo game
    │
    ▼
GameManager.IEInitGame()
    ├── Hiển thị loading screen
    ├── Load gameplay scene (SceneHelper.DOLoadScene)
    ├── Đợi scene sẵn sàng
    ├── Hiển thị Interstitial Ad (nếu cần)
    └── Ẩn loading screen
```

### 10.2 Vào Gameplay Scene

```
GameplayController.Awake()
    ├── Kiểm tra DataManager → nếu null → load lại scene 0
    ├── instance = this
    └── Đăng ký OnGameStateChanged
    │
    ▼
GameplayController.Start()
    └── StartCoroutine(InitGame())
        ├── Setup QuestMapHolder
        ├── player.PrepareCar(levelMap)
        ├── Khởi tạo TrafficSystem
        ├── Nếu first-time → RequestMission(FTUE_Level)
        └── Nếu không:
            ├── GameStateManager.Ready()
            └── GameStateManager.Play()    ← Player bắt đầu FreeRun
```

### 10.3 Luồng FreeRun → Nhận nhiệm vụ

```
FreeRun (lái xe tự do trong thành phố)
    │
    ├── Timeout → SuggestQuest() → UIRequestMission hiển thị
    │   └── Player chấp nhận → RequestMission(quest)
    │
    ├── Player lái vào QuestZone
    │   └── Trigger → RequestMission(quest)
    │
    └── Player từ UI chọn nhiệm vụ
        └── RequestMission(quest)
```

### 10.4 Quay về Main Menu

```
GameStateManager.Main()
    │
    ▼
GameManager.OnGameStateChanged(Main)
    ├── Set current stage = BigCity (list[0])
    └── GoToMain()
        ├── Hiển thị loading
        ├── GameplayController.SaveData()
        ├── Load idle scene
        ├── Ẩn loading
        └── Kiểm tra FTUE → GameStateManager.Init()
```

---

## 11. Kiến trúc tổng thể & Dependency Map

### Các component chính và quan hệ

```
┌─────────────────────────────────────────────────────────────────┐
│                        BASE LAYER (Framework)                    │
│                                                                  │
│  ┌──────────────────┐  ┌──────────────┐  ┌──────────────────┐  │
│  │ GameStateManager │  │ DataManager  │  │ CurrencyManager  │  │
│  │ (static, DLL)    │  │ (singleton)  │  │                  │  │
│  │                  │  │              │  │                  │  │
│  │ • CurrentState   │  │ • GameConfig │  │                  │  │
│  │ • OnGameState    │  │ • UserData   │  │                  │  │
│  │   Changed        │  │ • StageDatas │  │                  │  │
│  │ • Main/Init/     │  │ • Vehicles   │  │                  │  │
│  │   Ready/Play/... │  │ • Characters │  │                  │  │
│  └────────┬─────────┘  └──────────────┘  └──────────────────┘  │
│           │                                                      │
└───────────┼──────────────────────────────────────────────────────┘
            │ OnGameStateChanged event
            │
┌───────────┼──────────────────────────────────────────────────────┐
│           │          APPLICATION LAYER                            │
│           │                                                      │
│  ┌────────▼─────────┐                                            │
│  │   GameManager    │  ← Scene loading, Ads, FTUE                │
│  │   (singleton,    │                                            │
│  │    DontDestroy)  │                                            │
│  └──────────────────┘                                            │
│                                                                  │
└──────────────────────────────────────────────────────────────────┘
            │
┌───────────┼──────────────────────────────────────────────────────┐
│           │          GAMEPLAY LAYER                               │
│           │                                                      │
│  ┌────────▼──────────────┐     ┌────────────────────┐            │
│  │  GameplayController   │────►│   MissionManager   │            │
│  │  (singleton, per-scene│     │   (mission lifecycle│            │
│  │   • State machine     │     │    request/start/   │            │
│  │   • Mission flow      │     │    complete/fail)   │            │
│  │   • Player management │     └────────────────────┘            │
│  └───────────────────────┘                                       │
│           │                                                      │
│  ┌────────▼──────────┐  ┌────────────────┐  ┌────────────────┐  │
│  │      Player       │  │ RacingManager  │  │ TrafficSystem  │  │
│  │  • controller     │  │ • QuestZones   │  │ • NPC vehicles │  │
│  │  • vehicleBase    │  │ • Bot racing   │  │ • Pathfinding  │  │
│  └───────────────────┘  └────────────────┘  └────────────────┘  │
│                                                                  │
│  ┌───────────────────────────────────────────────────────────┐   │
│  │              TRIGGER SYSTEM (Scene Objects)                │   │
│  │  TriggerWaitComplete │ TriggerWaitGameOver │ TriggerCheck │   │
│  └───────────────────────────────────────────────────────────┘   │
│                                                                  │
│  ┌───────────────────────────────────────────────────────────┐   │
│  │              LISTENER SYSTEM (Inspector-configurable)      │   │
│  │  GameStateListeners  │  GameplayStateListenner            │   │
│  └───────────────────────────────────────────────────────────┘   │
│                                                                  │
└──────────────────────────────────────────────────────────────────┘
            │
┌───────────┼──────────────────────────────────────────────────────┐
│           │          UI LAYER                                    │
│           │                                                      │
│  ┌────────▼──────────┐  ┌────────────────┐  ┌────────────────┐  │
│  │   UIResultBase    │  │  UIReviceBase  │  │ UIRequestMission│  │
│  │  (win/lose screen)│  │  (revive popup)│  │ (quest suggest) │  │
│  └───────────────────┘  └────────────────┘  └────────────────┘  │
│                                                                  │
└──────────────────────────────────────────────────────────────────┘
```

### Subscriber Map — Ai lắng nghe GameState?

| Subscriber | File | Phản ứng chính |
|------------|------|----------------|
| `GameManager` | Samples/GameManager/GameManager.cs | Scene loading, Init/Main/Next flow |
| `GameplayController` | _FRG/Scripts/GameplayController.cs | Gameplay state transitions, Mission management |
| `GameStateListeners` | Base/GameStateManager/GameStateListeners.cs | Inspector-configured UnityEvents |
| `DataManager` | Base/DataManager/DataManager.cs | Data save/load |
| `RCC_Camera` | RealisticCarControllerV3/Scripts/RCC_Camera.cs | Camera behavior per state |
| `LoadScene` | _FRG/Scripts/LoadScene.cs | Scene transitions |
| `UIResultBase` | Samples/Scripts/UIResultBase.cs | Result screen actions |
| `UIReviceBase` | Samples/Scripts/UIReviceBase.cs | Revive popup |

---

## Tóm tắt

Kiến trúc Game Foundation của V06 được xây dựng trên:

1. **Hai lớp State Machine lồng nhau** — `GameStateManager` (global) điều phối vòng đời app, `GameplayController` (per-scene) điều phối logic gameplay.
2. **Event-driven architecture** — Giao tiếp thông qua C# events (`OnGameStateChanged`, `OnStateChanged`), giảm coupling giữa các component.
3. **Inspector-friendly listener system** — `GameStateListeners` và `GameplayStateListenner` cho phép designer cấu hình phản ứng mà không cần code.
4. **Trigger-based progression** — Collider triggers trong scene (`TriggerWaitComplete`, `TriggerWaitGameOver`) cho phép thiết kế level quyết định khi nào mission kết thúc.
5. **Mission System tách biệt** — `MissionManager` xử lý lifecycle nhiệm vụ riêng với `MissionState`, giao tiếp ngược với `GameStateManager` qua callback events.
6. **Data-driven flow** — `DataManager` cung cấp `GameConfig`, `UserData`, `StageDatas` cho mọi quyết định logic (FTUE, ads timing, revive limits, v.v.).
