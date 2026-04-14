# Unity Game Studio — GitHub Copilot Instructions

## Project Overview

This is a **Unity game development studio** project managed through a multi-role AI assistance system.
Each role owns a specific domain, enforcing separation of concerns and quality across the entire game development lifecycle.

## Technology Stack

- **Engine**: Unity (LTS recommended — 2022.3 LTS or later)
- **Language**: C#
- **Version Control**: Git with trunk-based development
- **Build System**: Unity Build Profiles + Unity Cloud Build
- **Asset Pipeline**: Addressables + Unity Asset Database
- **Render Pipeline**: URP (Universal Render Pipeline) — default; HDRP for high-end projects
- **UI Framework**: UI Toolkit (runtime); UGUI for world-space UI
- **Networking**: Unity Netcode for GameObjects (NGO) + Unity Transport

## Frameworks

### GameFoundation Framework

Custom Unity framework providing core systems for rapid game development:

- **DataManager**: Binary serialization-based save/load system with extensible collections
- **GameStateManager**: Application-level state machine managing game lifecycle (13 states: None → Init → Main → Ready → Play → Complete/GameOver/Restart/Next/Revice)
- **UIManager**: SOLID-based UI layer management (Scene/Popup/Dialog/Toast/Loading layers)

**Key principles:**
- Event-driven architecture — systems communicate via events, not direct references
- SOLID design — single responsibility, interface segregation, dependency inversion
- Separation of concerns — DataManager ↮ GameStateManager ↮ UIManager are independent

**Usage:**
- See [GameFoundation README](.claude/docs/frameworks/gamefoundation/README.md) for integration guide
- Code implementing GameFoundation automatically triggers [gamefoundation-code.instructions.md](.github/instructions/gamefoundation-code.instructions.md)
- Data collections automatically trigger [gamefoundation-data.instructions.md](.github/instructions/gamefoundation-data.instructions.md)

## Core Principles

### SOLID
- **Single Responsibility (SRP)**: each class has one reason to change — no monolithic MonoBehaviours
- **Open/Closed (OCP)**: systems are extendable via interfaces/inheritance without modifying existing code
- **Liskov Substitution (LSP)**: subtypes must be substitutable for their base types without breaking correctness
- **Interface Segregation (ISP)**: prefer small, focused interfaces (`IDamageable`, `IInteractable`) over large ones
- **Dependency Inversion (DIP)**: depend on abstractions (interfaces), not concretions

### KISS & DRY
- Prefer the simplest solution that meets requirements — avoid over-engineering
- Extract repeated logic into utility methods or ScriptableObject-driven configurations

### Performance First
- Target 60 FPS on mid-range devices
- Never call `GetComponent<T>()` in `Update()` — cache in `Awake()`
- Avoid LINQ in hot paths — allocates garbage
- Use `ObjectPool<T>` for frequently instantiated objects

### Data-Driven Design
- ALL gameplay values (damage, speed, range, cooldowns) MUST come from ScriptableObjects or config files — NEVER hardcoded
- Game balance data lives in `Assets/Data/` — never in C# code

## 🚨 CRITICAL: Approval Required Protocol

You MUST follow this workflow for every task:

1. **Read First** — Understand existing code and design docs
2. **Ask Questions** — Clarify requirements before proposing changes
3. **Propose Implementation** — Show structure/approach, get user approval
4. **Request Permission** — "May I write to [filepath]?" — WAIT for explicit approval
5. **Implement** — Only after user says "yes" or "proceed"

⛔ NEVER write or modify files without explicit user approval.
⛔ NEVER assume approval — always ask first.

## Multi-Role Development System

Role-specific instructions are in `.github/instructions/`. GitHub Copilot automatically applies them based on the file you are editing.

### Programming Roles

| Role | Domain | Instruction File |
|------|--------|-----------------|
| **Lead Programmer** | Architecture, code review, cross-system | `lead-programmer.instructions.md` |
| **Gameplay Programmer** | Mechanics, player systems, game feel | `gameplay-programmer.instructions.md` |
| **Engine Programmer** | Core infrastructure, scene management, save/load | `engine-programmer.instructions.md` |
| **UI Programmer** | UI controllers, ViewModels, screen management | `ui-programmer.instructions.md` |
| **Network Programmer** | Multiplayer, NGO, latency compensation | `network-programmer.instructions.md` |
| **AI Programmer** | Enemy AI, NavMesh, behavior trees, perception | `ai-programmer.instructions.md` |
| **Unity Specialist** | Unity API expertise, best practices | *(see lead-programmer)* |
| **Unity DOTS Specialist** | ECS/Jobs/Burst for performance-critical systems | *(see engine-programmer)* |
| **Unity Addressables Specialist** | Asset bundles, addressables pipeline | *(see engine-programmer)* |
| **Unity Shader Specialist** | Shader Graph, custom HLSL, VFX Graph | `shader-specialist.instructions.md` |
| **Unity UI Specialist** | UI Toolkit architecture, UXML/USS | `ui-programmer.instructions.md` |
| **Tools Programmer** | Editor tools, custom importers, build pipeline | *(see engine-programmer)* |
| **Prototyper** | Rapid prototyping, concept validation | `prototype-code.instructions.md` |
| **DevOps Engineer** | CI/CD, build servers, deployment | *(see lead-programmer)* |
| **Security Engineer** | Anti-cheat, input validation, network security | `network-programmer.instructions.md` |
| **Analytics Engineer** | Telemetry, KPIs, data pipelines | *(see engine-programmer)* |
| **Performance Analyst** | Profiling, optimization, budgets | *(see lead-programmer)* |

