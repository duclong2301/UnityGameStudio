---
name: new-project
description: "Creates a new Unity project with GameFoundation framework pre-integrated. Prompts for project name, storage path, and Unity version (detected from local installs), then scaffolds LoadingScene/MainScene/GameplayScene with Ready→Play flow."
argument-hint: "[optional: project name]"
user-invocable: true
allowed-tools: Read, Glob, Grep, Write, Edit, Bash, Task, AskUserQuestion
---

When this skill is invoked, orchestrate the full GameFoundation project bootstrap flow.

## Phase 1 — Detect local Unity installs

Invoke the `unity-local-controller` skill to detect all installed Unity versions:

```bash
/unity-local-controller
```

This presents an interactive menu of detected installations. The skill scans (in order):
1. `E:\unity\<version>\Editor\Unity.exe` (custom location detected on this machine)
2. `C:\Program Files\Unity\Hub\Editor\<version>\Editor\Unity.exe` (Unity Hub default)
3. Any path listed in `%APPDATA%\UnityHub\secondaryInstallPath.json` (secondary installations)

When invoked, the skill displays:
- Unity version (e.g., `2023.2.20f1`)
- Installation path
- Install type (Custom / Hub Default / Secondary)

The detection results are used in Phase 2 to present version options to the user via AskUserQuestion.

## Phase 2 — Collect user input (use AskUserQuestion)

Ask sequentially:
1. **Project name** — PascalCase (e.g. `MyNewGame`). Validate: `^[A-Z][A-Za-z0-9]+$`.
2. **Storage path** — absolute path (e.g. `E:\Projects\MyNewGame`). Verify parent exists, target does not.
3. **Unity version** — present the detected versions from Phase 1 as options.
4. **Render pipeline** — URP (recommended) / HDRP. Passed to `/setup-engine`.

If any answer is invalid, ask again before continuing.

## Phase 3 — Create the Unity project

```bash
"<UnityEditorPath>" -batchmode -quit -createProject "<StoragePath>" -logFile -
```

Wait for exit code 0. Verify `<StoragePath>/ProjectSettings/ProjectVersion.txt` exists. If create fails, stop and surface the log.

## Phase 3.5 — Copy studio configuration into the new project

Copy the studio's `.claude/`, `.github/`, and `docs/` directories from the current working directory (Unity Game Studio AI repo root) into `<StoragePath>/`, at the same level as the `Assets/` folder Unity just created.

```bash
cp -r ".claude"  "<StoragePath>/.claude"
cp -r ".github"  "<StoragePath>/.github"
cp -r "docs"     "<StoragePath>/docs"
```

Also copy `CLAUDE.md` to the project root:

```bash
cp "CLAUDE.md" "<StoragePath>/CLAUDE.md"
```

> **Why**: Every new product starts with the full set of Claude Code skills, agent definitions, hooks, coding standards, GitHub Copilot instructions, and architecture docs already in place. The team can run `/sprint-plan`, `/code-review`, and all other agent skills from day one without manual setup.

## Phase 4 — Patch `Packages/manifest.json`

Merge these dependencies into `<StoragePath>/Packages/manifest.json` (use `Read` + `Edit`):
- `com.unity.render-pipelines.universal` (if URP)
- `com.unity.render-pipelines.high-definition` (if HDRP)
- `com.unity.inputsystem`
- `com.unity.addressables`
- `com.unity.textmeshpro`
- `com.unity.cinemachine`
- `com.unity.ai.assistant` — **Unity MCP bridge** (Unity 6 / version ≥ 6000.0 only; skip for 2022.x)

Use `"latest"` for the version (Unity resolves on first open) or the concrete version from a known-good lock file.

> **Unity MCP note**: `com.unity.ai.assistant` installs the MCP server inside the Unity Editor.
> After the project is first opened in Unity Hub, the user can go to
> `Edit > Project Settings > AI > Unity MCP` → click **Start** to activate the bridge.
> Claude Code then communicates with Unity via the relay binary at `%USERPROFILE%\.unity\relay\relay_win.exe`.

## Phase 5 — Copy GameFoundation code templates

Copy every file under `.claude/skills/new-project/templates/Assets/` into `<StoragePath>/Assets/`, preserving directory structure. Replace the token `{{PROJECT_NAMESPACE}}` inside every `.cs` and `.asmdef` file with the project name from Phase 2.

Files produced under `Assets/Scripts/Core/GameFoundation/`:
- `DataManager/` — `SaveData.cs`, `ISavableCollection.cs`, `CollectionDataSave.cs`, `GameData.cs`, `GameConfig.cs`, `UserData.cs`, `DataManager.cs`
- `GameState/` — `GameState.cs`, `GameStateManager.cs`
- `UI/` — `UILayer.cs`, `IUIAnim.cs`, `UIBase.cs`, `UISceneBase.cs`, `UIPopupBase.cs`, `UILoadingBase.cs`, `UIManager.cs`
- `Bootstrap/` — `Bootstrap.cs`, `BootstrapConfig.cs`, `BootstrapContext.cs`, `BootstrapPipeline.cs`, `IBootstrapModule.cs`
- Each folder gets its own `.asmdef`.

