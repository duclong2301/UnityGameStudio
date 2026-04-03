# Technical Preferences — Unity Game Studio

## Engine

- **Unity LTS** (Long Term Support) release — check [unity.com/releases/lts](https://unity.com/releases/lts) for current version
- **Render Pipeline**: URP (Universal Render Pipeline) by default; HDRP for high-end PC/console projects
- **Scripting Backend**: IL2CPP for builds; Mono for editor iteration

## Language

- **C# 10+** (Unity 2022+ supports C# 9/10 features)
- Nullable reference types enabled: `#nullable enable` in new files
- Pattern matching and records encouraged for data types

## Key Packages (Unity Package Manager)

- **Input System** (com.unity.inputsystem) — new input system, not legacy
- **Addressables** (com.unity.addressables) — asset management
- **UI Toolkit** — runtime and editor UI
- **TextMeshPro** — all text rendering
- **Cinemachine** — camera systems
- **Universal RP** (com.unity.render-pipelines.universal) — rendering

## Version Control

- Git with trunk-based development
- Unity-specific `.gitignore` (Library/, Temp/, Builds/ excluded)
- Git LFS for binary assets: `.png`, `.jpg`, `.wav`, `.mp3`, `.fbx`, `.psd`
- Commit convention: `type(scope): description` (feat, fix, docs, refactor, test, chore)

## Testing

- Unity Test Framework (Edit Mode + Play Mode)
- Test Runner accessible via Window > General > Test Runner
- CI: Unity Cloud Build or GitHub Actions with game-ci Docker image

## Build Targets

- PC (Windows/Mac/Linux) — primary development target
- Document additional platforms in `docs/engine-reference/unity/VERSION.md`
