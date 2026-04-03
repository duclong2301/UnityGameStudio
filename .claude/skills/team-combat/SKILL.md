---
name: team-combat
description: "Orchestrates a full team of agents to design and implement a complete combat system. Coordinates game-designer, gameplay-programmer, ai-programmer, unity-specialist, qa-tester, and sound-designer in parallel."
argument-hint: "[brief combat system description or design doc path]"
user-invocable: true
allowed-tools: Read, Glob, Grep, Write, Edit, Task
---

When this skill is invoked:

1. **Read the combat design document** if provided as path argument, or ask for the description.

2. **Confirm scope** with the user:
   - "Which aspects do you want covered? (design / implementation / AI / audio / testing / all)"
   - "Is there an existing system to extend, or is this greenfield?"

3. **Launch agents in parallel** for independent work streams:

   - **game-designer**: Review or create `design/gdd/combat-system.md`
     - Core mechanics, hit detection, damage model, status effects
     
   - **unity-specialist**: Architectural guidance
     - MonoBehaviour vs DOTS recommendation based on entity count
     - Physics layer setup, collision matrix
     
   - **gameplay-programmer**: Player combat implementation
     - Attack system, hit detection (`Physics.OverlapSphere`), damage application
     - Uses `IDamageable` interface
     
   - **ai-programmer**: Enemy combat behavior
     - Attack patterns, dodge behavior, threat assessment
     
   - **sound-designer**: Combat audio
     - Hit sounds, weapon SFX, hurt sounds, death sounds
     
   - **qa-tester**: Test plan
     - Damage calculations, edge cases, balance test cases

4. **Synthesize results** — present unified summary to user:
   - What each agent produced
   - Integration points between systems
   - Outstanding decisions requiring user input

5. **Ask**: "Shall I proceed with implementation, or do you need to review the design first?"
