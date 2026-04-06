# Migration: `.claude/` → GitHub Copilot Format

This document describes the migration of the `.claude/` configuration structure to GitHub Copilot's
`.github/instructions/` format, enabling role-based AI assistance in VS Code with GitHub Copilot.

## Overview

The original `.claude/` configuration has been converted to GitHub Copilot-compatible files in `.github/`.
The original `.claude/` directory has been kept intact — this is an additive migration, not a replacement.

## Migrated Files

### Root Overview

| New File | Description |
|----------|-------------|
| `.github/copilot-instructions.md` | Project overview, technology stack, all 39 roles, Unity project structure, naming conventions, collaboration protocol |

### Rule Files (11 migrated)

| Original (`.claude/rules/`) | New (`.github/instructions/`) | Path Pattern |
|-----------------------------|-------------------------------|--------------|
| `gameplay-code.md` | `gameplay-code.instructions.md` | `Assets/Scripts/Gameplay/**/*.cs`, `Assets/Scripts/Player/**/*.cs`, `Assets/Scripts/Combat/**/*.cs` |
| `data-files.md` | `data-files.instructions.md` | `Assets/Data/**`, `Design/**/*.json`, `Design/**/*.csv` |
| `ai-code.md` | `ai-code.instructions.md` | `Assets/Scripts/AI/**/*.cs` |
| `design-docs.md` | `design-docs.instructions.md` | `Design/**/*.md`, `docs/design/**/*.md` |
| `engine-code.md` | `engine-code.instructions.md` | `Assets/Scripts/Core/**/*.cs`, `Assets/Scripts/Engine/**/*.cs` |
| `narrative.md` | `narrative.instructions.md` | `Design/narrative/**/*.md`, `Assets/Data/Dialogue/**` |
| `network-code.md` | `network-code.instructions.md` | `Assets/Scripts/Network/**/*.cs`, `Assets/Scripts/Multiplayer/**/*.cs` |
| `prototype-code.md` | `prototype-code.instructions.md` | `Assets/Prototypes/**/*.cs`, `prototypes/**/*.cs` |
| `shader-code.md` | `shader-code.instructions.md` | `Assets/Shaders/**`, `Assets/Materials/**`, `Assets/VFX/**` |
| `test-standards.md` | `test-standards.instructions.md` | `Assets/Tests/**/*.cs`, `Assets/Editor/Tests/**/*.cs` |
| `ui-code.md` | `ui-code.instructions.md` | `Assets/Scripts/UI/**/*.cs`, `Assets/UI/**/*.uxml`, `Assets/UI/**/*.uss` |

### Agent Role Files (10 created)

| Agent (`.claude/agents/`) | New (`.github/instructions/`) | Paths Applied To |
|---------------------------|-------------------------------|------------------|
| `gameplay-programmer.md` | `gameplay-programmer.instructions.md` | `Assets/Scripts/Gameplay/**`, `Assets/Scripts/Player/**`, `Assets/Scripts/AI/**`, `Assets/Scripts/Combat/**` |
| `game-designer.md` | `game-designer.instructions.md` | `Assets/Data/**`, `Design/**` |
| `technical-artist.md` | `technical-artist.instructions.md` | `Assets/Shaders/**`, `Assets/Materials/**`, `Assets/VFX/**` |
| `ui-programmer.md` | `ui-programmer.instructions.md` | `Assets/Scripts/UI/**`, `Assets/UI/**` |
| `engine-programmer.md` | `engine-programmer.instructions.md` | `Assets/Scripts/Core/**`, `Assets/Scripts/Engine/**` |
| `network-programmer.md` | `network-programmer.instructions.md` | `Assets/Scripts/Network/**`, `Assets/Scripts/Multiplayer/**` |
| `unity-shader-specialist.md` | `shader-specialist.instructions.md` | `Assets/Shaders/**/*.shader`, `Assets/Shaders/**/*.shadergraph` |
| `ai-programmer.md` | `ai-programmer.instructions.md` | `Assets/Scripts/AI/**/*.cs` |
| `qa-tester.md` | `qa-tester.instructions.md` | `Assets/Tests/**/*.cs`, `Assets/Editor/Tests/**/*.cs` |
| `lead-programmer.md` | `lead-programmer.instructions.md` | `Assets/Scripts/**/*.cs` (general oversight) |

## Path Mapping Table

Paths from original `.claude/rules/` frontmatter mapped to Unity project structure:

| Original Path | Unity Path |
|---------------|-----------|
| `src/**/*.cs` | `Assets/Scripts/**/*.cs` |
| `src/AI/**/*.cs` | `Assets/Scripts/AI/**/*.cs` |
| `src/Network/**/*.cs` | `Assets/Scripts/Network/**/*.cs` |
| `src/Multiplayer/**/*.cs` | `Assets/Scripts/Multiplayer/**/*.cs` |
| `src/Engine/**/*.cs` | `Assets/Scripts/Engine/**/*.cs` |
| `src/Core/**/*.cs` | `Assets/Scripts/Core/**/*.cs` |
| `src/UI/**/*.cs` | `Assets/Scripts/UI/**/*.cs` |
| `src/UI/**/*.uxml` | `Assets/UI/**/*.uxml` |
| `src/UI/**/*.uss` | `Assets/UI/**/*.uss` |
| `src/Rendering/**/*.cs` | `Assets/Scripts/Rendering/**/*.cs` |
| `assets/data/**` | `Assets/Data/**` |
| `assets/shaders/**` | `Assets/Shaders/**` |
| `design/**/*.md` | `Design/**/*.md` |
| `design/**/*.json` | `Design/**/*.json` |
| `design/**/*.csv` | `Design/**/*.csv` |
| `design/narrative/**/*.md` | `Design/narrative/**/*.md` |
| `assets/data/dialogue/**` | `Assets/Data/Dialogue/**` |
| `tests/**/*.cs` | `Assets/Tests/**/*.cs` |
| `prototypes/**/*.cs` | `Assets/Prototypes/**/*.cs` or `prototypes/**/*.cs` |

