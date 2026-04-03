---
name: team-audio
description: "Orchestrates audio work across the full audio team: audio-director for strategy and sound-designer for implementation."
argument-hint: "[audio feature or scene description]"
user-invocable: true
allowed-tools: Read, Glob, Grep, Write, Edit, Task
---

When this skill is invoked:

1. **Read** audio design documents and audio mixer setup.

2. **audio-director**: Define audio strategy for the feature
   - Music state changes, SFX categories, spatial requirements

3. **sound-designer**: Implement audio
   - Create/reference audio assets
   - AudioSource configuration, Audio Mixer routing
   - Naming: follow `SFX_[Category]_[Action]` convention

4. **qa-tester**: Audio testing
   - All triggers fire correctly
   - No audio leaks (AudioSources still playing after scene unload)
   - Volume levels balanced

5. **Report**: files created, outstanding assets needed from external sound designer.
