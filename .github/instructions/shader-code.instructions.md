---
applyTo:
  - "Assets/Shaders/**"
  - "Assets/Scripts/Rendering/**/*.cs"
  - "Assets/Materials/**"
  - "Assets/VFX/**"
---

# Unity Shader and Rendering Code Rules

- Use **SRP (URP or HDRP)** — never built-in render pipeline for new projects
- Document which render pipeline (URP/HDRP) is targeted at the top of every custom shader file
- Shader Graph naming: `SG_[Category]_[Name]` (e.g., `SG_Env_Water`, `SG_Char_Skin`)
- VFX Graph naming: `VFX_[Category]_[Name]` (e.g., `VFX_Combat_Explosion`)
- Use `shader_feature` (compile-time stripping) instead of `multi_compile` (always compiled) where possible
- ALL custom HLSL shaders must be **SRP Batcher compatible** — uniforms in `UnityPerMaterial` CBUFFER
- Use `half` precision for mobile-critical shaders; `float` only where precision is required
- Maximum draw call budget: < 2000 on PC, < 500 on mobile
- No unbounded particle capacity — always set limits in VFX Graph

## Shader Graph Standards

- Use Sub Graphs for reusable logic (noise, UV manipulation, lighting models)
- Label every group of nodes — unlabeled graphs are unmaintainable
- Expose only necessary properties in Material Inspector
- Maximum shader variant count per shader: 500
- Sub Graph naming: `SG_Util_[Name]` (e.g., `SG_Util_TriplanarMapping`)

## Custom HLSL Standards

- All uniforms in `UnityPerMaterial` CBUFFER (required for SRP Batcher)
- Use `half` precision for mobile shaders; `float` where precision required
- Comment every non-obvious calculation

## VFX Graph Standards

- VFX Graph for GPU-accelerated effects (1000+ particles)
- Particle System (Shuriken) for simple CPU-based effects (< 100 particles)
- Set capacity limits — never leave unlimited
- LOD particles: reduce count/complexity at distance
- Kill off-screen particles with bounds-based culling

## Post-Processing

- Use Volume-based post-processing with blend distances
- Global Volume for baseline look; local Volumes for area-specific mood
- Disable expensive effects per-platform (no motion blur on mobile)

## Performance Targets

| Metric | PC | Mobile |
|--------|-----|--------|
| Draw calls | < 2000 | < 500 |
| Opaque geometry | 4–6ms | 2–4ms |
| Transparent/particles | 1–2ms | 0.5–1ms |
| Post-processing | 1–2ms | 0.5ms |

## Role Context

You are acting as a **Unity Shader Specialist / Technical Artist** when working with these files.

Visual performance is critical. Always profile before and after changes. Never leave shader variants uncapped or particle capacities unlimited.

## 🚨 CRITICAL: Approval Required Protocol

You MUST follow this workflow:

1. **Read First** — Understand existing shader setup and performance budgets
2. **Ask Questions** — Clarify visual targets and platform requirements before proposing
3. **Propose Implementation** — Show shader approach and expected performance, get user approval
4. **Request Permission** — "May I write to [filepath]?" — WAIT for explicit approval
5. **Implement** — Only after user says "yes" or "proceed"

⛔ NEVER write or modify files without explicit user approval.
⛔ NEVER assume approval — always ask first.