> The `Bootstrap` system is module-driven: `BootstrapConfig` is a serialized
> list of `ModuleProvider` entries (prefab or ScriptableObject modules) plus
> `firstSceneName`, `minLoadingTime`, and retry options. `BootstrapPipeline`
> runs `IBootstrapModule.InitializeAsync` in priority order; each module
> first calls `FindObjectOfType<T>()` and falls back to instantiating from a
> prefab field if not found (singleton-safe). See
> `Assets/GameFoundation/Bootstrap/SINGLETON_MANAGERS.md` for the pattern.

## Phase 5.5 — Copy start-packs to ImportQueue

Copy every `.unitypackage` file from `.claude/skills/new-project/start-packs/` into `<StoragePath>/Assets/ImportQueue/` (create the folder if needed). Use `cp` with quoted paths to handle filenames with spaces.

```bash
mkdir -p "<StoragePath>/Assets/ImportQueue"
cp ".claude/skills/new-project/start-packs/"*.unitypackage "<StoragePath>/Assets/ImportQueue/"
```

These packages are auto-imported by `ProjectScaffolder` on the first domain reload when Unity opens the project (see Phase 7). Currently bundled:
- **DOTween Pro** — tweening library

## Phase 6 — Copy scene-specific UI scripts

Copy `templates/Assets/Scripts/UI/` containing:
- `Scenes/UILoadingScene.cs`, `UIMainScene.cs`, `UIGameplayScene.cs`
- `Popups/PopupSettings.cs`
- `Gameplay/GameplayController.cs`

## Phase 7 — Scene scaffolding (on first Editor launch)

Scenes are NOT authored as raw `.unity` YAML by the skill — shipping YAML with correct MonoScript GUIDs is fragile. Instead, `Assets/Editor/ProjectScaffolder.cs` (copied in Phase 5) uses `[InitializeOnLoad]` + `EditorPrefs` marker to run once on the first domain reload after Unity opens the project. It programmatically:

**Scene creation** (hierarchies verified against the live project via Unity MCP):

`Assets/Scenes/LoadingScene.unity`
```
EventSystem                           (EventSystem + StandaloneInputModule)
LoadingCanvas                         (Canvas ScreenSpaceOverlay, 1920×1080, match 0.5)
└── UILoadingScene                    (RectTransform + UILoadingScene)
    ├── Background                    (Image, color ≈ 0.05,0.05,0.1)
    └── LoadingLabel                  (TextMeshProUGUI "Loading...", size 48)
[SystemBootstrap]                     (Bootstrap — drives BootstrapPipeline)
```
> `[UIManager]` and `DataManager` are **not** authored into the scene by
> the scaffolder — they are registered as modules in `BootstrapConfig`
> (`[UIManager]` from `Assets/GameFoundation/UI/[UIManager].prefab`,
> `DataManager` from a scene instance or prefab). The user can still add
> `DataManager` to LoadingScene manually and Bootstrap will reuse it
> (`FindObjectOfType` → `useExistingInstance = true`).

`Assets/Scenes/MainScene.unity`
```
EventSystem
MainCanvas
└── UIMainScene                       (RectTransform + UIMainScene)
    ├── Background                    (Image, color ≈ 0.15,0.15,0.2)
    ├── Title                         (TMP "MAIN MENU", size 72)
    ├── PlayButton                    (Image+Button, wired via reflection)
    └── SettingsButton                (Image+Button, wired via reflection)
```

`Assets/Scenes/GameplayScene.unity`
```
EventSystem
GameplayCanvas
└── UIGameplayScene                   (RectTransform + UIGameplayScene)
    ├── Background                    (Image, color ≈ 0.1,0.2,0.1)
    └── StateLabel                    (TMP "READY", size 120, wired via reflection)
[GameplayController]                  (GameplayController)
```

Then registers all three in `EditorBuildSettings.scenes` in the correct order
(Loading → Main → Gameplay).

**TextMeshPro Essential Resources import:**
After scene creation, imports TMP Essential Resources (fonts, shaders, atlases) via:
```csharp
EditorApplication.ExecuteMenuItem("Window/TextMeshPro/Import TMP Essential Resources");
```
This ensures TMP components work correctly without manual import.

**Start-pack import:**
After TMP import, scans `Assets/ImportQueue/` for `.unitypackage` files and calls:
```csharp
AssetDatabase.ImportPackage(pkgPath, false);  // false = non-interactive
```
for each package found. This auto-imports DOTween Pro (and any future start-packs) silently.

A menu `{{PROJECT_NAMESPACE}}/Rebuild Starter Scenes` lets the user re-run scene scaffolding if needed.

