# Unity Local Controller — Usage Examples & Troubleshooting

## Quick Start Examples

### List all Unity installations
```
/unity-local-controller
```
or
```
/unity-local-controller list
```

### Validate a specific version
```
/unity-local-controller validate 2022.3.45f1
```

### Get path to Unity.exe (for automation)
```
/unity-local-controller path 2022.3.45f1
```

### Launch Unity Editor
```
/unity-local-controller launch 2022.3.45f1
```

### Launch Unity with a project
```
/unity-local-controller launch 2022.3.45f1 E:\Projects\MyGame
```

### Check Unity Hub configuration
```
/unity-local-controller hub-config
```

## Command-Line Usage (PowerShell)

If you need to use the scripts directly from PowerShell (outside of the skill):

### Detect all installations
```powershell
.\.claude\skills\unity-local-controller\scripts\detect-unity.ps1
```

Output format:
```
2023.2.20f1    E:\unity\2023.2.20f1\Editor\Unity.exe    Custom
2022.3.45f1    C:\Program Files\Unity\Hub\Editor\2022.3.45f1\Editor\Unity.exe    Hub Default
```

### Launch Unity programmatically
```powershell
.\.claude\skills\unity-local-controller\scripts\launch-unity.ps1 -Version "2022.3.45f1"
```

With a project:
```powershell
.\.claude\skills\unity-local-controller\scripts\launch-unity.ps1 `
    -Version "2022.3.45f1" `
    -ProjectPath "E:\Projects\MyGame"
```

## Integration Examples

### From new-project skill
```markdown
## Phase 1 — Detect Unity versions

Invoke `/unity-local-controller list` and parse results:

$versions = .\.claude\skills\unity-local-controller\scripts\detect-unity.ps1 | ConvertFrom-Csv -Delimiter "`t" -Header "Version","Path","Type"

Present $versions to user during project setup.
```

### From CI/CD build script
```powershell
# Get Unity path for build
$unityExe = .\.claude\skills\unity-local-controller\scripts\detect-unity.ps1 | 
    Where-Object { $_ -like "2022.3.45f1*" } |
    ForEach-Object { ($_ -split "`t")[1] }

if (-not $unityExe) {
    Write-Error "Unity 2022.3.45f1 required but not installed"
    exit 1
}

# Execute build
& $unityExe -batchmode -quit -buildTarget Win64 -logFile build.log
```

### From setup-engine skill
```markdown
## Phase 1 — Validate Unity version

User specifies Unity version (e.g., 2022.3.45f1).

Invoke `/unity-local-controller validate 2022.3.45f1`.

