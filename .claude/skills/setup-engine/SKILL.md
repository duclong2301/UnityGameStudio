---
name: setup-engine
description: "Configures the Unity game studio environment for a specific Unity version and project. Creates the VERSION.md, validates project settings, and sets up the standard project structure."
argument-hint: "unity [version] (e.g., 'unity 2022.3.45f1')"
user-invocable: true
allowed-tools: Read, Glob, Grep, Write, Edit, Bash
---

When this skill is invoked with `unity [version]`:

1. **Extract the Unity version** from the argument.

2. **Ask for project configuration**:
   - "Render Pipeline: URP (recommended) or HDRP?"
   - "Primary target platform? (PC / Mobile / Console / Multi-platform)"
   - "Multiplayer? (None / Local Co-op / Online)"
   - "Game genre/type? (helps configure packages)"

3. **Create `docs/engine-reference/unity/VERSION.md`**:

```markdown
# Unity Version Reference

## Unity Version
[Version from argument]

## Render Pipeline
[URP / HDRP]

## Target Platforms
[List of platforms]

## Key Packages
| Package | Version | Purpose |
|---|---|---|
| com.unity.inputsystem | Latest | New Input System |
| com.unity.addressables | Latest | Asset management |
| com.unity.render-pipelines.universal | Latest | URP rendering |
| com.unity.textmeshpro | Latest | Text rendering |
| com.unity.cinemachine | Latest | Camera systems |
[Add multiplayer packages if applicable]

## Performance Budgets
| Metric | PC | [Other platform] |
|---|---|---|
| Frame time | 16.67ms (60fps) | |
| Draw calls | < 2000 | |
| Memory | < 2GB | |
| GC/frame (gameplay) | 0 bytes | |

## Scripting
- Backend: IL2CPP (builds), Mono (editor)
- C# version: 9.0+
- Nullable: enabled

## Build Profiles
- Development: Mono, profiler enabled, development build flag
- Staging: IL2CPP, no development flag, internal distribution
- Release: IL2CPP, code stripping High, no debug symbols
```

4. **Create `.gitignore`** if not present (Unity-specific):

```gitignore
/[Ll]ibrary/
/[Tt]emp/
/[Oo]bj/
/[Bb]uild/
/[Bb]uilds/
/[Ll]ogs/
/[Uu]ser[Ss]ettings/
/[Mm]emoryCaptures/
*.csproj
*.unityproj
*.sln
*.suo
*.tmp
*.user
*.userprefs
*.pidb
*.booproj
*.svd
*.pdb
*.opendb
*.VC.db
/Assets/Plugins/Editor/JetBrains*
.vs/
.idea/
.vscode/
```

5. **Confirm completion**: "Unity [version] environment configured. Ready to start building? Run `/start` or `/sprint-plan` next."
