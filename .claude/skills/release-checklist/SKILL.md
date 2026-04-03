---
name: release-checklist
description: "Generates a platform-specific release checklist and validates that the project meets all requirements for release. Covers build settings, store assets, legal, and QA sign-off."
argument-hint: "[platform: pc|mobile-ios|mobile-android|console]"
user-invocable: true
allowed-tools: Read, Glob, Grep, Write
---

When this skill is invoked:

1. **Read** `docs/engine-reference/unity/VERSION.md` for platform/build config.
2. **Read** most recent milestone review from `production/milestones/`.
3. **Count open P1/P2 bugs**.

4. **Generate the checklist**:

```markdown
# Release Checklist — [Platform] — v[Version]

## Build Settings
- [ ] Scripting Backend: IL2CPP
- [ ] Development Build: OFF
- [ ] Profiler: OFF
- [ ] Deep Profiling: OFF
- [ ] Stack Trace: OFF (or Script Only)
- [ ] Bundle ID / App ID: verified
- [ ] Version number: [x.x.x] correct
- [ ] Build number: auto-incremented

## Code Quality
- [ ] All P1 and P2 bugs resolved
- [ ] QA sign-off from qa-lead
- [ ] No `Debug.Log` statements in release build
- [ ] No test accounts or debug endpoints active

## Content
- [ ] All placeholder art replaced with final art
- [ ] All placeholder audio replaced with final audio
- [ ] All dialogue/narrative content reviewed and approved
- [ ] Credits screen: all contributors listed, all third-party licenses included

## Localization (if applicable)
- [ ] All supported locales: translation complete and reviewed
- [ ] Font coverage: all characters in all locales render correctly
- [ ] RTL languages: layout verified

## Analytics & Privacy
- [ ] Analytics consent screen shown before data collection
- [ ] Privacy policy URL current
- [ ] Age rating applied (ESRB / PEGI / etc.)

## Performance
- [ ] Profiled on minimum spec hardware
- [ ] Frame rate target met on all supported platforms
- [ ] Load times within spec

## Platform-Specific
[PC]
- [ ] Minimum/Recommended specs listed on store page
- [ ] All key bindings configurable
- [ ] Windowed and fullscreen modes work

[Mobile iOS]
- [ ] App Store Connect: all metadata complete
- [ ] Required permissions: justified (camera, location, etc.)
- [ ] Tested on oldest supported iOS version

[Mobile Android]
- [ ] Google Play: target API level current
- [ ] Data safety form submitted

## Store Assets
- [ ] Screenshots: all required sizes
- [ ] Trailer: uploaded
- [ ] Short description: ≤ 80 characters
- [ ] Long description: proofread, no placeholder text

## Sign-Off
- [ ] Technical Director sign-off
- [ ] QA Lead sign-off
- [ ] Producer sign-off
```

5. **Ask for approval** before writing to `production/milestones/release-checklist-v[version].md`.