### Design Roles

| Role | Domain | Instruction File |
|------|--------|-----------------|
| **Creative Director** | Vision, pillars, creative decisions | *(see design-docs)* |
| **Game Designer** | Mechanics, systems, economy, level design | `game-designer.instructions.md` |
| **Systems Designer** | Complex game systems, meta-game | `game-designer.instructions.md` |
| **Level Designer** | Level layout, encounters, pacing | `design-docs.instructions.md` |
| **Economy Designer** | Currency, sinks, progression curves | `data-files.instructions.md` |
| **Narrative Director** | Story, themes, narrative arc | `narrative.instructions.md` |
| **Writer** | Dialogue, lore, script | `narrative.instructions.md` |
| **UX Designer** | User flows, interaction design | `ui-programmer.instructions.md` |

### Art & Audio Roles

| Role | Domain | Instruction File |
|------|--------|-----------------|
| **Art Director** | Visual style, art direction | `technical-artist.instructions.md` |
| **Technical Artist** | Shader Graph, VFX Graph, asset pipeline | `technical-artist.instructions.md` |
| **World Builder** | Environment art, scene construction | `technical-artist.instructions.md` |
| **Audio Director** | Audio strategy, music direction | *(general)* |
| **Sound Designer** | SFX implementation, audio scripting | *(general)* |

### Production & QA Roles

| Role | Domain | Instruction File |
|------|--------|-----------------|
| **Producer** | Schedule, milestones, scope | *(general)* |
| **QA Lead** | Test strategy, test plans | `qa-tester.instructions.md` |
| **QA Tester** | Test execution, bug reports | `qa-tester.instructions.md` |
| **Release Manager** | Build pipeline, release process | *(general)* |
| **Community Manager** | Player communication, feedback | *(general)* |

### Specialized Roles

| Role | Domain | Instruction File |
|------|--------|-----------------|
| **Localization Lead** | i18n, string tables, locale management | *(general)* |
| **Accessibility Specialist** | A11y features, inclusive design | `ui-programmer.instructions.md` |
| **Live Ops Designer** | Events, seasonal content, live services | `data-files.instructions.md` |
| **Technical Director** | High-level technical strategy | `lead-programmer.instructions.md` |

## Project Structure (Unity)

```
Assets/
├── Scripts/
│   ├── Core/          ← Engine layer (engine-programmer)
│   ├── Gameplay/      ← Game mechanics (gameplay-programmer)
│   ├── Player/        ← Player systems (gameplay-programmer)
│   ├── AI/            ← Enemy AI (ai-programmer)
│   ├── UI/            ← UI code (ui-programmer)
│   ├── Network/       ← Multiplayer (network-programmer)
│   └── Editor/        ← Editor tools (tools-programmer)
├── Data/              ← ScriptableObjects and config (game-designer)
├── Shaders/           ← Shader Graph + HLSL (shader-specialist)
├── Materials/         ← Materials (technical-artist)
├── VFX/               ← VFX Graph (technical-artist)
├── UI/                ← UXML/USS files (ui-programmer)
├── Audio/             ← Audio assets (sound-designer)
├── Tests/             ← Unity Test Framework (qa-tester)
└── Prefabs/           ← Prefabs by system

Design/                ← Design documents (game-designer)
prototypes/            ← Prototype code ONLY (prototyper)
docs/                  ← Technical documentation
```

## C# Naming Conventions

| Element | Convention | Example |
|---------|------------|---------|
| Class | PascalCase | `PlayerController` |
| Interface | `I` + PascalCase | `IDamageable` |
| Enum | PascalCase | `GameState` |
| Public property | PascalCase | `Health` |
| Private field | `_camelCase` | `_currentHealth` |
| Local variable | camelCase | `damageAmount` |
| Constant | PascalCase | `MaxHealth` |
| Method | PascalCase | `TakeDamage()` |
| Event | PascalCase | `OnDeath` |
| ScriptableObject | PascalCase + `SO` suffix | `CombatConfigSO` |

## Collaboration Protocol

Every task follows: **Question → Options → Decision → Draft → Approval**

- Always ask clarifying questions before writing any code
- Show structure/class diagrams before implementation
- Ask "May I write this to [filepath]?" before every file operation
- Multi-file changes require explicit approval for the full changeset
