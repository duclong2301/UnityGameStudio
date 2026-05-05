# Phase 0: Deep Scan (qua Unity MCP + Python)

## Tổng quan

Quét và phân tích sâu toàn bộ project để lập bản đồ chi tiết trước khi đụng vào bất cứ thứ gì. Output là 1 bản báo cáo JSON chứa toàn bộ thông tin cần thiết cho các phase sau.

**Rủi ro:** Không có — chỉ đọc file, không sửa gì.
**Thời gian:** 2-5 phút tùy kích thước project.
**Cần Unity mở?** Có, để chạy Unity MCP tools.

---

## Tại sao cần 2 nguồn scan?

### 1. Python scan (file system)
- **Tốc độ:** Nhanh, quét trực tiếp file
- **Độ đầy đủ:** Lấy được tất cả .cs content, .meta GUID, image dimensions
- **Dùng cho:** Phân tích code patterns, tìm substring cần regex replace

### 2. Unity MCP scan (từ Editor)
- **Chính xác:** Unity đã compile xong, biết đúng kiểu của mỗi symbol
- **Runtime info:** Biết script nào attach vào GameObject nào, sprite nào đang dùng
- **Dùng cho:** Verify compile errors, detect missing scripts, type checking

---

## Dữ liệu cần thu thập

### A. C# Symbols
```json
{
  "scripts": [
    {
      "filepath": "Assets/Scripts/Player/PlayerController.cs",
      "filename": "PlayerController.cs",
      "guid": "a1b2c3d4e5f6...",
      "namespaces": ["OldGame.Gameplay"],
      "classes": ["PlayerController", "PlayerState"],
      "methods": ["Awake", "Start", "Update", "Move", "Jump"],
      "fields": ["moveSpeed", "jumpForce", "isGrounded"],
      "serialized_fields": ["moveSpeed", "jumpForce"],
      "usings": ["UnityEngine", "OldGame.Core"]
    }
  ]
}
```

**Cần lấy:**
- ✅ Tên file, đường dẫn, GUID
- ✅ Namespace declarations
- ✅ Class/struct/enum names
- ✅ Method names (kể cả Unity lifecycle)
- ✅ Field names, property names
- ✅ Serialized fields (phân biệt public vs [SerializeField])
- ✅ Using statements
- ✅ Partial classes (cả phần 1 và phần 2)

---

### B. GUID Mapping
```json
{
  "guid_map": {
    "a1b2c3d4e5f6...": "Assets/Scripts/Player/PlayerController.cs",
    "b2c3d4e5f6a1...": "Assets/Sprites/hero_idle.png",
    "c3d4e5f6a1b2...": "Assets/Prefabs/enemy.prefab"
  },
  "duplicate_guids": []
}
```

**Cần lấy:**
- ✅ GUID từ mỗi .meta file
- ✅ Ánh xạ GUID ↔ file path
- ✅ Cảnh báo GUID trùng lặp (nếu có)
- ✅ Orphan .meta files (file .meta không có asset tương ứng)

---

### C. Sprite Inventory
```json
{
  "sprites": [
    {
      "filepath": "Assets/Sprites/hero_idle.png",
      "filename": "hero_idle.png",
      "guid": "b2c3d4e5f6a1...",
      "width": 256,
      "height": 256,
      "format": "png",
      "sprite_mode": 1,
      "color_mode": "RGBA",
      "frames": null
    },
    {
      "filepath": "Assets/Sprites/character_sheet.png",
      "filename": "character_sheet.png",
      "guid": "c3d4e5f6a1b2...",
      "width": 512,
      "height": 512,
      "format": "png",
      "sprite_mode": 2,
      "color_mode": "RGBA",
      "frames": [
        {"name": "idle_0", "rect": [0, 0, 64, 64]},
        {"name": "idle_1", "rect": [64, 0, 64, 64]},
        {"name": "walk_0", "rect": [128, 0, 64, 64]}
      ]
    }
  ]
}
```

