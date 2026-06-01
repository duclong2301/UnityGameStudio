# Multi-Mode & Independent-Module Architecture

> Shared stance for `game-designer`, `lead-programmer`, and `technical-director`.
> When designing a new project or system, default to this structure. Adapt the wording to your role's altitude (design / code / binding decision).

**Default new projects to a multi-mode, module-plugin architecture (SOLID).** Treat core gameplay as one module among many — never assume a single game loop is the whole app.

Three layers, never conflated:

- **App Shell (Core)** — owns the mode contract + shared services (scene flow, save, audio, economy, UI framework). Knows nothing about any specific game.
- **Game Module** — one independent game under `Modules/<Name>/`, implements the mode contract, depends on Core only. Modules never reference each other.
- **Sub-Mode** — a variant *inside* one module (different win/lose rules) behind its own focused interface (e.g. `IBubbleRule`). The shell does not know about it.

The mode contract (`IGameMode`: `Init / Tick / CheckResult / Cleanup`) lives in Core and must be abstract enough to carry an unrelated game.

SOLID seam:
- **DIP** — Core depends on the abstraction (`IGameMode`), not on any concrete module.
- **OCP** — adding a mode = new folder + implement the contract + register it; never modify Core or sibling modules.
- **LSP** — every module is substitutable behind the contract; the shell runs them identically.
- **ISP** — keep the mode contract small; module-internal concerns use their own focused interfaces.

Enforcement: split `Core/` vs `Modules/<Name>/` and use Assembly Definitions so the dependency rule is compiler-enforced — `Core` references no module; each module references only `Core`; modules never reference each other.

**KISS guardrail**: this is the default *thinking*, not mandatory over-engineering. Don't extract Core into a standalone package/repo unless cross-project reuse (reskin) is an explicit goal; don't pre-build modules that aren't planned — just keep the seam clean so they can be added without rework.
