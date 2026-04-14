---
applyTo:
  - ".claude/skills/new-project/**/*"
  - ".claude/skills/new-project/templates/**/*.cs"
  - ".claude/skills/new-project/templates/**/*.asmdef"
---

# `/new-project` Skill — Rules & Conventions

> Skill tự động tạo Unity project với GameFoundation tích hợp sẵn.  
> Reference: `.claude/skills/new-project/SKILL.md`

## Skill Overview

`/new-project` orchestrates a 10-phase bootstrap flow:

| Phase | Action |
|-------|--------|
| 1 | Detect local Unity installs (`detect-unity.sh`) |
| 2 | Collect input: name (PascalCase), path, Unity version, pipeline |
| 3 | Create Unity project via `Unity.exe -batchmode -quit -createProject` |
| 3.5 | Copy `.claude/`, `.github/`, `docs/`, `CLAUDE.md` into new project root |
| 4 | Patch `Packages/manifest.json` (URP/HDRP + InputSystem + Addressables + TMP + Cinemachine + Unity MCP) |
| 5 | Copy GameFoundation templates → `Assets/Scripts/Core/GameFoundation/` |
| 5.5 | Copy `start-packs/*.unitypackage` → `Assets/ImportQueue/` |
| 6 | Copy scene UI scripts → `Assets/Scripts/UI/` + `Assets/Scripts/Gameplay/` |
| 7 | `ProjectScaffolder.cs` auto-runs on first Unity open (scenes + TMP + start-packs) |
| 8 | Run `/setup-engine` for VERSION.md + .gitignore |
| 9 | Unity MCP validation: compile check → GameplayScene smoke test → auto-fix |
| 10 | Report summary |

## Template Token

All template files use `{{PROJECT_NAMESPACE}}` as a placeholder.

**Replace rules:**
- Replace in file content: `.cs`, `.asmdef` files
- Replace in file names: `{{PROJECT_NAMESPACE}}.*.asmdef`
- Value: project name from Phase 2 (PascalCase, e.g. `MyNewGame`)

```bash
# Example: MyNewGame.GameFoundation.Data.asmdef
sed -i 's/{{PROJECT_NAMESPACE}}/MyNewGame/g' file.cs
```

## Start-Packs (Auto-Import)

Phase 5.5 copies Unity packages from `.claude/skills/new-project/start-packs/` into project's `Assets/ImportQueue/`.
Phase 7's `ProjectScaffolder.cs` auto-imports them on first Unity Editor launch.

**Currently bundled:**
- **DOTween Pro** — Default tweening library for gameplay & UGUI animations
  - Gameplay: object movement, camera effects, VFX (see gameplay-code.instructions.md)
  - UGUI: panel transitions, fade, scale, color (see ui-code.instructions.md)
  - NOT for physics-based movement or continuous Update() logic

**Adding new start-packs:**
1. Drop `.unitypackage` file into `.claude/skills/new-project/start-packs/`
2. ProjectScaffolder will auto-detect and import on next `/new-project` run
3. Update this doc and Phase 10 report with package name & purpose

## Template Directory Structure

```
templates/Assets/
├── Editor/
│   ├── ProjectScaffolder.cs            ← [InitializeOnLoad] first-run scaffolder
│   └── {{PROJECT_NAMESPACE}}.EditorTools.asmdef
├── Scripts/
│   ├── Core/GameFoundation/
│   │   ├── Bootstrap/Bootstrap.cs
│   │   ├── DataManager/{SaveData,ISavableCollection,DataManager,...}.cs
│   │   ├── GameState/{GameState,GameStateManager}.cs
│   │   └── UI/{UILayer,IUIAnim,UIBase,UISceneBase,UIPopupBase,...}.cs
│   ├── UI/
│   │   ├── Scenes/{UILoadingScene,UIMainScene,UIGameplayScene}.cs
│   │   └── Popups/PopupSettings.cs
│   └── Gameplay/GameplayController.cs
```

## GameState Flow (enforced by templates)

