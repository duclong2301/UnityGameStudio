---
applyTo:
  - "Assets/Editor/**/*.cs"
  - ".claude/skills/**/*.md"
  - ".claude/skills/**/*.cs"
---

# Unity MCP — Integration Rules

> Unity MCP (`com.unity.ai.assistant`) exposes the Unity Editor as an MCP server so AI agents
> (Claude Code, Cursor, Windsurf) can control Unity directly via the Model Context Protocol.  
> Reference: `.claude/skills/unity-mcp/SKILL.md`

## Architecture

```
Claude Code (AI client)
    │  MCP protocol (stdio)
%USERPROFILE%\.unity\relay\relay_win.exe --mcp
    │  IPC (named pipe)
Unity Editor → MCP Bridge → McpToolRegistry → Built-in + Custom tools
```

## Setup (Unity 6+ only)

1. Package `com.unity.ai.assistant` must be in `Packages/manifest.json`
2. `Edit > Project Settings > AI > Unity MCP` → Status = **Running** (click Start if Stopped)
3. First external connection: approve under **Pending Connections**
4. Claude Code relay path: `%USERPROFILE%\.unity\relay\relay_win.exe --mcp`

> `/new-project` automatically adds `com.unity.ai.assistant` to `manifest.json` for Unity 6 projects.

## Built-in Tools Reference

| Tool | Purpose |
|------|---------|
| `Unity_ManageScene` | Load / save / create scenes |
| `Unity_ManageGameObject` | Create, inspect, modify GameObjects |
| `Unity_ManageScript` | Read / write C# scripts |
| `Unity_ManageAsset` | Import, find, create assets |
| `Unity_ManageEditor` | Control editor state (Play mode, etc.) |
| `Unity_ReadConsole` / `Unity_GetConsoleLogs` | Read console logs / errors / warnings |
| `Unity_RunCommand` | Execute arbitrary C# in edit-time context |
| `Unity_PackageManager_ExecuteAction` | Install / remove packages |
| `Unity_FindProjectAssets` | Search assets by type or name |
| `Unity_Camera_Capture` | Capture scene view screenshot |

## Custom Tool Registration

```csharp
// ✅ Minimal typed-params tool (auto schema generation)
using Unity.AI.MCP.Editor.ToolRegistry;

[McpTool("spawn_prefab", "Spawns a prefab at the specified position")]
public static object SpawnPrefab(SpawnParams p)
{
    var go = PrefabUtility.InstantiatePrefab(
        AssetDatabase.LoadAssetAtPath<GameObject>(p.PrefabPath)) as GameObject;
    go.transform.position = new Vector3(p.X, p.Y, p.Z);
    return new { success = true, name = go.name };
}

public class SpawnParams
{
    [McpDescription("Asset path to the prefab", Required = true)]
    public string PrefabPath { get; set; }
    [McpDescription("World X position")] public float X { get; set; }
    [McpDescription("World Y position")] public float Y { get; set; }
    [McpDescription("World Z position")] public float Z { get; set; }
}
```

### Rules for Custom Tools

- ✅ Method must be `public static` + `[McpTool]` attribute
- ✅ Place in Editor assembly (auto-discovered via TypeCache)
- ✅ Return anonymous object or serializable type — **not** UnityEngine objects
- ✅ Prefer typed params (`MyParams` class) over raw `JObject` for auto schema
- ❌ KHÔNG reference runtime-only APIs (MonoBehaviour methods, physics, etc.) in edit-time tools
- ❌ KHÔNG return `GameObject`, `Transform`, or other Unity objects — serialize as strings/IDs
- ❌ KHÔNG do heavy work synchronously — use `EditorApplication.delayCall` for deferred ops

## Usage Patterns in Skills

### Compile error check
```
Unity_GetConsoleLogs → filter by "error" → parse file + line → fix via Unity_ManageScript
```

### Enter / exit Play mode
```
Unity_ManageEditor { action: "enter_play_mode" }
// ... wait / check state ...
Unity_ManageEditor { action: "exit_play_mode" }
```

### Read GameObject state in Play mode
```
Unity_ManageGameObject { name: "StateLabel", action: "get_component", component: "TextMeshProUGUI" }
```

### Run C# at edit time (e.g. simulate button click)
```
Unity_RunCommand { code: "GameStateManager.Init(); SceneManager.LoadScene(\"GameplayScene\");" }
```

## Troubleshooting Quick Reference

| Symptom | Fix |
|---------|-----|
| Bridge shows Stopped | Fix compile errors → click Start → restart editor |
| `Tools (0)` in client | Approve under Pending Connections, check `[McpTool]` is `public static` |
| Relay binary missing | Restart Unity — `ServerInstaller` runs on load |
| `Unity_RunCommand` fails | Code runs in edit-time; no MonoBehaviour context |

Full troubleshooting: `.claude/skills/unity-mcp/references/troubleshooting.md`
