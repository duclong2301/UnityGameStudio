# DataManager Collection System  Implementation Blueprint

> **Version:** 3.0  
> **Target:** Unity project cần hệ thống save/load linh hoạt cho nhiều data collection  
> **Mục tiêu:** DataManager chỉ quản lý `UserData` + `GameConfig`. Mọi data collection khác đăng ký qua Inspector list, thêm/xoá không cần sửa code.  
> **Serialization:** Binary (`BinaryFormatter`) — polymorphic, không JSON, không `[SerializeReference]`

---

## FILE STRUCTURE

```
Assets/Foundation/DataManager/
├── Core/
│   ├── SaveData.cs                    // Abstract base class
│   ├── ISavableCollection.cs          // Interface cho collections
│   ├── CollectionDataSave.cs          // Save wrapper + entry classes
│   ├── GameData.cs                    // Main save data container
│   ├── GameConfig.cs                  // Game configuration
│   └── UserData.cs                    // User profile data
├── Manager/
│   └── DataManager.cs                 // Singleton manager
├── Editor/
│   └── DataManagerEditor.cs           // Custom Inspector (optional)
└── Demo/
    ├── Collections/
    │   ├── AchievementTrackerCollection.cs
    │   ├── PlayerInventoryCollection.cs
    │   ├── StageProgressCollection.cs
    │   ├── ShopCollection.cs
    │   └── SettingsDataCollection.cs
    ├── Items/
    │   ├── AchievementData.cs
    │   ├── InventoryItemData.cs
    │   ├── StageData.cs
    │   ├── ShopItemData.cs
    │   └── SettingsEntryData.cs
    └── Tests/
        ├── ComprehensiveDataManagerTest.cs
        ├── SaveLoadTestRunner.cs
        └── PersistenceTestRunner.cs
```

---

## MỤC LỤC