## Phase 8 — Run `/setup-engine`

Execute from `<StoragePath>` directory: `/setup-engine unity <version>` to produce `docs/engine-reference/unity/VERSION.md`, the Unity `.gitignore`, and the standard folder structure (`src/`, `design/`, `tests/`, `production/`).

## Phase 9 — Unity MCP Validation

After setup, validate the full flow using Unity MCP. This requires Unity to have the project open.

**Step 1 — Wait for project to open**
Prompt the user:
> "Please open `<StoragePath>` in Unity Hub and wait for the initial package import to complete (may take 5–15 minutes). When Unity finishes importing and shows no progress bar, let me know."

Once the user confirms, check MCP connectivity by calling `Unity_GetConsoleLogs`. If MCP is not yet connected, guide the user:
- `Edit > Project Settings > AI > Unity MCP` → click **Start**
- Claude Code MCP config must include the relay binary path

**Step 2 — Compile error check**
Call `Unity_GetConsoleLogs` (or `Unity_ReadConsole`) with error filter. For each compilation error:
1. Identify the file from the error path
2. Read the file at `<StoragePath>/Assets/...`
3. Fix the error in place
4. Apply the same fix to the corresponding template at `.claude/skills/new-project/templates/Assets/...`
5. Wait for recompile (poll `Unity_GetConsoleLogs` until errors clear)

Repeat until 0 compile errors. Log each fix: "Fixed `<file>`: `<description>`".

**Step 3 — GameplayScene smoke test**
Use `Unity_ManageScene` to open `Assets/Scenes/GameplayScene.unity` (most self-contained — GameplayController auto-runs Ready→Play without UI interaction).

Enter Play mode via `Unity_ManageEditor`.

After 2.5 seconds, use `Unity_ManageGameObject` to find the `StateLabel` GameObject and read its `text` property:
- Expected: `"PLAY"` (GameplayController waits 1s then transitions)
- If still `"READY"`: wait 1 more second and retry once
- If missing or blank: check console for runtime errors, fix `GameplayController.cs` or `UIGameplayScene.cs`

Exit Play mode via `Unity_ManageEditor`.

**Step 4 — Full flow test (optional, if Step 3 passes)**
Use `Unity_ManageScene` to open `Assets/Scenes/LoadingScene.unity`.
Enter Play mode. After 3 seconds, verify active scene is `MainScene` (Bootstrap auto-transitions).
Use `Unity_RunCommand` to invoke `GameStateManager.Ready()` and load `GameplayScene` (simulates Play button click):
```csharp
// Run in Unity_RunCommand
GameStateManager.Ready();
UnityEngine.SceneManagement.SceneManager.LoadScene("GameplayScene");
```
After 2.5 seconds, verify `StateLabel.text == "PLAY"`.
Exit Play mode.

**Auto-fix protocol:**
When fixing a file:
1. Fix `<StoragePath>/Assets/<path>` first (project file)
2. Fix `.claude/skills/new-project/templates/Assets/<path>` (template — prevents same bug in future runs)
3. Log the fix to the summary report

## Phase 10 — Report

Print a summary:
```
✅ Project "{Name}" created at {path}
   Unity: {version}
   Pipeline: {URP|HDRP}
   Packages: URP/HDRP + InputSystem + Addressables + TextMeshPro + Cinemachine + Unity MCP
   Start-packs imported: DOTween Pro
   Validation: ✅ GameplayScene READY→PLAY confirmed

Next steps:
  1. Open {path} with Unity Hub (if not already open)
  2. Wait for first-time import (downloads packages, generates Library/)
  3. Enable Unity MCP: Edit > Project Settings > AI > Unity MCP → Start
  4. Open Assets/Scenes/LoadingScene.unity
  5. Press Play — watch: Loading → Main → (click Play) → Gameplay (READY → 1s → PLAY)
  6. Wire up Canvas/Button/Text references in the Inspector if any are missing
```

Remind the user:
- Git is NOT initialized automatically. Run `git init` when ready.
- Settings popup is a placeholder (shows/hides only).
- GameFoundation templates are minimal; extend by adding `ISavableCollection` implementations for your data.
- Unity MCP relay binary is at `%USERPROFILE%\.unity\relay\relay_win.exe` — configure in Claude Code's MCP settings if not already connected.

## Collaboration protocol

- Before any Write/Edit call, ask the user "May I write/patch {file}?" per project collaboration rules.
- If any phase fails, stop and surface the error. Do not proceed with partial scaffold.
- For Phase 5/6 bulk copy, show a single summary ("I will copy 18 files under Assets/…, proceed?") rather than per-file prompts.

## Delegation

- `engine-programmer` — validate GameFoundation code templates match the architecture docs before copying.
- `unity-specialist` — validate `manifest.json` patch and package versions against the selected Unity version.
- `ui-programmer` — validate scene scripts + scene wiring.
- `producer` — coordinate the flow if anything is blocked.
