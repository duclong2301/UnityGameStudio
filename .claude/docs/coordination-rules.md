# Agent Coordination Rules

## Studio Hierarchy

```
Tier 1 — Directors (Opus model)
  creative-director    technical-director    producer

Tier 2 — Department Leads (Sonnet model)
  game-designer        lead-programmer       art-director
  audio-director       narrative-director    qa-lead
  release-manager      localization-lead

Tier 3 — Specialists (Sonnet/Haiku model)
  gameplay-programmer  engine-programmer     ai-programmer
  network-programmer   tools-programmer      ui-programmer
  systems-designer     level-designer        economy-designer
  technical-artist     sound-designer        writer
  world-builder        ux-designer           prototyper
  performance-analyst  devops-engineer       analytics-engineer
  security-engineer    qa-tester
  accessibility-specialist  live-ops-designer  community-manager

Unity Engine Specialists (Tier 3)
  unity-specialist
    unity-dots-specialist
    unity-shader-specialist
    unity-addressables-specialist
    unity-ui-specialist
```

## Delegation Rules

1. **Vertical delegation** — directors delegate to leads, leads delegate to specialists
2. **Horizontal consultation** — same-tier agents can consult each other, but cannot make binding cross-domain decisions
3. **Conflict resolution** — disagreements escalate to the shared parent director
4. **Change propagation** — cross-department changes are coordinated by `producer`
5. **Domain boundaries** — agents do not modify files outside their domain without explicit delegation

## Escalation Paths

| Conflict Type | Escalate To |
|---|---|
| Creative vs. Design | creative-director |
| Technical vs. Architecture | technical-director |
| Schedule vs. Scope | producer |
| Design vs. Technical | producer (coordinates) |
| Unity-specific vs. Architecture | technical-director |

## Agent Invocation

Use the Task tool to delegate to sub-agents. Always provide:
- Full context (what needs to be done, why, constraints)
- Relevant file paths
- Design document references
- Performance requirements (if applicable)

Launch independent sub-agent tasks in parallel when possible.