1. [Kiến trúc](#1-kiến-trúc)
2. [Infrastructure files](#2-infrastructure-files)
3. [GameData](#3-gamedata)
4. [DataManager](#4-datamanager)
5. [Implement ISavableCollection](#5-implement-isavablecollection)
6. [Thêm collection mới](#6-thêm-collection-mới)
7. [Thứ tự triển khai](#7-thứ-tự-triển-khai)
8. [DataManagerEditor](#8-datamanagereditor-optional-enhancement)
9. [Demo Collections](#9-demo-collections)
10. [Best Practices](#10-best-practices)
11. [Troubleshooting](#11-troubleshooting)
12. [Summary](#12-summary)

---

## 1. Kiến trúc

### Nguyên tắc

- DataManager chỉ identify: `UserData` + `GameConfig`
- Mọi data collection đăng ký qua `List<DataCollectionEntry>` trong Inspector
- `GameData` chứa core data + `List<CollectionDataSave>` (key + typed `SaveData` object)
- Mỗi collection tự define concrete `XxxSave : SaveData` + implement `ISavableCollection`
- `BinaryFormatter` serialize toàn bộ object graph  lưu type info trong binary stream, polymorphism native
- **Không cần** JSON string wrapper, **không cần** `[SerializeReference]`

### Sơ đồ

```
DataManager (MonoBehaviour Singleton)
 GameConfig               Cố định
 UserData                 Cố định
 List<DataCollectionEntry>   Inspector, thêm/xoá tuỳ ý
      key:"xxx"  SO_A (ISavableCollection)
      key:"yyy"  SO_B (ISavableCollection)
      key:"zzz"  SO_C (ISavableCollection)
            
        Save()/Load()
            
GameData [Serializable]  .bin file
 GameConfig
 UserData
 List<CollectionDataSave>
      { key:"xxx", data: MyASave { items:[...] } }    typed object
      { key:"yyy", data: MyBSave { items:[...] } }
      { key:"zzz", data: MyCSave { count:5, ... } }
```

### So sánh với JSON approach

| | JSON (`JsonUtility`) | Binary (`BinaryFormatter`) |
|---|---|---|
| **Polymorphism** | Không  cần wrapper string | Có  type info trong binary stream |
| **Type safety** | Không | Có  `is` pattern cast |
| **Boilerplate** | Wrapper class trong mỗi method | Chỉ define `XxxSave : SaveData` |
| **File readable** | Có (dễ debug) | Không |
| **File size** | Lớn hơn | Nhỏ hơn |
| **Rename class** | An toàn |  Đổi tên `XxxSave`  mất save cũ |

---

## 2. Infrastructure files

### 2.1 `SaveData.cs`

**Location:** `Assets/Foundation/DataManager/Core/SaveData.cs`

```csharp
using System;

/// <summary>
/// Abstract base class cho tất cả save data của collection.
/// Mỗi collection tự define concrete subclass.
/// BinaryFormatter serialize/deserialize đúng concrete type — không cần [SerializeReference].
/// </summary>
[Serializable]
public abstract class SaveData { }
```

### 2.2 `ISavableCollection.cs`

**Location:** `Assets/Foundation/DataManager/Core/ISavableCollection.cs`

```csharp
/// <summary>
/// Interface cho ScriptableObject collection có thể save/load qua DataManager.
/// ExportSaveData() trả typed SaveData object — BinaryFormatter xử lý binary.
/// </summary>
public interface ISavableCollection
{
    string CollectionKey { get; }
    SaveData ExportSaveData();
    void ImportSaveData(SaveData data);
    void UnlockAll();
    void ResetData();
}
```

### 2.3 `CollectionDataSave.cs`

**Location:** `Assets/Foundation/DataManager/Core/CollectionDataSave.cs`

```csharp
using System;
using UnityEngine;

[Serializable]
public class CollectionDataSave
{
    public string key;
    public SaveData data;  // BinaryFormatter giữ type info — polymorphism tự động
}

[Serializable]
public class DataCollectionEntry
{
    [Tooltip("Key duy nhất, match với ISavableCollection.CollectionKey")]
    public string key;

    [Tooltip("ScriptableObject source — phải implement ISavableCollection")]
    public ScriptableObject source;

    [NonSerialized] public ScriptableObject runtimeInstance;

    public ISavableCollection Savable => runtimeInstance as ISavableCollection;
}
```

---

## 3. GameData

**Location:** `Assets/Foundation/DataManager/Core/GameData.cs`

```csharp
using System;
using System.Collections.Generic;

[Serializable]
public class GameData
{
    public GameConfig gameConfig = new GameConfig();
    public UserData userData = new UserData { name = "YOU" };
    public List<CollectionDataSave> collections = new List<CollectionDataSave>();

    public GameData Clone() => new GameData
    {
        gameConfig = new GameConfig
        {
            bundleVersion            = gameConfig.bundleVersion,
            forceVersion             = gameConfig.forceVersion,
            inAppUpdate             = gameConfig.inAppUpdate,
            FTUE_Play               = gameConfig.FTUE_Play,
            FTUE_Data               = gameConfig.FTUE_Data,
            timePlayToShowAd       = gameConfig.timePlayToShowAd,
            timePlayToShowAdReduce= gameConfig.timePlayToShowAdReduce,
            timeToWaitOpenAd       = gameConfig.timeToWaitOpenAd,
            timePlayToShowOpenAd   = gameConfig.timePlayToShowOpenAd,
            adShowFromLevel        = gameConfig.adShowFromLevel,
            adInterOnStart         = gameConfig.adInterOnStart,
            adInterOnComplete      = gameConfig.adInterOnComplete,
            adShowInterFTUE         = gameConfig.adShowInterFTUE,
            adUseBackup             = gameConfig.adUseBackup,
            adUseBackupRewardInter  = gameConfig.adUseBackupRewardInter,
            adUseBackupBannerInter = gameConfig.adUseBackupBannerInter,
            adUseOpenBackup         = gameConfig.adUseOpenBackup,
            adUseNative             = gameConfig.adUseNative,
            adUseNativeClickReLoad= gameConfig.adUseNativeClickReLoad,
            adUseNativeTimeReLoad = gameConfig.adUseNativeTimeReLoad,
            adUseInPlay             = gameConfig.adUseInPlay,
            forceInterToReward      = gameConfig.forceInterToReward,
            rewardLevel            = gameConfig.rewardLevel,
            rewardByAd             = gameConfig.rewardByAd,
            rewardLevelScale       = gameConfig.rewardLevelScale,
            reviceCountDown        = gameConfig.reviceCountDown,
            reviceCountMax         = gameConfig.reviceCountMax,
            reviceNoThankCountDown = gameConfig.reviceNoThankCountDown,
        },
        userData = new UserData
        {
            name                 = userData.name,
            totalScore           = userData.totalScore,
            playCount            = userData.playCount,
            lastPlayedTimestamp  = userData.lastPlayedTimestamp,
        },
        // collections không copy từ inspector default — luôn bắt đầu rỗng
        collections = new List<CollectionDataSave>(),
    };

    public CollectionDataSave GetCollection(string key)
        => collections.Find(c => c.key == key);

    public void SetCollection(string key, SaveData data)
    {
        var existing = collections.Find(c => c.key == key);
        if (existing != null)
            existing.data = data;
        else
            collections.Add(new CollectionDataSave { key = key, data = data });
    }
}
```

### GameConfig & UserData

**GameConfig** (`Assets/Foundation/DataManager/Core/GameConfig.cs`):
```csharp
using System;
using UnityEngine;

[Serializable]
public class GameConfig
{
    public string bundleVersion = "1.0.0";
    public string forceVersion = "0.0.0";
    public bool inAppUpdate = true;
    public bool FTUE_Play = true;
    public string FTUE_Data = "";

    [Header("ADS Time Config")]
    public float timePlayToShowAd = 15f;
    public float timePlayToShowAdReduce = 15f;
    public float timeToWaitOpenAd = 8.6f;
    public float timePlayToShowOpenAd = 8.6f;
    public int adShowFromLevel;
    public bool adInterOnStart = true;
    public bool adInterOnComplete = true;
    public int adShowInterFTUE = 1;
    public bool adUseBackup;
    public bool adUseBackupRewardInter;
    public bool adUseBackupBannerInter;
    public bool adUseOpenBackup;
    public bool adUseNative;
    public bool adUseNativeClickReLoad = true;
    public float adUseNativeTimeReLoad = 15f;
    public bool adUseInPlay;
    public bool forceInterToReward;
    public int rewardLevel = 50;
    public int rewardByAd = 500;
    public float rewardLevelScale = 0.05f;
    public int reviceCountDown = 5;
    public int reviceCountMax = 3;
    public int reviceNoThankCountDown = 2;
}
```

**UserData** (`Assets/Foundation/DataManager/Core/UserData.cs`):
```csharp
using System;

[Serializable]
public class UserData
{
    public string name = "YOU";
    public int totalScore = 0;
    public int playCount = 0;
    public long lastPlayedTimestamp = 0L;
}
```

> **Lưu ý:** `GameConfig` và `UserData` phải có `[Serializable]` để `BinaryFormatter` serialize được.

---

## 4. DataManager

**Location:** `Assets/Foundation/DataManager/Manager/DataManager.cs`

```csharp
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Events;

#pragma warning disable SYSLIB0011

public class DataManager : MonoBehaviour
{
    protected static DataManager instance;
    public static bool IsLoaded = false;

    public delegate void LoadedDelegate();
    public static event LoadedDelegate OnLoaded;
    [SerializeField] protected UnityEvent onLoadCompleted;

    [SerializeField] protected GameData gameData = new GameData();
    public static GameData GameData;
    public static GameConfig GameConfig;
    public static UserData UserData;

    [Header("Data Collections")]
    [SerializeField]
    protected List<DataCollectionEntry> dataCollections = new List<DataCollectionEntry>();

    protected virtual void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    protected virtual void Start()
    {
        if (!IsLoaded)
            StartCoroutine(IELoad());
    }

    public static T GetCollection<T>(string key) where T : ScriptableObject
    {
        if (instance == null) return null;
        var entry = instance.dataCollections.Find(e => e.key == key);
        return entry?.runtimeInstance as T;
    }

    private static string SavePath => Path.Combine(Application.persistentDataPath, "savedata.bin");

    private static void WriteSave(GameData data)
    {
        var formatter = new BinaryFormatter();
        using var stream = new FileStream(SavePath, FileMode.Create);
        formatter.Serialize(stream, data);
        Debug.Log($"[DataManager] Saved to {SavePath}");
    }

    private static GameData ReadSave()
    {
        if (!File.Exists(SavePath)) return null;
        try
        {
            var formatter = new BinaryFormatter();
            using var stream = new FileStream(SavePath, FileMode.Open);
            return formatter.Deserialize(stream) as GameData;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[DataManager] Save file corrupted. Starting fresh.\\n{ex.Message}");
            return null;
        }
    }

    public static void Load()
    {
        if (instance != null)
            instance.StartCoroutine(instance.IELoad());
        else
            Debug.LogError("[DataManager] No instance found to load data.");
    }

    protected IEnumerator IELoad()
    {
        if (IsLoaded)
        {
            OnLoaded?.Invoke();
            onLoadCompleted?.Invoke();
            yield break;
        }

        GameData = gameData.Clone();
        GameConfig = GameData.gameConfig;
        UserData = GameData.userData;

        foreach (var entry in dataCollections)
        {
            if (entry.source != null)
                entry.runtimeInstance = Instantiate(entry.source);
        }

        var saveData = ReadSave();
        if (saveData != null)
        {
            GameData = saveData;
            GameConfig = GameData.gameConfig;
            UserData = GameData.userData;

            foreach (var entry in dataCollections)
            {
                if (entry.Savable == null) continue;
                var saved = GameData.GetCollection(entry.key);
                if (saved?.data != null)
                    entry.Savable.ImportSaveData(saved.data);
            }
        }

        OnAfterDataLoaded();

        IsLoaded = true;
        OnLoaded?.Invoke();
        onLoadCompleted?.Invoke();
        yield return null;
    }

    protected virtual void OnAfterDataLoaded() { }

    public static void Save()
    {
        if (instance == null || !IsLoaded) return;

        GameData.gameConfig = GameConfig;
        GameData.userData = UserData;
        GameData.collections.Clear();

        foreach (var entry in instance.dataCollections)
        {
            if (entry.Savable != null)
                GameData.SetCollection(entry.key, entry.Savable.ExportSaveData());
        }

        WriteSave(GameData);
    }

    public void UnlockAll()
    {
        foreach (var entry in dataCollections)
            entry.Savable?.UnlockAll();
        Save();
    }

    public void ResetData()
    {
        foreach (var entry in dataCollections)
        {
            entry.Savable?.ResetData();
#if UNITY_EDITOR
            if (entry.source != null)
                UnityEditor.EditorUtility.SetDirty(entry.source);
#endif
        }

        if (File.Exists(SavePath))
            File.Delete(SavePath);

        IsLoaded = false;
        Debug.Log("[DataManager] ResetData DONE — save file deleted. Reloading...");
        StartCoroutine(IELoad());
    }
}

#pragma warning restore SYSLIB0011
```

### Điểm chính

| Feature | Chi tiết |
|---|---|
| **Auto Load** | `Start()` tự động gọi `IELoad()` nếu chưa loaded |
| **Manual Load** | `DataManager.Load()` static method để load từ code |
| **Coroutine** | `IELoad()` protected, có thể override trong subclass |
| **Reset** | `ResetData()` xoá save + reload data mặc định |
| **Warning Suppression** | `#pragma warning disable SYSLIB0011` cho BinaryFormatter |
| **Event** | `OnLoaded` event + `onLoadCompleted` UnityEvent |
| **Hook** | `OnAfterDataLoaded()` virtual method để extend |

> **Tuỳ chọn — static accessor cho backward compatible:**
> ```csharp
> public static MyDatas MyDatas => GetCollection<MyDatas>("myKey");
> ```

---

## 5. Implement ISavableCollection

Mỗi collection: (1) define `[Serializable] XxxSave : SaveData`, (2) thêm `: ISavableCollection`, (3) implement 2 method. Không sửa logic có sẵn.

### Pattern A — Collection với deep copy trong ExportSaveData

**Example:** `AchievementTrackerCollection` từ Demo

**Location:** `Assets/Foundation/DataManager/Demo/Collections/AchievementTrackerCollection.cs`

```csharp
// CẢNH BÁO: Không đổi tên class này sau khi có save file
[Serializable]
public class AchievementTrackerSave : SaveData
{
    public List<AchievementData> achievements = new List<AchievementData>();
    public int totalUnlocked;
    public long lastUnlockTimestamp;
}

[CreateAssetMenu(menuName = "DataAsset/Demo/AchievementTracker")]
public class AchievementTrackerCollection : ScriptableObject, ISavableCollection
{
    public string CollectionKey => "achievementTracker";

    public List<AchievementData> achievements = new List<AchievementData>
    {
        new AchievementData { achievementId = "first_win",   displayName = "First Victory", targetProgress = 1   },
        new AchievementData { achievementId = "kills_10",    displayName = "Warrior",       targetProgress = 10  },
        new AchievementData { achievementId = "kills_100",   displayName = "Veteran",       targetProgress = 100 },
        new AchievementData { achievementId = "stage_clear", displayName = "Explorer",      targetProgress = 3   },
        new AchievementData { achievementId = "all_items",   displayName = "Collector",     targetProgress = 5   },
    };

    public int totalUnlocked;
    public long lastUnlockTimestamp;

    public SaveData ExportSaveData()
    {
        // Deep copy — tránh shared reference khi dirty achievements sau export
        var copy = achievements.ConvertAll(a => new AchievementData
        {
            achievementId      = a.achievementId,
            displayName        = a.displayName,
            isUnlocked         = a.isUnlocked,
            currentProgress    = a.currentProgress,
            targetProgress     = a.targetProgress,
            unlockedTimestamp  = a.unlockedTimestamp,
        });
        return new AchievementTrackerSave
        {
            achievements        = copy,
            totalUnlocked       = totalUnlocked,
            lastUnlockTimestamp = lastUnlockTimestamp,
        };
    }

    public void ImportSaveData(SaveData data)
    {
        if (data is AchievementTrackerSave save)
        {
            if (save.achievements != null) achievements = save.achievements;
            totalUnlocked = save.totalUnlocked;
            lastUnlockTimestamp = save.lastUnlockTimestamp;
        }
    }

    public void UnlockAll()
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        foreach (var ach in achievements)
        {
            if (!ach.isUnlocked)
            {
                ach.isUnlocked = true;
                ach.currentProgress = ach.targetProgress;
                ach.unlockedTimestamp = now;
                totalUnlocked++;
            }
        }
        lastUnlockTimestamp = now;
    }

    public void ResetData()
    {
        foreach (var ach in achievements)
        {
            ach.isUnlocked = false;
            ach.currentProgress = 0;
            ach.unlockedTimestamp = 0;
        }
        totalUnlocked = 0;
        lastUnlockTimestamp = 0;
    }
}
```

### Pattern B — Collection đơn giản với direct copy

**Example:** `PlayerInventoryCollection` từ Demo

**Location:** `Assets/Foundation/DataManager/Demo/Collections/PlayerInventoryCollection.cs`

```csharp
// CẢNH BÁO: Không đổi tên class này sau khi có save file
[Serializable]
public class PlayerInventorySave : SaveData
{
    public List<InventoryItemData> items = new List<InventoryItemData>();
}

[CreateAssetMenu(menuName = "DataAsset/Demo/PlayerInventory")]
public class PlayerInventoryCollection : ScriptableObject, ISavableCollection
{
    public string CollectionKey => "playerInventory";

    [Header("Default items (set in asset)")]
    public List<InventoryItemData> items = new List<InventoryItemData>
    {
        new InventoryItemData { itemId = "sword_01",     count = 1, isOwned = false },
        new InventoryItemData { itemId = "potion_small", count = 5, isOwned = true  },
        new InventoryItemData { itemId = "shield_01",    count = 1, isOwned = false },
        new InventoryItemData { itemId = "bow_01",       count = 1, isOwned = false },
        new InventoryItemData { itemId = "potion_large", count = 0, isOwned = false },
    };

    public SaveData ExportSaveData()
    {
        // Deep copy — tránh shared reference khi dirty items sau export
        var copy = items.ConvertAll(i => new InventoryItemData
        {
            itemId  = i.itemId,
            count   = i.count,
            isOwned = i.isOwned,
        });
        return new PlayerInventorySave { items = copy };
    }

    public void ImportSaveData(SaveData data)
    {
        if (data is PlayerInventorySave save && save.items != null)
            items = save.items;
    }

    public void UnlockAll()
    {
        foreach (var item in items)
        {
            item.isOwned = true;
            if (item.count == 0) item.count = 1;
        }
    }

    public void ResetData()
    {
        foreach (var item in items)
        {
            item.isOwned = false;
            item.count = 0;
        }
    }
}
```

### Pattern C — ScriptableObject thuần (không kế thừa ItemDatas)

```csharp
[Serializable]
public class CustomCollectionSave : SaveData
{
    public List<CustomItem> items = new List<CustomItem>();
    public int someOtherValue;
}

public class CustomDatas : ScriptableObject, ISavableCollection
{
    public List<CustomItem> list;

    public string CollectionKey => "custom";

    public SaveData ExportSaveData()
        => new CustomCollectionSave { items = list };

    public void ImportSaveData(SaveData data)
    {
        if (data is CustomCollectionSave save && save.items != null)
            list = save.items;
    }

    public void UnlockAll() { }
    public void ResetData() { list?.Clear(); }
}
```

### Pattern D — Nested/hierarchical data

```csharp
[Serializable]
public class StageCollectionSave : SaveData
{
    public List<StageData> items = new List<StageData>();
}

public class StageDatas : ItemDatas<Stage>, ISavableCollection
{
    public string CollectionKey => "stages";

    public SaveData ExportSaveData()
        => new StageCollectionSave { items = saveDataList };

    public void ImportSaveData(SaveData data)
    {
        if (data is StageCollectionSave save && save.items != null)
            UpdateFromSaveData(save.items);
    }
}
```

### Các Data class mẫu

**AchievementData:**
```csharp
[Serializable]
public class AchievementData
{
    public string achievementId;
    public string displayName;
    public bool isUnlocked;
    public int currentProgress;
    public int targetProgress;
    public long unlockedTimestamp;
}
```

**InventoryItemData:**
```csharp
[Serializable]
public class InventoryItemData
{
    public string itemId;
    public int count;
    public bool isOwned;
}
```

### Quy tắc

| Quy tắc | Lý do |
|---|---|
| `CollectionKey` phải unique | Trùng key → overwrite save data |
| `XxxSave` phải `[Serializable]` | `BinaryFormatter` yêu cầu |
| Item classes trong `XxxSave` phải `[Serializable]` | `BinaryFormatter` serialize đệ quy toàn bộ graph |
| `ImportSaveData()` dùng `is` pattern | Type-safe cast, null-safe |
| **Không đổi tên `XxxSave` sau release** | Binary lưu `AssemblyQualifiedName` — đổi tên → save cũ không load được |
| **Deep copy trong ExportSaveData()** | Tránh shared reference khi modify data sau export |

---

## 6. Thêm collection mới

**3 bước  không sửa DataManager/GameData:**

**1.** Tạo ScriptableObject implement `ISavableCollection`:

```csharp
// Đặt tên class cẩn thận  không đổi sau khi release
[Serializable]
public class NewItemsSave : SaveData
{
    public List<NewItem> items = new List<NewItem>();
}

[CreateAssetMenu(menuName = "DataAsset/NewDatas")]
public class NewDatas : ScriptableObject, ISavableCollection
{
    public List<NewItem> list;

    public string CollectionKey => "newItems";

    public SaveData ExportSaveData()
        => new NewItemsSave { items = list };

    public void ImportSaveData(SaveData data)
    {
        if (data is NewItemsSave save && save.items != null)
            list = save.items;
    }

    public void UnlockAll() { }
    public void ResetData() { list?.Clear(); }
}
```

**2.** Tạo asset: `Create > DataAsset > NewDatas`

**3.** Kéo vào DataManager Inspector  `dataCollections`, set key = `"newItems"`

---

## 7. Thứ tự triển khai

| # | Task | Ghi chú |
|---|---|---|
| 1 | Tạo `SaveData.cs` | Abstract base class |
| 2 | Tạo `ISavableCollection.cs` | `SaveData` signature |
| 3 | Tạo `CollectionDataSave.cs` | `CollectionDataSave` + `DataCollectionEntry` |
| 4 | Tạo/sửa `GameData` class | `SetCollection(key, SaveData)` |
| 5 | Tạo/sửa `DataManager` class | Binary persistence, loop Save/Load |
| 6 | Implement `ISavableCollection` trên từng `*Datas.cs` | Define `XxxSave : SaveData` + 2 method |
| 7 | Kiểm tra `[Serializable]` trên toàn bộ data classes | Xem checklist bên dưới |
| 8 | Setup Inspector: kéo SO vào `dataCollections` list | |
| 9 | Test save/load cycle | Play → save → quit → play → data đúng |

### Checklist `[Serializable]`

`BinaryFormatter` yêu cầu **toàn bộ object graph** có `[Serializable]`:

```
GameData                [Serializable] ✓
 ├─ GameConfig          [Serializable] ✓ phải check
 ├─ UserData            [Serializable] ✓ phải check
 └─ CollectionDataSave  [Serializable] ✓
      ├─ string key     (built-in)
      └─ SaveData       [Serializable] ✓
           └─ XxxSave   [Serializable] ✓
                └─ List<XxxItem>
                     └─ XxxItem      [Serializable] ✓ phải check
```

Nếu thiếu `[Serializable]` trên bất kỳ class nào → `SerializationException` lúc runtime.

---

## 8. DataManagerEditor (Optional Enhancement)

**Location:** `Assets/Foundation/DataManager/Editor/DataManagerEditor.cs`

Custom Inspector với UI nâng cao cho DataManager, bao gồm:

### Features

| Feature | Mô tả |
|---|---|
| **Status Overview** | Badge hiển thị số lượng collections, trạng thái loaded/loading |
| **View Modes** | 4 chế độ xem: All Sections, Default Only, Runtime Only, Side by Side |
| **ReorderableList** | Drag-and-drop collections list với add/remove buttons |
| **Validation** | Hiển thị warning cho empty key, duplicate key, null source, invalid interface |
| **Color-coded Headers** | Mỗi collection có màu riêng, dễ phân biệt |
| **Foldout Sections** | GameConfig, UserData, và từng collection có thể collapse/expand |
| **Side-by-Side Compare** | Xem default data và runtime data cạnh nhau |
| **Live Update** | Auto-repaint trong Play mode |

### Screenshots (Conceptual)

```
┌─────────────────────────────────────────────────────────────┐
│ ⚙ DataManager Inspector                                     │
├─────────────────────────────────────────────────────────────┤
│ 📚 Collections: 5     ● LOADED                              │
├─────────────────────────────────────────────────────────────┤
│ View Mode: [All Sections] [Default Only] [Runtime Only] [S→]│
├─────────────────────────────────────────────────────────────┤
│ ⚙ GameConfig (Default)                                      │
│   bundleVersion: 1.0.0                                      │
│   forceVersion: 0.0.0                                       │
│   ...                                                        │
├─────────────────────────────────────────────────────────────┤
│ 👤 UserData (Default)                                       │
│   name: YOU                                                 │
│   totalScore: 0                                             │
│   ...                                                        │
├─────────────────────────────────────────────────────────────┤
│ 📚 Collections                                              │
│   key: playerInventory  ○ PlayerInventoryCollection         │
│   key: achievements     ○ AchievementTrackerCollection      │
│   ...                                                        │
├─────────────────────────────────────────────────────────────┤
│ ⚠ Validation                                                │
│   ✓ All collections valid                                   │
└─────────────────────────────────────────────────────────────┘
```

### Key Implementation Highlights

```csharp
#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;

[CustomEditor(typeof(DataManager))]
public class DataManagerEditor : Editor
{
    private ReorderableList _collectionsList;
    private int _viewMode = 0; // 0=All, 1=Default, 2=Runtime, 3=SideBySide
    
    private void OnEnable()
    {
        InitializeReorderableList();
    }
    
    private void InitializeReorderableList()
    {
        var listProp = serializedObject.FindProperty("dataCollections");
        _collectionsList = new ReorderableList(serializedObject, listProp, true, true, true, true)
        {
            drawHeaderCallback = rect => EditorGUI.LabelField(rect, "📚 Collections", EditorStyles.boldLabel),
            drawElementCallback = DrawCollectionElement,
            onAddCallback = OnAddCollection,
            onRemoveCallback = OnRemoveCollection,
        };
    }
    
    public override void OnInspectorGUI()
    {
        DrawMainHeader();
        DrawStatusOverview();
        
        if (Application.isPlaying)
            DrawViewModeSelector();
            
        DrawGameConfigSection();
        DrawUserDataSection();
        DrawCollectionsSection();
        DrawValidationSection();
    }
}
#endif
```

### Validation Rules

| Validation | MessageType | Condition |
|---|---|---|
| Empty key | Error | `string.IsNullOrEmpty(entry.key)` |
| Duplicate key | Error | Multiple entries with same key |
| Null source | Warning | `entry.source == null` |
| Invalid interface | Error | `entry.source is not ISavableCollection` |

### Usage

1. Inspector tự động hiển thị khi select DataManager GameObject
2. Edit mode: Xem và chỉnh sửa default data
3. Play mode: Chuyển view mode để so sánh default vs runtime
4. Validation tự động chạy, hiển thị lỗi ngay trong Inspector

---

## 9. Demo Collections

Trong `Assets/Foundation/DataManager/Demo/Collections/`, có các collection mẫu hoàn chỉnh:

| Collection | Description | Key | Features |
|---|---|---|---|
| `AchievementTrackerCollection` | Achievement system với progress tracking | `achievementTracker` | Progress bars, unlock timestamps, total count |
| `PlayerInventoryCollection` | Inventory items với count và ownership | `playerInventory` | Item ownership, quantity tracking |
| `StageProgressCollection` | Stage progression với stars và unlock | `stageProgress` | **Hierarchical:** Stages → Levels, star ratings |
| `ShopCollection` | Shop items với price và purchase status | `shopCollection` | Currency, purchase history, featured items, rarity/category |
| `SettingsDataCollection` | Game settings (volume, quality, etc.) | `settingsData` | Audio, display, gameplay preferences, custom entries |

### Collection Highlights

**StageProgressCollection** — Hierarchical data example:
```csharp
[Serializable]
public class StageProgressSave : SaveData
{
    public List<StageData> stages;
}

[Serializable]
public class StageData
{
    public string stageId;
    public string stageName;
    public bool isUnlocked;
    public List<LevelData> levels;  // Nested structure
}

[Serializable]
public class LevelData
{
    public int levelIndex;
    public bool isUnlocked;
    public bool isCompleted;
    public int bestStarRating;
    public float bestCompletionTime;
}
```

**ShopCollection** — Enums + metadata:
```csharp
[Serializable]
public class ShopItemData
{
    public string itemId;
    public string displayName;
    public ItemRarity rarity;      // Enum
    public ItemCategory category;  // Enum
    public int price;
    public bool isPurchased;
    public bool isEquipped;
    public bool isFeatured;
}
```

**SettingsDataCollection** — Simple flat structure:
```csharp
[Serializable]
public class SettingsDataSave : SaveData
{
    public float masterVolume;
    public float musicVolume;
    public float sfxVolume;
    public int targetFrameRate;
    public bool isFullscreen;
    public int qualityLevel;
    public bool isVibrationEnabled;
    public bool isNotificationsEnabled;
    public string languageCode;
    public List<SettingsEntryData> customEntries;  // Key-value overflow
}
```

### Test Runners

Demo cũng bao gồm các test runner để verify tính năng:

| Test | Purpose | Location |
|---|---|---|
| `ComprehensiveDataManagerTest` | Test đầy đủ save/load cycle với tất cả collections | `Demo/ComprehensiveDataManagerTest.cs` |
| `SaveLoadTestRunner` | Test basic save/load | `Demo/SaveLoadTestRunner.cs` |
| `PersistenceTestRunner` | Test persistence qua sessions | `Demo/PersistenceTestRunner.cs` |

### Usage Example

```csharp
// Access collection từ code
var inventory = DataManager.GetCollection<PlayerInventoryCollection>("playerInventory");
var item = inventory.items.Find(i => i.itemId == "sword_01");
item.isOwned = true;
item.count++;

// Save changes
DataManager.Save();

// Access từ static shortcut (optional, tự thêm vào DataManager)
// var achievements = DataManager.Achievements;
```

---

## 10. Best Practices

### Deep Copy trong ExportSaveData()

✅ **ĐÚNG:**
```csharp
public SaveData ExportSaveData()
{
    var copy = items.ConvertAll(i => new InventoryItemData
    {
        itemId  = i.itemId,
        count   = i.count,
        isOwned = i.isOwned,
    });
    return new PlayerInventorySave { items = copy };
}
```

❌ **SAI:**
```csharp
public SaveData ExportSaveData()
{
    // Shared reference — modify items sau export sẽ ảnh hưởng save data
    return new PlayerInventorySave { items = items };
}
```

### Naming Convention

| Element | Convention | Example |
|---|---|---|
| SaveData class | `{Collection}Save` | `AchievementTrackerSave` |
| Collection class | `{Feature}Collection` | `AchievementTrackerCollection` |
| CollectionKey | camelCase | `achievementTracker` |
| Data item class | `{Feature}Data` | `AchievementData` |
| Asset menu path | `DataAsset/{Category}/{Name}` | `DataAsset/Demo/AchievementTracker` |

### Version Control

- **.gitignore:** Add `savedata.bin` vào gitignore
- **Save file location:** `Application.persistentDataPath/savedata.bin`
- **Editor testing:** Dùng `ResetData()` button trong Inspector để xoá save nhanh

### Migration Strategy

Khi thay đổi data structure (thêm/xoá field):

1. **Backward compatible:** Thêm field mới với default value
2. **Breaking change:** Bump version trong GameConfig, detect old save và migrate hoặc reset
3. **Alternative:** Dùng custom `ISerializationBinder` để handle type renaming

---

## 11. Troubleshooting

| Issue | Cause | Solution |
|---|---|---|
| `SerializationException` | Missing `[Serializable]` | Add `[Serializable]` to all data classes |
| Save file corrupted | Changed class structure | Use `ResetData()` hoặc delete save file |
| Collection not found | Key mismatch | Check `CollectionKey` matches entry key |
| Data not persisting | `Save()` not called | Call `DataManager.Save()` after modify |
| Null runtime instance | Source not set | Assign ScriptableObject in Inspector |
| Duplicate key warning | Same key in multiple entries | Make all keys unique |

---

## 12. Summary

**DataManager Collection System** cung cấp:

✅ **Flexible:** Thêm/xoá collection qua Inspector, không sửa code  
✅ **Type-safe:** `BinaryFormatter` polymorphism, không cần wrapper  
✅ **Scalable:** Mỗi collection tự quản lý save/load logic  
✅ **Editor-friendly:** Custom Inspector với validation và live preview  
✅ **Demo-rich:** 5 collection mẫu + test runners để tham khảo  

**Architecture:** Core infrastructure → GameData → DataManager → ISavableCollection implementations → Demo collections