---
name: unity-mcp
description: >
  Guide for setting up, using, and extending Unity MCP (com.unity.ai.assistant 2.0.0-pre.1+) —
  the bridge that lets AI agents like Claude Code, Cursor, and Windsurf interact directly with
  the Unity Editor via the Model Context Protocol. Use this skill whenever the user asks about:
  connecting an AI client to Unity, Unity MCP setup or configuration, built-in Unity MCP tools
  (Unity_ManageScene, Unity_ManageGameObject, Unity_ReadConsole, etc.), registering custom MCP
  tools in C# with [McpTool], troubleshooting MCP connection issues, the relay binary, or
  using MCP to automate Unity Editor tasks (scene management, asset operations, script editing,
  running commands). Also use when the user mentions com.unity.ai.assistant, McpToolRegistry,
  IUnityMcpTool, or wants to control Unity from Claude Code / Cursor / Windsurf.
---

# Unity MCP Skill

Unity MCP is part of the `com.unity.ai.assistant` package (Unity 6+). It exposes the Unity
Editor as an MCP server so AI clients can discover and invoke Unity tools directly.

## Architecture

```
AI Client (Claude Code, Cursor, Windsurf…)
    │  MCP protocol (stdio)
Relay binary (~/.unity/relay/) --mcp flag
    │  IPC (named pipe on Windows / Unix socket on macOS/Linux)
Unity Editor → MCP Bridge → McpToolRegistry → Built-in + Custom tools
```

## Quick Setup

### Prerequisites
- Unity 6 (6000.0) or later
- `com.unity.ai.assistant` package installed
- MCP-compatible client: Claude Code, Cursor, Windsurf, or Claude Desktop

### Step 1 – Verify the bridge is running
`Edit > Project Settings > AI > Unity MCP` → status must show **Running** (green).
If Stopped → click **Start**. The relay binary auto-installs to `~/.unity/relay/` on first launch.

### Step 2 – Configure the MCP client

**Relay binary paths by platform:**

| Platform | Path |
|---|---|
| macOS Apple Silicon | `~/.unity/relay/relay_mac_arm64.app/Contents/MacOS/relay_mac_arm64` |
| macOS Intel | `~/.unity/relay/relay_mac_x64.app/Contents/MacOS/relay_mac_x64` |
| Windows | `%USERPROFILE%\.unity\relay\relay_win.exe` |
| Linux | `~/.unity/relay/relay_linux` |

**Claude Code / Cursor config** (`mcp_settings.json` or equivalent):
```json
{
  "mcpServers": {
    "unity-mcp": {
      "command": "~/.unity/relay/relay_mac_arm64.app/Contents/MacOS/relay_mac_arm64",
      "args": ["--mcp"]
    }
  }
}
```

> The `--mcp` flag is required. Use `Edit > Project Settings > AI > Unity MCP > Integrations`
> to auto-configure supported clients.

### Step 3 – Approve the connection
First connection from an external client shows under **Pending Connections** in the settings page.
Click **Accept**. Previously approved clients reconnect automatically.

> AI Gateway connections (built-in Unity Assistant) are auto-approved — no manual step needed.

### Step 4 – Test
In your client: *"Read the Unity console messages and summarize any warnings or errors."*
The client will call `Unity_ReadConsole` and return the result.

---

## Built-in Tools

Unity MCP ships with tools across these categories:

| Category | Tool examples | What they do |
|---|---|---|
| Scene management | `Unity_ManageScene` | Load/save/create scenes |
| GameObject operations | `Unity_ManageGameObject` | Create, inspect, modify GameObjects |
| Console | `Unity_ReadConsole` | Read console logs/errors/warnings |
| Asset operations | `Unity_ManageAsset` | Import, find, create assets |
| Script editing | `Unity_ManageScript` | Read/write C# scripts |
| Command execution | `Unity.RunCommand` | Compile and run arbitrary C# at edit time |

Full schema: in Unity, `UnityMCPBridge.PrintToolSchemas()` prints and copies all schemas.

---

## Registering Custom MCP Tools

For full details, see → `references/tool-registration.md`

### 4 registration methods at a glance

| Method | Use when |
|---|---|
| Static method + typed params | Simple tools, auto schema generation |
| Static method + `JObject` params | Flexible/dynamic schemas |
| Class-based (`IUnityMcpTool<T>`) | Stateful tools needing helper methods |
| Runtime API (`McpToolRegistry.RegisterTool`) | Dynamic, conditional tool registration |

### Minimal example (typed params)
```csharp
using Unity.AI.MCP.Editor.ToolRegistry;

[McpTool("my_tool", "Does something useful in Unity")]
public static object MyTool(MyParams p)
{
    return new { success = true, result = p.Name };
}

public class MyParams
{
    [McpDescription("Target name", Required = true)]
    public string Name { get; set; }
}
```

Tools are discovered automatically at editor startup via `TypeCache`. No manual registration needed.

**Rules for auto-discovery:**
- Method must be `public static`
- Decorated with `[McpTool]`
- Class-based: must implement `IUnityMcpTool<T>` or `IUnityMcpTool` + have parameterless constructor
- Must be in an assembly scanned by TypeCache (standard Editor assemblies qualify)

---

## Project Settings Reference

`Edit > Project Settings > AI > Unity MCP` controls:

- **Bridge status** + Start/Stop button
- **Connected Clients** list
- **Pending Connections** — approve/deny external clients
- **Tools** — per-tool enable/disable toggles
- **Integrations** — one-click client configuration
- **Show Debug Logs** — verbose tool discovery info in Console

> Note: `AI > MCP Servers` (different page) configures servers Unity *connects to*.
> `AI > Unity MCP` configures the server Unity *exposes*.

---

## Troubleshooting

See → `references/troubleshooting.md` for detailed fixes.

| Symptom | Quick fix |
|---|---|
| Bridge shows Stopped | Fix compilation errors → click Start → restart editor |
| Client can't connect | Check relay path, confirm `--mcp` arg, check bridge status |
| Client connects but no tools | Check Pending Connections (approve it) |
| Tools (0) shown | Check `[McpTool]` attribute, `public static`, compilation errors, tool enabled in settings |
| Relay binary missing | Restart editor — ServerInstaller runs on load |
| Wrong PATH for executables | Compare `User Path` vs `Path Accessible by Unity` in settings |

---

## API Reference (Key Types)

| Type / Member | Description |
|---|---|
| `UnityMCPBridge.Init()` | Auto-called at editor load; starts bridge if enabled |
| `UnityMCPBridge.PrintToolSchemas()` | Prints all registered tool schemas to console + clipboard |
| `McpToolRegistry.RegisterTool<T>()` | Runtime tool registration |
| `McpToolRegistry.UnregisterTool(name)` | Runtime tool removal |
| `McpToolRegistry.ToolsChanged` | Event fired when tools are added/removed/refreshed |
| `[McpTool(name, description)]` | Marks static method or class as MCP tool |
| `[McpDescription(desc)]` | Adds description + constraints to parameter properties |
| `[McpSchema(toolName)]` | Links a static method as custom input schema provider |
| `[McpOutputSchema(toolName)]` | Links a static method as custom output schema provider |
| `IUnityMcpTool<T>` | Interface for class-based stateful tools |

---

## Reference Files

- `references/tool-registration.md` — Full code examples for all 4 registration methods,
  attribute reference, and JSON schema generation details
- `references/troubleshooting.md` — Detailed troubleshooting for all common issues
