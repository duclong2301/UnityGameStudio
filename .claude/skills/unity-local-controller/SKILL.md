---
name: unity-local-controller
description: 'Detects, validates, and manages Unity Editor installations on the local machine. Use for: finding Unity versions, validating Unity paths, launching Unity Editor, checking Unity Hub configuration, getting installation paths for project creation or build automation.'
argument-hint: '[list | validate <version> | launch <version> <project-path> | path <version>]'
user-invocable: true
allowed-tools: Read, Glob, Grep, Bash, AskUserQuestion
---

# Unity Local Controller

Manages Unity Editor installations on Windows machines. Detects all installed Unity versions, validates specific versions, launches editors, and integrates with Unity Hub configuration.

## When to Use

- **Before creating a new Unity project** — verify Unity version availability
- **Build automation** — get the correct Unity.exe path for command-line builds
- **Multi-version development** — switch between Unity versions
- **Troubleshooting** — validate Unity installation paths
- **Quick launch** — open Unity Editor from command-line workflow

## Invocation Flow

When this skill is invoked, parse the argument to determine the operation:

| Argument Pattern | Operation | Example |
|---|---|---|
| (empty) or `list` | List all installations | `/unity-local-controller` |
| `validate <version>` | Validate specific version | `/unity-local-controller validate 2022.3.45f1` |
| `path <version>` | Get Unity.exe path | `/unity-local-controller path 2022.3.45f1` |
| `launch <version>` | Launch Unity | `/unity-local-controller launch 2022.3.45f1` |
| `launch <version> <path>` | Launch with project | `/unity-local-controller launch 2022.3.45f1 E:\Projects\MyGame` |
| `hub-config` | Show Hub configuration | `/unity-local-controller hub-config` |

## Common Procedure: Unity Detection

All operations start by running the detection script. Use PowerShell on Windows:

```powershell
$results = .\.claude\skills\unity-local-controller\scripts\detect-unity.ps1
```

**Parse results** — Each line is tab-separated: `<version>\t<path>\t<install type>`

**Deduplicate** — Multiple install types may point to the same physical installation (e.g., "Custom" and "Secondary" both referencing `E:\unity`). Deduplicate by path, preferring the first occurrence.

```powershell
$installations = $results | ForEach-Object {
    $parts = $_ -split "`t"
    [PSCustomObject]@{
        Version = $parts[0]
        Path = $parts[1]
        InstallType = $parts[2]
    }
} | Group-Object Path | ForEach-Object { $_.Group[0] } | Sort-Object Version -Descending
```

## Operations

### 1. List All Installations (Default)

When called with no arguments or `list`, scans all known Unity installation locations and presents an interactive selection menu.

**Detection locations (in order):**
1. `E:\unity\<version>\Editor\Unity.exe` (detected custom install on this machine)
2. `C:\Program Files\Unity\Hub\Editor\<version>\Editor\Unity.exe` (Unity Hub default)
3. Secondary install paths from `%APPDATA%\UnityHub\secondaryInstallPath.json`

**Procedure:**
1. Run detection script: `bash ./scripts/detect-unity.sh`
2. Parse output (format: `<version>\t<path>`)
3. Present interactive menu with:
   - Unity version (e.g., `2022.3.45f1`)
   - Installation path
   - Install type (Hub Default / Custom / Secondary)
   - Available actions (Launch | Validate | Copy Path | Open Installation Folder)

**Example output:**
```
╔══════════════════════════════════════════════════════════════════════════════╗
║                         Unity Installations Detected                         ║
╚══════════════════════════════════════════════════════════════════════════════╝

[1] 2023.2.20f1
    📁 E:\unity\2023.2.20f1\Editor\Unity.exe
    ℹ️  Custom Install
    
[2] 2022.3.45f1
    📁 C:\Program Files\Unity\Hub\Editor\2022.3.45f1\Editor\Unity.exe
    ℹ️  Unity Hub Default

[3] 6000.0.28f1
    📁 E:\unity\6000.0.28f1\Editor\Unity.exe
    ℹ️  Custom Install

Select an installation [1-3], or press Enter to continue:
```

If user selects an installation, present actions:
```
Selected: Unity 2023.2.20f1

Actions:
[L] Launch Editor (new empty project)
[P] Launch with Project... (specify path)
[C] Copy path to clipboard
[V] Validate installation
[F] Open installation folder in Explorer
[Q] Cancel

Choose action:
```

### 2. Validate Specific Version

**Usage:** `validate <version>` (e.g., `validate 2022.3.45f1`)

**Procedure:**
1. Run detection script
2. Search for exact version match
3. If found:
   - ✅ Confirm path exists
   - ✅ Verify `Unity.exe` is executable
   - ✅ Display installation details
4. If not found:
   - ❌ List closest matches (by major.minor version)
   - Suggest installing via Unity Hub

**Example output (found):**
```
✅ Unity 2022.3.45f1 is installed

