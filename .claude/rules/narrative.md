---
paths:
  - "design/narrative/**/*.md"
  - "assets/data/dialogue/**"
---

# Narrative Rules

- All dialogue must go through the writer agent — no ad-hoc dialogue in code strings
- Character voice must be consistent — each character has a voice guide in `design/narrative/characters/`
- Dialogue strings must use localization keys, not hardcoded text in C# code
- All story-critical events must be documented in `design/narrative/story-beats.md`
- Cutscene scripts use the standard format: [CHARACTER] [ACTION] [DIALOGUE]
- Narrative documents reference game mechanics to ensure story and gameplay are aligned
- Tone and themes established in `design/pillars.md` override department-level narrative choices
