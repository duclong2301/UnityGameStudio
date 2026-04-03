# Unity Game Studio

A complete Unity game development studio for Claude Code — specialized agents, skills, and workflows purpose-built for Unity (C#).

[![Unity](https://img.shields.io/badge/engine-Unity-black)](https://unity.com)
[![C#](https://img.shields.io/badge/language-C%23-purple)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![Built for Claude Code](https://img.shields.io/badge/built%20for-Claude%20Code-f5f5f5)](https://docs.anthropic.com/en/docs/claude-code)

---

## What's Included

| Category | Count | Description |
|---|---|---|
| **Agents** | 30+ | Specialized agents for design, Unity programming, art, audio, narrative, QA, and production |
| **Skills** | 37 | Slash commands for common workflows (`/start`, `/sprint-plan`, `/code-review`, `/brainstorm`, etc.) |
| **Hooks** | 7 | Automated validation on session start/stop, commits, asset writes, and gap detection |
| **Rules** | 9 | Path-scoped Unity C# coding standards for gameplay, engine, UI, shaders, AI, and more |
| **Templates** | 5+ | Document templates for GDDs, pillars, ADRs, sprint plans, and level design |

---

## Studio Hierarchy

```
Tier 1 — Directors (Opus)
  creative-director    technical-director    producer

Tier 2 — Department Leads (Sonnet)
  game-designer        lead-programmer       art-director
  audio-director       narrative-director    qa-lead
  release-manager      localization-lead

Tier 3 — Specialists (Sonnet/Haiku)
  gameplay-programmer  engine-programmer     ai-programmer
  network-programmer   tools-programmer      ui-programmer
  systems-designer     level-designer        economy-designer
  technical-artist     sound-designer        writer
  world-builder        ux-designer           prototyper
  performance-analyst  devops-engineer       analytics-engineer
  security-engineer    qa-tester
  accessibility-specialist  live-ops-designer  community-manager

Unity Engine Specialists
  unity-specialist
    ├── unity-dots-specialist        (ECS, Jobs, Burst)
    ├── unity-shader-specialist      (Shader Graph, VFX Graph, URP/HDRP)
    ├── unity-addressables-specialist (asset management, CDN)
    └── unity-ui-specialist          (UI Toolkit, UGUI, data binding)
```

---

## Slash Commands

Type `/` in Claude Code to access all 37 skills:

**Reviews & Analysis**
`/design-review` `/code-review` `/balance-check` `/asset-audit` `/scope-check` `/perf-profile` `/tech-debt`

**Production**
`/sprint-plan` `/milestone-review` `/estimate` `/retrospective` `/bug-report`

**Project Management**
`/start` `/project-stage-detect` `/reverse-document` `/gate-check` `/map-systems` `/design-system`

**Release**
`/release-checklist` `/launch-checklist` `/changelog` `/patch-notes` `/hotfix`

**Creative**
`/brainstorm` `/playtest-report` `/prototype` `/onboard` `/localize`

**Unity-Specific**
`/setup-engine` `/architecture-decision`

**Team Orchestration**
`/team-combat` `/team-narrative` `/team-ui` `/team-release` `/team-polish` `/team-audio` `/team-level`

---

## Getting Started

### Prerequisites

- [Git](https://git-scm.com/)
- [Claude Code](https://docs.anthropic.com/en/docs/claude-code) (`npm install -g @anthropic-ai/claude-code`)
- [Unity](https://unity.com/download) (LTS version recommended)

### Setup

1. **Clone this repository** into your Unity project root (or use as a template):
   ```bash
   git clone https://github.com/duclong2301/UnityGameStudio my-unity-game
   cd my-unity-game
   ```

2. **Open Claude Code**:
   ```bash
   claude
   ```

3. **Run `/start`** — the system asks where you are and guides you to the right workflow:
   - No idea → `/brainstorm` to explore concepts
   - Have a concept → `/setup-engine unity 2022.3.x` to configure your environment
   - Existing project → `/project-stage-detect` to assess state

---

## Project Structure

```
CLAUDE.md                               # Master Unity configuration
.claude/
  settings.json                         # Hooks, permissions, safety rules
  agents/                               # 30+ agent definitions
  skills/                               # 37 slash commands
  hooks/                                # 7 automation hooks
  rules/                                # 9 Unity C# coding standards
  docs/
    coding-standards.md                 # Unity C# naming and style guide
    coordination-rules.md               # Agent hierarchy
    technical-preferences.md            # Engine, packages, CI preferences
    templates/                          # GDD, pillars, ADR templates

src/                                    # Unity C# source code
  Core/                                 # Engine-layer: events, pooling, scene loading
  Gameplay/                             # Game mechanics and systems
  UI/                                   # UI controllers and ViewModels
  Network/                              # Multiplayer (if applicable)
  Audio/                                # Audio management

assets/                                 # Unity assets (art, audio, VFX, shaders, data)
design/                                 # Design documents (GDDs, narrative, levels)
docs/                                   # Technical docs (ADRs, Unity version reference)
tests/                                  # Unity Test Framework tests (Edit Mode + Play Mode)
tools/                                  # Build and pipeline tools
prototypes/                             # Throwaway prototypes (isolated from src/)
production/                             # Sprint plans, milestones, release tracking
```

---

## Unity Best Practices Enforced

The studio enforces Unity-specific standards through path-scoped rules:

- **No `GetComponent<T>()` in `Update()`** — cache in `Awake()`
- **No hardcoded gameplay values** — all from ScriptableObjects
- **No `Resources.Load()`** — use Addressables
- **`[SerializeField] private`** instead of `public` inspector fields
- **Zero GC allocations** in gameplay hot paths
- **Assembly definitions** for all code folders
- **URP/HDRP only** — never built-in render pipeline
- **New Input System only** — not legacy `Input.GetKey()`

---

## How It Works

### Agent Coordination

Agents follow a structured delegation model:
1. **Vertical delegation** — directors delegate to leads, leads to specialists
2. **Horizontal consultation** — same-tier agents consult each other
3. **Conflict resolution** — escalates to shared parent director
4. **Unity hierarchy** — `unity-specialist` coordinates all four Unity sub-specialists

### Collaborative, Not Autonomous

Every agent follows: **Question → Options → Decision → Draft → Approval**

Agents always ask "May I write this to [filepath]?" before creating or modifying files. No commits without user instruction.

---

## License

MIT License — see [LICENSE](LICENSE) for details.

Based on the [Claude Code Game Studios](https://github.com/Donchitos/Claude-Code-Game-Studios) template, specialized for Unity.
