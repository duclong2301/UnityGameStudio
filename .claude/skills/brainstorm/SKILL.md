---
name: brainstorm
description: "Explores and develops game ideas through structured creative ideation. Presents multiple distinct game concepts with pillar analysis, genre positioning, and technical feasibility. Run this when starting from scratch or when exploring new game directions. Also supports Reference Mode when given a store URL or game name — generates differentiators or variants instead of from-scratch concepts."
argument-hint: "[optional theme, constraint, or reference game URL/name]"
user-invocable: true
allowed-tools: Read, Glob, Grep, Write, WebSearch, WebFetch
---

When this skill is invoked:

1. **Read `design/pillars.md`** if it exists — existing pillars constrain the brainstorm.

2. **Detect mode** from the argument:
   - If argument is a **URL** (store link) or **looks like a reference game** ("clone XYZ", "like Vampire Survivors") → **Reference Mode** (step 3R)
   - Otherwise → **From-Scratch Mode** (step 3S)

3R. **Reference Mode**:

   a. If a URL was provided, use `WebFetch` to gather intel: title, studio, genre, description, screenshots, monetization, install count, update cadence.
      If only a game name was given, ask the user for a link or use `WebSearch` to locate one.

   b. **Summarize the reference**:
      - Core loop (best guess from intel)
      - Genre + sub-genre
      - Art style
      - Monetization model
      - Estimated scope of the original
      - Why it likely succeeds in market

   c. **Ask the user for clone mode** (if not already set by `/start`):
      - `1:1` — faithful clone → brainstorm will produce **3-5 differentiators** (twist mechanic / setting / audience / monetization / platform)
      - `reskin` — keep core loop, change wrapper → brainstorm will produce **3-5 concept variants** (different themes/audiences sharing the same loop)
      - `inspiration` — borrow elements → brainstorm will produce **3-5 new concepts** that share only 1-2 elements with the reference

   d. **Generate 3-5 options** per the chosen mode, using the per-concept template in step 4.
      In Reference Mode, each option must include an extra field:
      ```
      **Relation to reference**: [What is kept, what is changed, what is new]
      ```

3S. **From-Scratch Mode**:

   a. **Ask the user for any constraints** (if not provided as argument):
      - Genre preferences?
      - Target platform (PC, mobile, console)?
      - Team size / scope?
      - Any mechanics that are required or off-limits?

   b. **Generate 3 distinct game concepts** that span different approaches:
      - Each concept at a different complexity/ambition level (indie-small, indie-mid, indie-large)
      - Each with a genuinely different core fantasy

4. **For each concept, provide**:
   ```
   ## Concept [A/B/C/D/E]: [Name]

   **Core Fantasy**: [What does the player get to BE or DO?]
   **Unique Hook**: "It's like [reference], AND ALSO [differentiator]"
   **Target Aesthetic** (MDA): [Primary aesthetic: Sensation/Fantasy/Narrative/Challenge/Discovery/etc.]

   **Core Loop** (30-second cycle):
   1. [Player action]
   2. [World response]
   3. [Player reward/progression]

   **Unity Technical Complexity**: [Low / Medium / High]
   [Brief notes on key systems needed]

   **Scope Estimate**: [Weekend jam / 1-3 months / 6-12 months / 1+ year]

   **Why this could be great**: [What makes it compelling]
   **Why this could fail**: [Main risk or challenge]

   [Reference Mode only]
   **Relation to reference**: [What is kept, what is changed, what is new]
   ```

5. **Present all concepts**, then ask:
   - "Which concept resonates most? Or would you like to combine elements?"
   - "Is there an aspect of any concept you want to explore further?"

6. **🆕 Screen Specification step (MANDATORY after a direction is picked)**:

   Before offering to create pillars/GDD, lock the UI foundation by asking the user these questions in order. Do not skip — getting this wrong forces rework of every wireframe, prefab, and anchor later.

   a. **Orientation**:
      - Portrait — mobile casual, hyper-casual, RPG verticals
      - Landscape — action, racing, strategy, console/PC
      - Both — only if truly required (≈2× UI workload)

   b. **Target platform(s)** → drives aspect ratio range:
      | Target | Reference resolution | Aspect range to support |
      |---|---|---|
      | Mobile Portrait | 1080×1920 (9:16) | 9:16 → 9:21 |
      | Mobile Landscape | 1920×1080 (16:9) | 16:9 → 21:9 |
      | Tablet | 2048×1536 (4:3) | 4:3 → 16:10 |
      | PC | 1920×1080 (16:9) | 16:9 → 32:9 (ultrawide) |
      | Console | 1920×1080 (16:9) | fixed 16:9 |

   c. **Reference resolution** for design (default to the table above unless user overrides).

   d. **Safe area policy**:
      - iOS notch / Dynamic Island
      - Android punch-hole / nav bar
      - Foldables (if relevant)

   e. **Unity CanvasScaler config** (proposed defaults, user can override):
      - UI Scale Mode: Scale With Screen Size
      - Reference Resolution: from (c)
      - Screen Match Mode: Match Width Or Height
      - Match value: 0.5 (balanced) — adjust to 0 (width) for landscape-first or 1 (height) for portrait-first

   f. **Asset resolution tiers**: 1x / 2x / 3x decision based on platform.

   After the user confirms each item, **offer to write the answers** to `design/ui/screen-spec.md` (request approval before Write per the collaboration protocol). This document becomes the source of truth for every later UX/UI step.

7. **If user selects a direction** (after screen spec is locked):
   - Offer to create `design/pillars.md` from the chosen concept
   - Offer to start a GDD outline
   - Suggest running `/setup-engine unity [version]` next
