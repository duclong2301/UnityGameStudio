---
applyTo:
  - "Assets/Data/GameFoundation/**/*.asset"
  - "Assets/Data/Collections/**/*.asset"
  - "Assets/Data/GameConfig.asset"
  - "Assets/Scripts/**/Collections/**/*.cs"
---

# GameFoundation Data & ScriptableObjects Rules

> Quy tắc tạo và quản lý data collections, ScriptableObjects cho GameFoundation framework.  
> Reference: `.claude/docs/frameworks/gamefoundation/README.md`

## ScriptableObject Organization

### Directory Structure

```
Assets/Data/
├── GameFoundation/
│   ├── GameConfig.asset             ← Singleton game configuration
│   └── Collections/
│       ├── PlayerInventory.asset    ← Runtime data collections
│       ├── ProgressTracker.asset
│       ├── AchievementTracker.asset
│       └── ShopData.asset
```

### Naming Conventions

| Asset Type | Pattern | Example |
|------------|---------|---------|
| GameConfig | `GameConfig.asset` | `GameConfig.asset` (only one) |
| Collection | `[Name].asset` | `PlayerInventory.asset` |
| Collection Script | `[Name]Collection.cs` | `PlayerInventoryCollection.cs` |
| Save Data Script | `[Name]Save.cs` | `InventorySave.cs` |

## Collection Implementation Pattern

### Step 1: Define Save Data Class

```csharp
// Assets/Scripts/Core/GameFoundation/DataManager/Collections/InventorySave.cs
using System;
using System.Collections.Generic;

[Serializable]
public class InventorySave : SaveData
{
    public List<ItemData> items;
    public int gold;
    public int gems;
    
    // Default constructor for deserialization
    public InventorySave() {
        items = new List<ItemData>();
    }
}

// Item data must be [Serializable]
[Serializable]
public class ItemData
{
    public string itemId;
    public int quantity;
    public int level;
}
```

### Step 2: Create Collection ScriptableObject

```csharp
// Assets/Scripts/Core/GameFoundation/DataManager/Collections/PlayerInventoryCollection.cs
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "PlayerInventory", menuName = "GameFoundation/Collections/Player Inventory")]
public class PlayerInventoryCollection : ScriptableObject, ISavableCollection
{
    // ─── Runtime Data (NOT serialized to .asset file) ───
    private List<ItemData> _items = new List<ItemData>();
    private int _gold;
    private int _gems;
    
    // ─── Public API ───
    public IReadOnlyList<ItemData> Items => _items;
    public int Gold => _gold;
    public int Gems => _gems;
    
    public void AddItem(string itemId, int quantity) {
        var item = _items.Find(i => i.itemId == itemId);
        if (item != null) {
            item.quantity += quantity;
        } else {
            _items.Add(new ItemData {
                itemId = itemId,
                quantity = quantity,
                level = 1
            });
        }
    }
    
    public void AddGold(int amount) => _gold += amount;
    public void AddGems(int amount) => _gems += amount;
    
    // ─── ISavableCollection Implementation ───
    public SaveData ToSaveData() {
        return new InventorySave {
            items = new List<ItemData>(_items), // Deep copy
            gold = _gold,
            gems = _gems
        };
    }
    
    public void FromSaveData(SaveData data) {
        if (data is InventorySave save) {
            _items = new List<ItemData>(save.items);
            _gold = save.gold;
            _gems = save.gems;
        } else {
            Debug.LogError($"Invalid save data type: {data?.GetType()}");
        }
    }
    
    public void ResetData() {
        _items.Clear();
        _gold = 0;
        _gems = 0;
    }
}
```

### Step 3: Create ScriptableObject Asset

1. Right-click in `Assets/Data/GameFoundation/Collections/`
2. Create → GameFoundation → Collections → Player Inventory
3. Name it `PlayerInventory.asset`

### Step 4: Register in DataManager

1. Select `DataManager` GameObject in scene (usually in DontDestroyOnLoad)
2. In Inspector, find `Data Collection Entries` list
3. Add new entry:
   - **Key**: `"player_inventory"` (lowercase + underscore)
   - **Collection**: Drag `PlayerInventory.asset`

## Data Best Practices

### Collection Design

✅ **DO:**
- Store runtime game state (inventory, progress, stats)
- Use private fields (`_items`) for data, public properties for access
- Deep copy collections in `ToSaveData()` — prevent reference sharing
- Implement `ResetData()` for clean state (new game, logout)
- Validate data in `FromSaveData()` — check type, handle nulls

❌ **DON'T:**
- Store configuration/balance data — use separate config ScriptableObjects
- Serialize Unity object references (`GameObject`, `Transform`)
- Store cross-collection references — each collection is independent
- Modify data outside collection methods — encapsulation

### Data Validation

```csharp
public void FromSaveData(SaveData data) {
    if (data is InventorySave save) {
        // ✅ Validate loaded data
        _items = save.items ?? new List<ItemData>();
        _gold = Mathf.Max(0, save.gold); // No negative gold
        _gems = Mathf.Max(0, save.gems);
        
        // ✅ Remove invalid items
        _items.RemoveAll(item => string.IsNullOrEmpty(item.itemId));
    }
}
```