If validation fails:
- Show installation instructions
- Offer to list installed versions
- Cancel setup until Unity is available
```

## Troubleshooting

### No Unity installations found

**Symptom:** Skill reports "No Unity installations found on this machine"

**Causes:**
1. Unity not installed via Unity Hub
2. Custom installation in non-standard location
3. Unity Hub configuration not in `%APPDATA%\UnityHub`

**Solutions:**
1. Install Unity via Unity Hub: https://unity.com/download
2. If using custom install location, update the detection script:
   - Edit `.claude/skills/unity-local-controller/scripts/detect-unity.ps1`
   - Add your custom path to the scan list:
     ```powershell
     Scan-UnityDirectory -RootPath "D:\MyUnityInstalls" -InstallType "Custom"
     ```
3. Verify Unity Hub config exists:
   ```powershell
   Test-Path "$env:APPDATA\UnityHub"
   ```

### Version detected but validation fails

**Symptom:** Skill lists a version, but `validate` says it doesn't exist

**Causes:**
1. Unity.exe was moved or deleted after detection cache
2. Permissions issue preventing file access
3. Corrupted Unity installation

**Solutions:**
1. Re-run detection to refresh cache: `/unity-local-controller list`
2. Check file permissions:
   ```powershell
   Get-Acl "E:\unity\2022.3.45f1\Editor\Unity.exe"
   ```
3. Reinstall Unity via Unity Hub

### Launch fails with "Not a valid Unity project"

**Symptom:** `launch` command reports missing `Assets/` or `ProjectSettings/`

**Causes:**
1. Wrong project path specified
2. Path points to a subdirectory of the project
3. Project was not fully initialized

**Solutions:**
1. Verify project path contains both folders:
   ```powershell
   Get-ChildItem "E:\Projects\MyGame" -Directory
   # Should show: Assets, Library, Packages, ProjectSettings
   ```
2. Use the root project folder (not Assets/ or ProjectSettings/)
3. If project is corrupt, create a new one or restore from version control

### PowerShell execution policy blocks scripts

**Symptom:** "script cannot be loaded because running scripts is disabled"

**Causes:**
PowerShell execution policy set to `Restricted` or `AllSigned`

**Solutions:**
1. **Session-only bypass** (recommended for testing):
   ```powershell
   Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
   ```
2. **Permanent change** (requires admin):
   ```powershell
   Set-ExecutionPolicy -Scope CurrentUser -ExecutionPolicy RemoteSigned
   ```
3. **One-time bypass**:
   ```powershell
   powershell -ExecutionPolicy Bypass -File ".\detect-unity.ps1"
   ```

### Git Bash shows wrong paths (e.g., `/e/unity`)

**Symptom:** Bash script returns Unix-style paths instead of Windows paths

**Causes:**
Git Bash uses MSYS path format

**Solutions:**
Use the PowerShell script instead:
```powershell
.\.claude\skills\unity-local-controller\scripts\detect-unity.ps1
```

Or convert paths in your script:
```bash
unity_path=$(cygpath -w "$bash_path")  # /e/unity → E:\unity
```

### Unity Hub secondary path not detected

**Symptom:** Installations in secondary path don't appear

**Causes:**
1. `secondaryInstallPath.json` doesn't exist
2. JSON file is malformed
3. Path in JSON is invalid

**Solutions:**
1. Check Unity Hub settings:
   - Open Unity Hub
   - Preferences → Installs → **Install Location**
   - Note the path
2. Manually verify JSON:
   ```powershell
   Get-Content "$env:APPDATA\UnityHub\secondaryInstallPath.json"
   # Should show: "E:\\unity"
   ```
3. If missing, set in Unity Hub then re-run detection

## Advanced Usage

### Custom detection locations

To add detection for non-standard Unity installation locations, edit either script:

**PowerShell** (`.ps1`):
```powershell
# Add after existing scans:
Scan-UnityDirectory -RootPath "D:\Custom\Unity\Versions" -InstallType "Custom"
```

**Bash** (`.sh`):
```bash
# Add after existing scans:
scan_dir "/d/Custom/Unity/Versions" "Custom"
```

### Parallel version detection

For faster detection across network drives or slow locations:

```powershell
$paths = @(
    "E:\unity",
    "C:\Program Files\Unity\Hub\Editor",
    "\\NetworkDrive\Unity"
)

$jobs = $paths | ForEach-Object {
    Start-Job -ScriptBlock {
        param($path)
        .\.claude\skills\unity-local-controller\scripts\detect-unity.ps1 -RootPath $path
    } -ArgumentList $_
}

$results = $jobs | Wait-Job | Receive-Job
```

### Filtering by Unity version range

Find all Unity 2022.3.x LTS versions:

```powershell
.\.claude\skills\unity-local-controller\scripts\detect-unity.ps1 |
    Where-Object { $_ -match "^2022\.3\.\d+f1" }
```

Find Unity 6 (6000.x) versions:

```powershell
.\.claude\skills\unity-local-controller\scripts\detect-unity.ps1 |
    Where-Object { $_ -match "^6000\." }
```

## Output Parsing

For automation scripts that need structured data:

```powershell
# Parse as objects
$installations = .\.claude\skills\unity-local-controller\scripts\detect-unity.ps1 | ForEach-Object {
    $parts = $_ -split "`t"
    [PSCustomObject]@{
        Version = $parts[0]
        Path = $parts[1]
        InstallType = $parts[2]
        VersionMajor = ($parts[0] -split '\.')[0]
        VersionMinor = ($parts[0] -split '\.')[1]
        IsLTS = $parts[0] -match 'f1$'
    }
}

# Filter and sort
$installations | 
    Where-Object { $_.IsLTS -and $_.VersionMajor -ge 2022 } |
    Sort-Object Version -Descending |
    Format-Table
```

## Testing

To verify the skill without running Unity:

```powershell
# Test detection only
.\.claude\skills\unity-local-controller\scripts\detect-unity.ps1

# Test validation (requires Unity installed)
.\.claude\skills\unity-local-controller\scripts\launch-unity.ps1 -Version "2022.3.45f1" -WhatIf

# Test with invalid version
.\.claude\skills\unity-local-controller\scripts\launch-unity.ps1 -Version "9999.9.9f1"
# Should show error and suggest alternatives
```
