# Unity Game Studio — Agent Architecture

Indie game development in Unity managed through coordinated Claude Code subagents.
Each agent owns a specific domain, enforcing separation of concerns and quality.

## Technology Stack

- **Engine**: Unity (LTS recommended)
- **Language**: C#
- **Version Control**: Git with trunk-based development
- **Build System**: Unity Build Profiles + Unity Cloud Build
- **Asset Pipeline**: Addressables + Unity Asset Database
- **Render Pipeline**: URP (Universal Render Pipeline) — default; HDRP for high-end projects
- **UI Framework**: UI Toolkit (runtime); UGUI for world-space UI

## Frameworks

### GameFoundation Framework

Custom Unity framework providing core systems for rapid game development:

- **DataManager**: Binary serialization-based save/load system with extensible collections
- **GameStateManager**: Application-level state machine managing game lifecycle
- **UIManager**: SOLID-based UI layer management (Scene/Popup/Dialog/Toast/Loading)

See `.claude/docs/frameworks/gamefoundation/README.md` for architecture and integration guide.

## Project Structure

@.claude/docs/directory-structure.md

## Technical Preferences

@.claude/docs/technical-preferences.md

## Coordination Rules

@.claude/docs/coordination-rules.md

## Collaboration Protocol

**User-driven collaboration, not autonomous execution.**
Every task follows: **Question → Options → Decision → Draft → Approval**

- Agents MUST ask "May I write this to [filepath]?" before using Write/Edit tools
- Agents MUST show drafts or summaries before requesting approval
- Multi-file changes require explicit approval for the full changeset
- No commits without user instruction

See `.claude/docs/collaboration-protocol.md` for full protocol and examples.

> **First session?** If the project has no game concept configured yet,
> run `/start` to begin the guided onboarding flow.

## Coding Standards

@.claude/docs/coding-standards.md

## Context Management

@.claude/docs/context-management.md
