# Unity MCP Troubleshooting Reference

Source: Unity MCP 2.0.0-pre.1 (`com.unity.ai.assistant`)

---

## Bridge won't start (shows Stopped)

**Symptoms:** `AI > Unity MCP` settings page shows Stopped, doesn't auto-start.

**Causes:**
- Script compilation errors in the project
- Incomplete package installation
- Bridge was explicitly stopped

**Fixes:**
1. Open Unity Console → fix all compilation errors
2. `Edit > Project Settings > AI > Unity MCP` → click **Start**
3. Verify `com.unity.ai.assistant` is installed: `Window > Package Manager`
4. Restart Unity Editor if the above don't work

---

## MCP client can't connect to Unity

**Symptoms:** Client reports connection errors or can't find Unity tools.

**Causes:**
- Unity not running, or bridge stopped
- Relay binary missing from `~/.unity/relay/`
- Client config points to wrong executable path
- `--mcp` flag missing from args

**Fixes:**
1. Confirm bridge is running: `Project Settings > AI > Unity MCP` → green Running status
2. Verify relay binary exists at the platform path:
   - macOS ARM: `~/.unity/relay/relay_mac_arm64.app/Contents/MacOS/relay_mac_arm64`
   - macOS x64: `~/.unity/relay/relay_mac_x64.app/Contents/MacOS/relay_mac_x64`
   - Windows: `%USERPROFILE%\.unity\relay\relay_win.exe`
   - Linux: `~/.unity/relay/relay_linux`
3. Check client config includes `"args": ["--mcp"]`
4. Use `Integrations` section to reconfigure the client automatically
5. Restart both Unity and the MCP client

---

## Client connects but can't invoke any tools (Pending Connection)

**Symptoms:** Client connects but tools are inaccessible.

**Cause:** Direct connections require user approval. The connection is awaiting approval.

**Fix:**
1. `Edit > Project Settings > AI > Unity MCP`
2. In **Pending Connections**, review client details
3. Click **Accept**

> Once approved, the client is remembered for future sessions.
> AI Gateway (built-in Unity Assistant) connections are auto-approved.

---

## Tools (0) shown — client connects but no tools appear

**Symptoms:** Client shows zero tools, or custom tools are missing.

**Causes:**
- `[McpTool]` attribute missing or incorrectly applied
- Tool methods not `public static`
- Class-based tools missing parameterless constructor
- Class-based tools not implementing `IUnityMcpTool` / `IUnityMcpTool<T>`
- Tool is in an assembly not scanned by TypeCache
- Tool disabled in Unity MCP settings
- Compilation errors in tool scripts

**Fixes:**
1. Check Unity Console for compilation errors in tool scripts
2. Verify tool methods are `public static` and have `[McpTool]`
3. For class-based tools: confirm they implement `IUnityMcpTool` or `IUnityMcpTool<T>` and have a parameterless constructor
4. Check `Tools` section in Unity MCP settings — ensure tools are enabled
5. Enable **Show Debug Logs** in Unity MCP settings for detailed discovery info

---

## Relay binary missing

**Symptoms:** Client configurations fail because the relay binary isn't found.

**Cause:** The ServerInstaller (runs at editor startup) was interrupted or the package directory is inaccessible.

**Fix:** Restart Unity Editor — the installer runs automatically on load.

---

## Unity can't find MCP server executable (PATH issue)

**Symptoms:** MCP server starts in terminal but fails from Unity Editor.

**Cause:** Unity's PATH differs from your terminal's PATH.

**Fix:**
1. `Edit > Project Settings > AI > Unity MCP > Path Configuration`
2. Compare **User Path** vs **Path Accessible by Unity**
3. Add missing directories to **User Path**
4. Click **Refresh Config File** and **Reload Servers**

---

## Tools (0) from external MCP server (MCP Client page)

**Symptoms:** A server added to `AI > MCP Servers` (the client page) shows connected but Tools (0).

**Diagnosis:**
1. Install MCP Inspector and validate the server outside Unity
2. `AI > MCP Servers` → select the server → **Inspect** → review status, messages, and tool manifest

**If tools don't appear in MCP Inspector:** Fix the server installation/config first.
**If tools appear in MCP Inspector but Unity shows Tools (0):** File a Unity bug report with:
- MCP configuration file
- Server logs
- MCP Inspector output

---

## Incorrect tool execution / wrong output

**Symptoms:** Tool runs but produces wrong or incomplete results.

**Fixes:**
1. In Assistant conversation, expand **tool call details** to review tool name, arguments, and output
2. `AI > MCP Servers` → select server → **Inspect** → check tool manifest for required parameters and types
3. Update prompt with explicit arguments the tool expects
4. Validate tool behavior in MCP Inspector with the same parameters

---

## Debug Tips

- Enable **Show Debug Logs** in `Project Settings > AI > Unity MCP`
- Call `UnityMCPBridge.PrintToolSchemas()` to print/copy all registered tool schemas
- Call `UnityMCPBridge.PrintConnectedClients()` to see all currently connected clients
- Use MCP Inspector to validate any MCP server outside of Unity before configuring it in Unity
