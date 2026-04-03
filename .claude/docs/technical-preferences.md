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

## Mobile Game Development Patterns

When targeting mobile (iOS/Android), apply these additional constraints:

### Performance
- Frame time budget: 33.33ms (30 FPS) — use `Application.targetFrameRate = 30`
- Memory budget: < 512 MB total; < 200 MB for assets in memory
- Draw calls: < 500 per frame — use GPU instancing, SRP Batcher, and atlas textures
- Texture sizes: max 2048×2048; prefer compressed formats (ASTC for mobile)
- Use `half` precision in shaders wherever possible

### Mobile-Specific Patterns
- **Touch Input Abstraction** — wrap touch/pointer input behind an interface for cross-platform support
- **Asset Streaming** — use Addressables with remote catalogs for on-demand asset loading
- **Battery-Aware Design** — reduce update frequency for background systems; use `OnApplicationPause()`
- **Adaptive Quality** — implement quality tier switching at runtime based on device capability
- **Offline-First Architecture** — cache data locally; sync when network is available

### Mobile Anti-Patterns to Avoid
- No `Update()` on idle UI screens — disable components when not visible
- No unbounded particle systems — always set `maxParticles`
- No real-time shadows on low-end devices — bake where possible
- No `Camera.main` in `Update()` — cache the reference
- Avoid `Instantiate`/`Destroy` loops — use Object Pooling exclusively
