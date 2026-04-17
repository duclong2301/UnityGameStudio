# Unity Version Reference

## Unity Version
2022.3.x LTS (update this to your actual version)

## Render Pipeline
URP (Universal Render Pipeline)

## Target Platforms
- Android — primary
- IOS — secondary
- PC (Windows, macOS, Linux)
- (Add additional platforms here)

## Key Packages

| Package | Version | Purpose |
|---|---|---|
| com.unity.inputsystem | 1.7.x | New Input System |
| com.unity.addressables | 1.21.x | Asset management |
| com.unity.render-pipelines.universal | 14.x | URP rendering |
| com.unity.textmeshpro | 3.0.x | Text rendering |
| com.unity.cinemachine | 2.9.x | Camera systems |
| com.unity.localization | 1.4.x | Localization |

## Performance Budgets

| Metric | PC | Mobile |
|---|---|---|
| Frame time | 16.67ms (60 FPS) | 33.33ms (30 FPS) |
| CPU game logic | < 6ms | < 10ms |
| CPU rendering | < 4ms | < 8ms |
| Draw calls | < 2000 | < 500 |
| GC/frame (gameplay) | 0 bytes | 0 bytes |
| Total asset memory | < 2 GB | < 512 MB |
| Level load time | < 3s | < 5s |

## Scripting Backend
- Editor: Mono (fast iteration)
- Builds: IL2CPP (performance and security)
- C# version: 9.0+
- Nullable reference types: enabled

## Build Profiles

| Profile | Backend | Dev Build | Profiler | Strip Level |
|---|---|---|---|---|
| Development | Mono | Yes | Yes | None |
| Staging | IL2CPP | Yes | No | Medium |
| Release | IL2CPP | No | No | High |

## Git LFS Tracked Extensions
`.png`, `.jpg`, `.tga`, `.psd`, `.ai`, `.wav`, `.mp3`, `.ogg`, `.fbx`, `.obj`, `.blend`, `.pdb`