**Cần lấy:**
- ✅ Đường dẫn, tên file, GUID, dimensions
- ✅ Format (PNG, JPG, TGA, etc.)
- ✅ Sprite mode (1=Single, 2=Multiple/sheet, 3=Polygon)
- ✅ Color mode (RGBA, RGB, etc.)
- ✅ Với sprite sheets: rect của từng frame, tên frame
- ✅ Cảnh báo: sprites dùng NineSlice, sprite atlas references

---

### D. Dependency Graph
```json
{
  "scene_references": {
    "Assets/Scenes/MainMenu.unity": [
      {
        "gameobject": "Canvas",
        "script_guid": "a1b2c3d4e5f6...",
        "script_name": "PlayerController",
        "serialized_values": {
          "moveSpeed": 5.5,
          "jumpForce": 10.0
        }
      }
    ]
  },
  "prefab_references": {
    "Assets/Prefabs/enemy.prefab": [
      {
        "script_guid": "d4e5f6a1b2c3...",
        "script_name": "EnemyAI"
      }
    ]
  },
  "sprite_usage": {
    "b2c3d4e5f6a1...": [
      {"file": "Assets/Scenes/MainMenu.unity", "component": "SpriteRenderer", "gameobject": "PlayerSprite"}
    ]
  }
}
```

**Cần lấy:**
- ✅ Mỗi scene/prefab sử dụng script nào (qua GUID)
- ✅ Serialized field values trong scenes/prefabs
- ✅ Mỗi sprite được sử dụng ở đâu

---

### E. Animation & Animator Clips
```json
{
  "animation_clips": [
    {
      "filepath": "Assets/Animations/player_idle.anim",
      "properties": ["transform.position.x", "playerSpeed", "isJumping"]
    }
  ],
  "animators": [
    {
      "filepath": "Assets/Animations/player.controller",
      "states": ["Idle", "Walk", "Jump"],
      "parameters": ["speed", "isGrounded"]
    }
  ]
}
```

**Cần lấy:**
- ✅ .anim files reference property paths nào (quan trọng cho field rename)
- ✅ Animator parameters (tên tham số)
- ✅ Animation states (có thể reference script field names)

---

### F. Assembly Definitions
```json
{
  "asmdef_files": [
    {
      "filepath": "Assets/Scripts/Runtime.asmdef",
      "name": "OldGame.Runtime",
      "rootNamespace": "OldGame.Core",
      "references": ["OldGame.Data"],
      "guid": "e5f6a1b2c3d4..."
    }
  ]
}
```

**Cần lấy:**
- ✅ .asmdef file names, paths, GUIDs
- ✅ name field (cần rename)
- ✅ rootNamespace (cần update)
- ✅ references giữa asmdefs

---

## Thực hiện Scan

### Bước 1: Chạy Python Scanner
```bash
python3 phase0_scanner.py \
  --project-path /path/to/UnityProject \
  --output scan_report.json \
  --verbose
```

**Output:** `scan_report.json` (500KB - 10MB tùy project)

### Bước 2: Chạy Unity MCP Scan
Trong Editor, tạo script sau trong `Assets/Editor/`:

```csharp
using UnityEditor;
using UnityEditor.SceneManagement;
using Unity.AI.MCP.Editor.ToolRegistry;

[InitializeOnLoad]
public class ReskinMCPScanner
{
    static ReskinMCPScanner()
    {
        EditorApplication.delayCall += OnEditorLoad;
    }

    [McpTool("reskin_scan_symbols", "Scan all C# symbols in project")]
    public static object ScanSymbols()
    {
        var types = TypeCache.GetTypesDerivedFrom<MonoBehaviour>();
        // Scan all types, serialize to JSON
        // Return: { success: true, count: N, symbols: [...] }
        return new { success = true, count = types.Count };
    }

    [McpTool("reskin_detect_missing_scripts", "Detect missing scripts in scenes")]
    public static object DetectMissingScripts()
    {
        var missing = new List<string>();
        var allGOs = Resources.FindObjectsOfTypeAll<GameObject>();
        
        foreach (var go in allGOs)
        {
            foreach (var comp in go.GetComponents<Component>())
            {
                if (comp == null)
                {
                    missing.Add($"{go.name} has missing script");
                }
            }
        }
        
        return new { success = true, missing_count = missing.Count, items = missing };
    }

    [McpTool("reskin_verify_guids", "Verify GUID integrity")]
    public static object VerifyGUIDs()
    {
        // Compare GUID from .meta vs scan_report.json
        // Return differences
        return new { success = true, guid_mismatches = 0 };
    }
}
```