```
AppStart → None → Main (Bootstrap) → Init (user click Play)
         → Ready (GameplayController.InitGame done) → Play
```

**Bootstrap.cs** — goes directly to `Main`, never calls `Init`:
```csharp
// None → Main (load MainScene → GameStateManager.Main())
// Init is triggered by user action, NOT by Bootstrap
```

**UIMainScene.cs** — Play button triggers `Init`:
```csharp
GameStateManager.Init();
SceneManager.LoadScene(gameplaySceneName);
```

**GameplayController.cs** — responds to `Init` (with CurrentState fallback):
```csharp
protected virtual void OnEnable()
{
    GameStateManager.OnGameStateChanged += HandleStateChanged;
    // Scene loads AFTER Init fires — check CurrentState on enable
    if (GameStateManager.CurrentState == GameState.Init)
        StartCoroutine(InitGame());
}

protected virtual IEnumerator InitGame()
{
    yield return null; // game-specific setup goes here
    GameStateManager.Ready();
}
```

## ProjectScaffolder Responsibilities

On first domain reload after Unity opens the project:
1. Creates `LoadingScene`, `MainScene`, `GameplayScene` programmatically
2. Wires `[SerializeField] private` references via reflection
3. Registers all three scenes in `EditorBuildSettings.scenes`
4. Imports TMP Essential Resources (`TMP_PackageUtilities.ImportProjectResourcesMenu`)
5. Imports all `.unitypackage` files from `Assets/ImportQueue/`

**Re-run menu**: `{{PROJECT_NAMESPACE}}/Rebuild Starter Scenes`

## Start-Packs

Pre-built `.unitypackage` files in `start-packs/`:
- `DOTween Pro.unitypackage` — tweening library

To add a new start-pack: drop `.unitypackage` into `start-packs/`. It will be automatically:
1. Copied to `Assets/ImportQueue/` during Phase 5.5
2. Imported by `ProjectScaffolder.ImportStartPacks()` on first Unity open

## Manifest Packages

| Package | ID | Version |
|---------|----|---------|
| URP | `com.unity.render-pipelines.universal` | latest |
| HDRP | `com.unity.render-pipelines.high-definition` | latest |
| Input System | `com.unity.inputsystem` | latest |
| Addressables | `com.unity.addressables` | latest |
| TextMeshPro | `com.unity.textmeshpro` | latest |
| Cinemachine | `com.unity.cinemachine` | latest |
| Unity MCP | `com.unity.ai.assistant` | latest *(Unity 6+ only)* |

## Validation (Phase 9)

After user opens project in Unity Hub:
1. `Unity_GetConsoleLogs` — check for compile errors
2. Fix errors in BOTH project file AND template file
3. `Unity_ManageScene` → open `GameplayScene`
4. `Unity_ManageEditor` → enter Play mode
5. After 2.5s: `Unity_ManageGameObject` → read `StateLabel.text` → expect `"PLAY"`
6. `Unity_ManageEditor` → exit Play mode

## Rules for Template Modifications

- ✅ Modify template files in `.claude/skills/new-project/templates/`
- ✅ Use `{{PROJECT_NAMESPACE}}` for all namespace/class/menu references
- ✅ Keep templates minimal — game-specific code goes in the product repo
- ✅ `GameplayController` is `protected virtual` — games subclass and override `InitGame()`
- ❌ KHÔNG hardcode project names in templates
- ❌ KHÔNG add game-specific logic to GameFoundation core templates
- ❌ KHÔNG modify `ProjectScaffolder` without testing that scenes build correctly

## Documentation References

- [SKILL.md](.claude/skills/new-project/SKILL.md) — Full 10-phase workflow
- [GameFoundation README](.claude/docs/frameworks/gamefoundation/README.md) — Framework overview
- [GameState Simplified](.claude/docs/frameworks/gamefoundation/gamestate-simplified.md) — State flow diagram
- [Unity MCP Skill](.claude/skills/unity-mcp/SKILL.md) — MCP setup and tools