### Serializable Data Rules

```csharp
// ✅ ĐÚNG: Serializable class với [Serializable]
[Serializable]
public class ItemData {
    public string itemId;
    public int quantity;
}

// ✅ ĐÚNG: List, Dictionary serializable types
[Serializable]
public class InventorySave : SaveData {
    public List<ItemData> items;
    public Dictionary<string, int> resources; // BinaryFormatter supports
}

// ❌ SAI: Unity objects không serialize được
[Serializable]
public class InvalidSave : SaveData {
    public GameObject prefab;      // ❌ GameObject reference
    public Transform playerTrans;  // ❌ Component reference
}

// ❌ SAI: Không có [Serializable]
public class InvalidData {  // ❌ Missing attribute
    public int value;
}
```

## GameConfig Pattern

GameConfig lưu configuration/balance data (KHÔNG thay đổi runtime):

```csharp
// Assets/Scripts/Core/GameFoundation/DataManager/Core/GameConfig.cs
using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "GameFoundation/Game Config")]
public class GameConfig : ScriptableObject
{
    [Header("Economy")]
    public int startingGold = 1000;
    public int startingGems = 100;
    
    [Header("Gameplay")]
    public float playerSpeed = 5f;
    public int maxHealth = 100;
    
    [Header("Levels")]
    public LevelConfig[] levels;
    
    // ✅ Config is READONLY at runtime
    // ❌ KHÔNG modify GameConfig values trong code
}

[System.Serializable]
public class LevelConfig
{
    public string levelId;
    public int requiredLevel;
    public int goldReward;
}
```

**GameConfig KHÔNG implement `ISavableCollection`** — nó không cần save/load.

## Collection Registration

### DataManager Inspector Setup

```
DataManager (GameObject)
├── Game Config: [GameConfig.asset]          ← Drag config asset
├── User Data: [Auto-generated]              ← UserData internal
└── Data Collection Entries:                 ← List of collections
    ┌─ Element 0
    │   Key: "player_inventory"
    │   Collection: [PlayerInventory.asset]
    ├─ Element 1
    │   Key: "progress_tracker"
    │   Collection: [ProgressTracker.asset]
    ├─ Element 2
    │   Key: "achievement_tracker"
    │   Collection: [AchievementTracker.asset]
    └─ Element 3
        Key: "shop_data"
        Collection: [ShopData.asset]
```

### Key Naming Rules

- ✅ `lowercase_with_underscores`
- ✅ Descriptive: `"player_inventory"`, not `"inv"`
- ✅ Unique across all collections
- ❌ KHÔNG dùng spaces: `"player inventory"` ❌
- ❌ KHÔNG PascalCase: `"PlayerInventory"` ❌
- ❌ KHÔNG duplicate keys

## Common Collection Patterns

### Progress Tracker

```csharp
[CreateAssetMenu(menuName = "GameFoundation/Collections/Progress Tracker")]
public class ProgressTrackerCollection : ScriptableObject, ISavableCollection
{
    private Dictionary<string, int> _levelProgress = new();
    private int _currentLevel;
    private int _playerXP;
    
    public int CurrentLevel => _currentLevel;
    
    public void CompleteLevel(string levelId) {
        _levelProgress[levelId] = 1; // 1 = completed
        _currentLevel++;
    }
    
    public bool IsLevelCompleted(string levelId) {
        return _levelProgress.TryGetValue(levelId, out int status) && status > 0;
    }
    
    public SaveData ToSaveData() {
        return new ProgressSave {
            levelProgress = new Dictionary<string, int>(_levelProgress),
            currentLevel = _currentLevel,
            playerXP = _playerXP
        };
    }
    
    public void FromSaveData(SaveData data) {
        if (data is ProgressSave save) {
            _levelProgress = save.levelProgress ?? new();
            _currentLevel = save.currentLevel;
            _playerXP = save.playerXP;
        }
    }
    
    public void ResetData() {
        _levelProgress.Clear();
        _currentLevel = 1;
        _playerXP = 0;
    }
}

[Serializable]
public class ProgressSave : SaveData {
    public Dictionary<string, int> levelProgress;
    public int currentLevel;
    public int playerXP;
}
```

### Achievement Tracker

```csharp
[CreateAssetMenu(menuName = "GameFoundation/Collections/Achievement Tracker")]
public class AchievementTrackerCollection : ScriptableObject, ISavableCollection
{
    private HashSet<string> _unlockedAchievements = new();
    private Dictionary<string, int> _achievementProgress = new();
    
    public void UnlockAchievement(string achievementId) {
        _unlockedAchievements.Add(achievementId);
    }
    
    public bool IsUnlocked(string achievementId) {
        return _unlockedAchievements.Contains(achievementId);
    }
    
    public void UpdateProgress(string achievementId, int progress) {
        _achievementProgress[achievementId] = progress;
    }
    
    public SaveData ToSaveData() {
        return new AchievementSave {
            unlockedIds = new List<string>(_unlockedAchievements),
            progress = new Dictionary<string, int>(_achievementProgress)
        };
    }
    
    public void FromSaveData(SaveData data) {
        if (data is AchievementSave save) {
            _unlockedAchievements = new HashSet<string>(save.unlockedIds ?? new());
            _achievementProgress = save.progress ?? new();
        }
    }
    
    public void ResetData() {
        _unlockedAchievements.Clear();
        _achievementProgress.Clear();
    }
}

[Serializable]
public class AchievementSave : SaveData {
    public List<string> unlockedIds;
    public Dictionary<string, int> progress;
}
```

