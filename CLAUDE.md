# Unity Game Studio — Agent Architecture

Indie game development in Unity managed through coordinated Claude Code subagents.
Each agent owns a specific domain, enforcing separation of concerns and quality.

---

## Technology Stack

- **Engine**: Unity (LTS recommended)
- **Language**: C#
- **Version Control**: Git with trunk-based development
- **Build System**: Unity Build Profiles + Unity Cloud Build
- **Asset Pipeline**: Addressables + Unity Asset Database
- **Render Pipeline**: URP (Universal Render Pipeline) — default; HDRP for high-end projects
- **UI Framework**: UI Toolkit (runtime); UGUI for world-space UI

---

## Frameworks

### GameFoundation Framework

Custom Unity framework providing core systems for rapid game development.

See `.claude/docs/frameworks/gamefoundation/README.md` for architecture and integration guide.

---

## Project Structure

@.claude/docs/directory-structure.md

---

## Technical Preferences

@.claude/docs/technical-preferences.md

---

## Coordination Rules

@.claude/docs/coordination-rules.md

---

## Collaboration Protocol

**User-driven collaboration, not autonomous execution.**
Every task follows: **Question → Options → Decision → Draft → Approval**

- Agents MUST ask "May I write this to [filepath]?" before using Write/Edit tools.
- Agents MUST show drafts or summaries before requesting approval.
- Multi-file changes require explicit approval for the full changeset.
- No commits without explicit user instruction.

See `.claude/docs/collaboration-protocol.md` for full protocol and examples.

> **First session?** If the project has no game concept configured yet, run `/start` to begin the guided onboarding flow.

---

## Coding Standards

@.claude/docs/coding-standards.md

---

## Context Management

@.claude/docs/context-management.md

---

## Agent Behavioral Guidelines

> These guidelines bias toward caution over speed. For trivial tasks, use judgment.

### 1. Think Before Coding

**Don't assume. Don't hide confusion. Surface tradeoffs.**

- State assumptions explicitly. If uncertain, ask.
- If multiple interpretations exist, present them — don't pick silently.
- If a simpler approach exists, say so. Push back when warranted.
- If something is unclear, stop. Name what's confusing. Ask.

### 2. Simplicity First

**Minimum code that solves the problem. Nothing speculative.**

- No features beyond what was asked.
- No abstractions for single-use code.
- No "flexibility" or "configurability" that wasn't requested.
- No error handling for impossible scenarios.
- If you write 200 lines and it could be 50, rewrite it.

Ask: *"Would a senior engineer say this is overcomplicated?"* If yes, simplify.

### 3. Surgical Changes

**Touch only what you must. Clean up only your own mess.**

When editing existing code:
- Don't "improve" adjacent code, comments, or formatting.
- Don't refactor things that aren't broken.
- Match existing style, even if you'd do it differently.
- If you notice unrelated dead code, mention it — don't delete it.

When your changes create orphans:
- Remove imports/variables/functions that YOUR changes made unused.
- Don't remove pre-existing dead code unless asked.

Every changed line should trace directly to the user's request.

### 4. Goal-Driven Execution

**Define success criteria. Loop until verified.**

Transform tasks into verifiable goals:
- "Add validation" → "Write tests for invalid inputs, then make them pass"
- "Fix the bug" → "Write a test that reproduces it, then make it pass"
- "Refactor X" → "Ensure tests pass before and after"

For multi-step tasks, state a brief plan:
```
1. [Step] → verify: [check]
2. [Step] → verify: [check]
3. [Step] → verify: [check]
```

**These guidelines are working if:** fewer unnecessary changes in diffs, fewer rewrites due to overcomplication, and clarifying questions come before implementation rather than after mistakes.
