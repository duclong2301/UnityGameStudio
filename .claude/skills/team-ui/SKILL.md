---
name: team-ui
description: "Orchestrates a full team to design and implement a UI screen or flow. Coordinates ux-designer, unity-ui-specialist, ui-programmer, localization-lead, and accessibility-specialist."
argument-hint: "[UI screen name or description]"
user-invocable: true
allowed-tools: Read, Glob, Grep, Write, Edit, Task
---

When this skill is invoked:

1. **Read context** — existing UI docs, design pillars, UX patterns.

2. **Confirm with user**:
   - "Is this a new screen or updating an existing one?"
   - "Primary interaction: menu navigation, inventory, HUD, or dialog?"
   - "Target: UI Toolkit or UGUI? (UI Toolkit recommended for new screens)"

3. **Launch agents in parallel**:

   - **ux-designer**: UX flow and wireframe
     - User journey, decision points, navigation map
     
   - **unity-ui-specialist**: Architecture recommendation
     - UXML structure, USS themes, data binding approach
     
   - **localization-lead**: String requirements
     - Key naming, expansion factors, font requirements
     
   - **accessibility-specialist**: Accessibility requirements
     - Contrast check, keyboard/gamepad navigation, screen reader needs

4. **Review outputs with user**, then:

   - **ui-programmer**: Implements the UXML, USS, and controller code
   - **qa-tester**: Tests navigation, localization, accessibility, input devices

5. **Summary report**:
   - Files created
   - Outstanding issues
   - Test results
