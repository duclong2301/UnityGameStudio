---
name: brainstorm
description: "Explores and develops game ideas through structured creative ideation. Presents multiple distinct game concepts with pillar analysis, genre positioning, and technical feasibility. Run this when starting from scratch or when exploring new game directions."
argument-hint: "[optional theme or constraint]"
user-invocable: true
allowed-tools: Read, Glob, Grep, Write, WebSearch
---

When this skill is invoked:

1. **Read `design/pillars.md`** if it exists — existing pillars constrain the brainstorm.

2. **Ask the user for any constraints** (if not provided as argument):
   - Genre preferences?
   - Target platform (PC, mobile, console)?
   - Team size / scope?
   - Any mechanics that are required or off-limits?

3. **Generate 3 distinct game concepts** that span different approaches:
   - Each concept at a different complexity/ambition level (indie-small, indie-mid, indie-large)
   - Each with a genuinely different core fantasy

4. **For each concept, provide**:
   ```
   ## Concept [A/B/C]: [Name]
   
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
   ```

5. **Present all 3 concepts**, then ask:
   - "Which concept resonates most? Or would you like to combine elements?"
   - "Is there an aspect of any concept you want to explore further?"

6. **If user selects a direction**:
   - Offer to create `design/pillars.md` from the chosen concept
   - Offer to start a GDD outline
   - Suggest running `/setup-engine unity [version]` next