Path: E:\unity\2022.3.45f1\Editor\Unity.exe
Type: Custom Install
Size: 3.2 GB
Executable: Verified

Ready to use for project creation or builds.
```

**Example output (not found):**
```
❌ Unity 2022.3.45f1 not found

🔍 Similar versions installed:
   • 2022.3.40f1 (E:\unity\2022.3.40f1\Editor\Unity.exe)
   • 2023.2.20f1 (E:\unity\2023.2.20f1\Editor\Unity.exe)

💡 To install Unity 2022.3.45f1:
   1. Open Unity Hub
   2. Go to Installs → Add
   3. Select version 2022.3.45f1
   4. Run this skill again to verify
```

### 3. Get Path to Specific Version

**Usage:** `path <version>` (e.g., `path 2022.3.45f1`)

**Procedure:**
1. Run detection script
2. Search for exact version match
3. Output ONLY the absolute path (for scripting/automation)

**Example output:**
```
E:\unity\2022.3.45f1\Editor\Unity.exe
```

**Use case:** Build scripts, CI/CD pipelines, project creation automation.

```bash
UNITY_EXE=$(/unity-local-controller path 2022.3.45f1)
"$UNITY_EXE" -batchmode -quit -executeMethod BuildScript.BuildWindows
```

### 4. Launch Unity Editor

**Usage:** 
- `launch <version>` — Opens Unity Hub project selector for that version
- `launch <version> <project-path>` — Opens specific project

**Procedure:**
1. Validate version exists (reuse validation procedure)
2. If project path provided:
   - Verify path exists
   - Verify it contains `Assets/` and `ProjectSettings/`
   - Launch with `-projectPath` argument
3. If no project path:
   - Launch Unity Hub with version pre-selected
   - Or launch Editor directly (shows project selector)

**Example (no project):**
```bash
"E:\unity\2022.3.45f1\Editor\Unity.exe"
```

**Example (with project):**
```bash
"E:\unity\2022.3.45f1\Editor\Unity.exe" -projectPath "E:\Projects\MyGame"
```

### 5. Manage Unity Hub Configuration

**Usage:** `hub-config`

**Procedure:**
1. Read `%APPDATA%\UnityHub\secondaryInstallPath.json`
2. Display current Hub preferences:
   - Default installation path
   - Secondary installation path
   - Auto-update preferences (if available)
3. Offer to update secondary install path

**Example output:**
```
Unity Hub Configuration
=======================

Default Install Path:
  C:\Program Files\Unity\Hub\Editor

Secondary Install Path:
  E:\unity

Detected Editors: 17
Latest Installed: 6000.0.28f1

[U] Update secondary install path
[R] Refresh detection
[Q] Cancel
```

## Integration with Other Skills

This skill can be called by other skills as a helper:

**From `new-project` skill:**
```markdown
1. Detect Unity versions: invoke `unity-local-controller list` → store results
2. Present versions to user during project creation
3. Use selected version path for `-createProject`
```

**From `setup-engine` skill:**
```markdown
1. Validate user-specified Unity version: invoke `unity-local-controller validate <version>`
2. If not found, show installation instructions
```

**From build automation:**
```bash
UNITY_PATH=$(unity-local-controller path 2022.3.45f1)
if [[ -z "$UNITY_PATH" ]]; then
    echo "Unity 2022.3.45f1 required but not installed"
    exit 1
fi
```

## Scripts Reference

- [detect-unity.sh](./scripts/detect-unity.sh) — Bash script for scanning Unity installations
- [detect-unity.ps1](./scripts/detect-unity.ps1) — PowerShell alternative (Windows-native)
- [launch-unity.ps1](./scripts/launch-unity.ps1) — Launch Unity with validation and error handling

## Error Handling

| Error | Action |
|-------|--------|
| No Unity installations found | Show installation guide, link to Unity Hub download |
| Version not found | List similar versions, suggest Unity Hub installation |
| Path invalid | Validate path format, suggest re-running detection |
| Unity.exe not executable | Check permissions, suggest reinstalling |
| Project path invalid | Verify Assets/ and ProjectSettings/ exist |

## Output Modes

**Interactive mode** (default): Menu-driven interface with colored output
**Script mode** (`path` command): Plain text output for automation
**Validation mode** (`validate` command): Checkmark/cross with details

## Notes

- Detection is **read-only** — never modifies Unity installations or Hub config
- Uses native Windows paths (e.g., `E:\unity\...`, not `/e/unity`) in output
- Caches detection results during a single skill invocation (no redundant scans)
- Compatible with Unity Hub 3.x and Unity 6+ installation patterns