## All 39 Agent Roles

The following roles exist in `.claude/agents/`. 10 have dedicated instruction files; the remaining roles
are covered by the root `copilot-instructions.md` overview or by related instruction files.

| Agent | Has Dedicated File | Covered By |
|-------|--------------------|------------|
| `accessibility-specialist` | ✗ | `ui-programmer.instructions.md` |
| `ai-programmer` | ✅ | `ai-programmer.instructions.md` |
| `analytics-engineer` | ✗ | Root overview |
| `art-director` | ✗ | `technical-artist.instructions.md` |
| `audio-director` | ✗ | Root overview |
| `community-manager` | ✗ | Root overview |
| `creative-director` | ✗ | `design-docs.instructions.md` |
| `devops-engineer` | ✗ | Root overview |
| `economy-designer` | ✗ | `data-files.instructions.md` |
| `engine-programmer` | ✅ | `engine-programmer.instructions.md` |
| `game-designer` | ✅ | `game-designer.instructions.md` |
| `gameplay-programmer` | ✅ | `gameplay-programmer.instructions.md` |
| `lead-programmer` | ✅ | `lead-programmer.instructions.md` |
| `level-designer` | ✗ | `design-docs.instructions.md` |
| `live-ops-designer` | ✗ | `data-files.instructions.md` |
| `localization-lead` | ✗ | Root overview |
| `narrative-director` | ✗ | `narrative.instructions.md` |
| `network-programmer` | ✅ | `network-programmer.instructions.md` |
| `performance-analyst` | ✗ | `lead-programmer.instructions.md` |
| `producer` | ✗ | Root overview |
| `prototyper` | ✗ | `prototype-code.instructions.md` |
| `qa-lead` | ✗ | `qa-tester.instructions.md` |
| `qa-tester` | ✅ | `qa-tester.instructions.md` |
| `release-manager` | ✗ | Root overview |
| `security-engineer` | ✗ | `network-programmer.instructions.md` |
| `sound-designer` | ✗ | Root overview |
| `systems-designer` | ✗ | `game-designer.instructions.md` |
| `technical-artist` | ✅ | `technical-artist.instructions.md` |
| `technical-director` | ✗ | `lead-programmer.instructions.md` |
| `tools-programmer` | ✗ | `engine-programmer.instructions.md` |
| `ui-programmer` | ✅ | `ui-programmer.instructions.md` |
| `unity-addressables-specialist` | ✗ | `engine-programmer.instructions.md` |
| `unity-dots-specialist` | ✗ | `engine-programmer.instructions.md` |
| `unity-shader-specialist` | ✅ | `shader-specialist.instructions.md` |
| `unity-specialist` | ✗ | `lead-programmer.instructions.md` |
| `unity-ui-specialist` | ✗ | `ui-programmer.instructions.md` |
| `ux-designer` | ✗ | `ui-programmer.instructions.md` |
| `world-builder` | ✗ | `technical-artist.instructions.md` |
| `writer` | ✗ | `narrative.instructions.md` |

## What Was NOT Migrated

The following `.claude/` features have no direct equivalent in GitHub Copilot:

| Feature | `.claude/` Location | Reason Not Migrated |
|---------|---------------------|---------------------|
| Pre/post hooks | `.claude/hooks/` | GitHub Copilot has no lifecycle hooks |
| Automatic approval workflow | `.claude/settings.json` | Copilot doesn't enforce workflows — embedded as text instructions instead |
| Agent coordination (multi-agent) | `.claude/agents/` + orchestration | Copilot doesn't support multi-agent orchestration |
| Skills / macros | `.claude/skills/` | No equivalent in Copilot — these are interactive Claude workflows |
| Context management | `.claude/docs/context-management.md` | Handled automatically by Copilot |
| Settings (model, maxTurns, tools) | `.claude/settings.json` | Not configurable per-file in Copilot |

## How GitHub Copilot Uses These Files

1. **`.github/copilot-instructions.md`** — always loaded as context for all files in the repository
2. **`.github/instructions/*.instructions.md`** — loaded automatically when you open a file matching the `applyTo` glob pattern
3. Multiple instruction files can apply simultaneously (e.g., opening a gameplay .cs file loads both `gameplay-code.instructions.md` and `gameplay-programmer.instructions.md`)

## Verification

To verify Copilot is reading the instructions:

1. Open any file in VS Code (e.g., `Assets/Scripts/Gameplay/PlayerController.cs`)
2. Open Copilot Chat (`Ctrl+Alt+I` / `Control+Command+I`)
3. Ask: "What are my instructions for this file?"
4. Copilot should list the applicable rules from the instruction files