### Settings Data

```csharp
[CreateAssetMenu(menuName = "GameFoundation/Collections/Settings Data")]
public class SettingsDataCollection : ScriptableObject, ISavableCollection
{
    private float _musicVolume = 1f;
    private float _sfxVolume = 1f;
    private bool _vibrationEnabled = true;
    private string _language = "en";
    
    public float MusicVolume { get => _musicVolume; set => _musicVolume = Mathf.Clamp01(value); }
    public float SFXVolume { get => _sfxVolume; set => _sfxVolume = Mathf.Clamp01(value); }
    public bool VibrationEnabled { get => _vibrationEnabled; set => _vibrationEnabled = value; }
    public string Language { get => _language; set => _language = value; }
    
    public SaveData ToSaveData() {
        return new SettingsSave {
            musicVolume = _musicVolume,
            sfxVolume = _sfxVolume,
            vibrationEnabled = _vibrationEnabled,
            language = _language
        };
    }
    
    public void FromSaveData(SaveData data) {
        if (data is SettingsSave save) {
            MusicVolume = save.musicVolume;
            SFXVolume = save.sfxVolume;
            VibrationEnabled = save.vibrationEnabled;
            Language = save.language ?? "en";
        }
    }
    
    public void ResetData() {
        _musicVolume = 1f;
        _sfxVolume = 1f;
        _vibrationEnabled = true;
        _language = "en";
    }
}

[Serializable]
public class SettingsSave : SaveData {
    public float musicVolume;
    public float sfxVolume;
    public bool vibrationEnabled;
    public string language;
}
```

## Data Migration

Khi thay đổi structure của SaveData (thêm/xóa fields):

```csharp
[Serializable]
public class InventorySave : SaveData
{
    public List<ItemData> items;
    public int gold;
    public int gems;
    public int diamonds; // ← NEW FIELD added in v2.0
    
    // Version tracking
    public int version = 2;
}

public void FromSaveData(SaveData data) {
    if (data is InventorySave save) {
        _items = save.items ?? new();
        _gold = save.gold;
        _gems = save.gems;
        
        // ✅ Handle migration
        if (save.version < 2) {
            _diamonds = 0; // Default for old saves
        } else {
            _diamonds = save.diamonds;
        }
    }
}
```

## Testing Collections

```csharp
// ✅ Test save/load round-trip
[Test]
public void TestInventorySaveLoad() {
    var collection = ScriptableObject.CreateInstance<PlayerInventoryCollection>();
    collection.AddItem("sword", 1);
    collection.AddGold(500);
    
    var saveData = collection.ToSaveData();
    collection.ResetData();
    Assert.AreEqual(0, collection.Gold);
    
    collection.FromSaveData(saveData);
    Assert.AreEqual(500, collection.Gold);
    Assert.AreEqual(1, collection.Items.Count);
}

// ✅ Test data validation
[Test]
public void TestInvalidDataHandling() {
    var collection = ScriptableObject.CreateInstance<PlayerInventoryCollection>();
    var invalidSave = new InventorySave { gold = -100 }; // Negative gold
    
    collection.FromSaveData(invalidSave);
    Assert.GreaterOrEqual(collection.Gold, 0); // Should clamp to 0
}
```

## Performance Tips

- ✅ Cache collection references — don't find by key every frame
- ✅ Use `readonly` collections (`IReadOnlyList`) để prevent external modification
- ✅ Debounce save calls — don't save every frame
- ❌ KHÔNG call `ToSaveData()` nếu data chưa thay đổi (dirty flag pattern)

```csharp
public class PlayerInventoryCollection : ScriptableObject, ISavableCollection
{
    private bool _isDirty;
    
    public void AddItem(string itemId, int quantity) {
        // ... add logic
        _isDirty = true;
    }
    
    public SaveData ToSaveData() {
        _isDirty = false;
        return new InventorySave { /* ... */ };
    }
    
    public bool NeedsSave() => _isDirty;
}
```

## Role Context

When working with GameFoundation data:
- **Game Designer** định nghĩa collections cần thiết
- **Gameplay Programmer** implement collection logic
- **Engine Programmer** đảm bảo save/load hoạt động đúng

## Documentation References

- [DataManager Architecture](.claude/docs/frameworks/gamefoundation/datamanager-architecture.md)
- [README](.claude/docs/frameworks/gamefoundation/README.md)

---

**🚨 CRITICAL:** 
- LUÔN implement `ISavableCollection` cho data collections
- LUÔN tạo concrete `SaveData` subclass riêng
- KHÔNG serialize Unity objects
- TEST save/load round-trip trước khi commit
