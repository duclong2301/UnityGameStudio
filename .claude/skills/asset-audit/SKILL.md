---
name: asset-audit
description: "Audits Unity assets for naming convention compliance, size budget adherence, import settings correctness, and Addressables organization."
argument-hint: "[path to audit, or 'all' for full audit]"
user-invocable: true
allowed-tools: Read, Glob, Grep, Bash
---

When this skill is invoked:

1. **Scan the target path** (or all of `assets/` if 'all').
2. **Check naming conventions**:
   - Textures: `T_[Asset]_[Type]` (e.g., `T_Rock_Albedo`)
   - Materials: `M_[Asset]`
   - Prefabs: `PF_[Category]_[Name]`
   - Audio: `SFX_[Category]_[Action]` or `MUS_[Level]_[Mood]`
   - Shaders: `SG_[Category]_[Name]`
   - VFX: `VFX_[Category]_[Name]`
3. **Check import settings** (scan `.meta` files where accessible):
   - Textures: compressed? Power-of-two?
   - Audio: correct compression, load type?
4. **Output**:

```
## Asset Audit Report

### Naming Violations
| Asset | Issue | Suggested Name |
|---|---|---|

### Missing or Incorrect Import Settings
| Asset | Current Setting | Required Setting |
|---|---|---|

### Large Assets (> 2MB uncompressed)
| Asset | Size | Recommendation |
|---|---|---|

### Summary
- Total assets scanned: X
- Naming violations: X
- Import setting issues: X
- Estimated size savings: X MB
```