### Bước 3: Review Scan Report

Mở `scan_report.json` và kiểm tra:

- [ ] Tất cả .cs files được detect
- [ ] Không có namespace, class, method bị sót
- [ ] GUID mapping hoàn chỉnh (không có GUID trùng)
- [ ] Sprite inventory chính xác (dimensions đúng)
- [ ] Dependency graph valid (không có broken references)

---

## Đầu ra Phase 0

### File: `scan_report.json`
```json
{
  "scan_timestamp": "2025-04-20T10:30:00Z",
  "project_path": "/path/to/UnityProject",
  "summary": {
    "total_scripts": 45,
    "total_sprites": 87,
    "total_scenes": 5,
    "total_prefabs": 23,
    "total_guids_tracked": 180,
    "duplicate_guids": 0,
    "orphan_meta_files": 0
  },
  "scripts": [...],
  "sprites": [...],
  "guid_map": {...},
  "scene_references": {...},
  "animation_clips": [...],
  "asmdef_files": [...],
  "warnings": [
    "SpriteRenderer in Assets/Prefabs/player.prefab references deleted sprite",
    "PlayerController.cs has 1247 lines (recommend split into multiple files)"
  ]
}
```

### File: `scan_human_review.txt`
Báo cáo dạng text dễ đọc để con người review:

```
===== RESKIN SCAN REPORT =====

Project: MyUnityGame
Scanned: 2025-04-20 10:30:00

[SUMMARY]
Scripts: 45
Sprites: 87
Scenes: 5
Prefabs: 23
Total GUIDs: 180

[CRITICAL WARNINGS]
- None

[WARNINGS]
- Orphan .meta: 0
- Duplicate GUIDs: 0
- Missing sprites: 0

[TOP 10 LARGEST SCRIPTS]
1. GameManager.cs (1247 lines) ← Consider split
2. PlayerController.cs (945 lines)
3. EnemyAI.cs (823 lines)
...

[NAMESPACES FOUND]
- OldGame
- OldGame.Core
- OldGame.UI
- OldGame.Gameplay

[CLASSES TO RENAME] (45 total)
PlayerController → HeroController
GameManager → GameController
EnemyAI → HostileNPC
...

[SERIALIZED FIELDS TO PROTECT]
Must add FormerlySerializedAs for:
- moveSpeed (in PlayerController)
- jumpForce (in PlayerController)
- enemyDamage (in EnemyAI)
...

[SPRITE SHEETS DETECTED]
- character_sheet.png (4 frames: idle_0, idle_1, walk_0, walk_1)
- ui_icons.png (16 frames)
→ These require special handling in Phase 6
```

---

## Kiểm tra hoàn tất Phase 0

- [ ] `scan_report.json` được tạo thành công
- [ ] Không có parse errors
- [ ] Tất cả GUIDs được track
- [ ] Verify: `len(guid_map) == total_guids_tracked`
- [ ] Review warnings, không có critical issues
- [ ] Backup `scan_report.json` (dùng cho verify ở Phase 7)

---

## Nếu có vấn đề

### Problem: GUIDs không match
**Solution:** Chạy `Ctrl+R` (Reimport) trong Unity, rồi scan lại

### Problem: Missing scripts được detect
**Solution:** Xem danh sách, check xem có script nào bị delete hay không

### Problem: Sprite dimensions không đọc được
**Solution:** Kiểm tra file ảnh, có thể bị corrupt. Restore từ backup hoặc re-export

---

## Next Step
Sau khi Phase 0 hoàn tất ✅, tiến hành **Phase 1: Auto-gen mapping**