---
name: localize
description: "Prepares a feature or screen for localization. Audits for hardcoded strings, assigns localization keys, and creates the translation-ready string table."
argument-hint: "[path to file or feature name]"
user-invocable: true
allowed-tools: Read, Glob, Grep, Write, Edit
---

When this skill is invoked:

1. **Scan the target** for hardcoded strings:
   - C# files: look for string literals in UI-facing methods
   - UXML files: look for text values not using bindings
   - ScriptableObjects: look for displayName or description fields

2. **For each hardcoded string**:
   - Assign a localization key: `[Screen].[Component].[Context]`
   - Document context for translators

3. **Create a string table CSV** at `assets/data/localization/[Feature]-en-US.csv`:
```csv
Key,English,Context,Max Length
MainMenu.StartButton.Label,Start Game,Primary game start button,20
MainMenu.SettingsButton.Label,Settings,Opens settings menu,15
```

4. **Replace hardcoded strings** in code with localization key lookups:
   - Unity Localization package: `LocalizationSettings.StringDatabase.GetLocalizedString(key)`
   - Or binding via UI Toolkit data binding

5. **Output report**:
```
## Localization Audit: [Feature]
- Strings found: X
- Already localized: X
- Newly assigned keys: X
- Files modified: [list]
```

6. **Ask for approval** before modifying any files.
