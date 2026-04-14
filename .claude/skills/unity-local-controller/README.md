# Unity Local Controller

Comprehensive Unity Editor installation manager for Windows.

## Features

- **Auto-detect** all Unity installations from multiple sources
- **Validate** specific Unity versions
- **Launch** Unity Editor with or without project
- **Get paths** for build automation and CI/CD
- **Manage** Unity Hub configuration

## Quick Start

```
/unity-local-controller list
```

See all installed Unity versions with interactive menu.

```
/unity-local-controller validate 2022.3.45f1
```

Check if a specific version is installed.

```
/unity-local-controller path 2022.3.45f1
```

Get the absolute path to Unity.exe (for automation).

```
/unity-local-controller launch 2022.3.45f1 E:\Projects\MyGame
```

Launch Unity Editor with a project.

## Files

- **SKILL.md** — Main skill documentation and procedures
- **scripts/detect-unity.ps1** — PowerShell detection script (Windows-native)
- **scripts/detect-unity.sh** — Bash detection script (Git Bash compatible)
- **scripts/launch-unity.ps1** — Unity launcher with validation
- **references/usage-examples.md** — Detailed usage examples and troubleshooting

## Detection Sources

The skill scans multiple locations to find Unity installations:

1. **Custom install location**: `E:\unity` (detected on this machine)
2. **Unity Hub default**: `C:\Program Files\Unity\Hub\Editor`
3. **Secondary install paths**: From `%APPDATA%\UnityHub\secondaryInstallPath.json`
4. **Hub registry**: From `%APPDATA%\UnityHub\editors.json` (Unity Hub 3.x)

## Script Usage

### PowerShell (recommended on Windows)
```powershell
.\.claude\skills\unity-local-controller\scripts\detect-unity.ps1
```

### Bash (Git Bash)
```bash
bash .claude/skills/unity-local-controller/scripts/detect-unity.sh
```

### Launch Unity
```powershell
.\.claude\skills\unity-local-controller\scripts\launch-unity.ps1 -Version "2022.3.45f1"
```

## Integration

This skill can be called by other skills:

- **new-project**: Detect Unity versions for project creation
- **setup-engine**: Validate specified Unity version exists
- **Build automation**: Get Unity.exe path for CI/CD

## Testing

Current machine has these Unity versions installed:
- 2022.3.62f3 (E:\unity\2022.3.62f3\Editor\Unity.exe)
- 6000.2.6f2 (E:\unity\6000.2.6f2\Editor\Unity.exe)  
- 6000.3.4f1 (E:\unity\6000.3.4f1\Editor\Unity.exe)

## See Also

- [Usage Examples & Troubleshooting](./references/usage-examples.md)
- [SKILL.md](./SKILL.md) — Full documentation
