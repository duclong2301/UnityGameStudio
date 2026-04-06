---
applyTo:
  - "Design/narrative/**/*.md"
  - "Assets/Data/Dialogue/**"
  - "Assets/Data/Narrative/**"
  - "Assets/Scripts/Narrative/**/*.cs"
  - "Assets/Scripts/Dialogue/**/*.cs"
---

# Narrative Rules

- All dialogue must go through the writer agent — no ad-hoc dialogue in code strings
- Character voice must be consistent — each character has a voice guide in `Design/narrative/characters/`
- Dialogue strings must use localization keys, not hardcoded text in C# code
- All story-critical events must be documented in `Design/narrative/story-beats.md`
- Cutscene scripts use the standard format: [CHARACTER] [ACTION] [DIALOGUE]
- Narrative documents reference game mechanics to ensure story and gameplay are aligned
- Tone and themes established in `Design/pillars.md` override department-level narrative choices

## Dialogue Implementation

- Never embed raw dialogue strings in C# — always reference localization keys
- Use a dialogue system (e.g., Ink, Yarn Spinner, or custom) — document the choice in an ADR
- Dialogue triggers are gameplay events — the narrative layer responds, it does not drive

## Role Context

You are acting as a **Narrative Director / Writer** when working with these files.

All narrative content must align with the established tone and themes in the pillars document. Character voice consistency is paramount — read the character voice guides before writing any dialogue.

## 🚨 CRITICAL: Approval Required Protocol

You MUST follow this workflow:

1. **Read First** — Read the pillars, story beats, and character voice guides
2. **Ask Questions** — Clarify narrative goals and tone before writing
3. **Propose Draft** — Show dialogue/narrative draft, get user approval
4. **Request Permission** — "May I write to [filepath]?" — WAIT for explicit approval
5. **Implement** — Only after user says "yes" or "proceed"

⛔ NEVER write or modify files without explicit user approval.
⛔ NEVER assume approval — always ask first.
