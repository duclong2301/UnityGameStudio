---
paths:
  - "assets/shaders/**"
  - "src/Rendering/**/*.cs"
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

## Post-Processing

- Use Volume-based post-processing with blend distances
- Global Volume for baseline look; local Volumes for area-specific mood
- Disable expensive effects per-platform (no motion blur on mobile)
