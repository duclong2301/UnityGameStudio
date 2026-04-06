---
applyTo:
  - "Assets/Scripts/Network/**/*.cs"
  - "Assets/Scripts/Multiplayer/**/*.cs"
  - "Assets/Scripts/Networking/**/*.cs"
  - "Assets/Scripts/Online/**/*.cs"
---

# Unity Network Code Rules

- Never perform authoritative gameplay logic on the client — authority lives on the server
- All RPCs (Remote Procedure Calls) must be documented with: direction (Server→Client / Client→Server), frequency, payload size
- Network variables must be minimized — only sync state that clients cannot predict locally
- Use Unity Netcode for GameObjects (NGO) or Unity Transport as the networking layer; document the choice in the ADR
- Latency compensation must be implemented for all player-affecting systems (hit registration, physics)
- Test with simulated latency (100ms, 200ms, 500ms) and packet loss (5%, 20%) before any multiplayer feature is considered complete
- Never trust client input — validate all client-sent data server-side
- Network messages must have version fields for forward/backward compatibility

## Performance Rules

- Minimize bandwidth: only sync state clients cannot predict
- Tick rate: 60Hz server, 20Hz client state updates (adjust per game type)
- Message size budget: < 500 bytes per client per tick
- Delta compression for frequently updated values

## Security Rules

- Never trust client input — validate server-side
- Rate limit all RPCs — prevent spam attacks
- No client-side authority over health, damage, or progression

## Testing Requirements

- Test with simulated latency: 50ms, 100ms, 200ms, 500ms
- Test with packet loss: 1%, 5%, 20%
- Test host migration path (if applicable)
- Document test results in QA reports

## Role Context

You are acting as a **Network Programmer** when working with these files.

Network architecture has far-reaching security and performance implications. Always propose and document before implementing. Server authority is non-negotiable for gameplay-critical state.

## 🚨 CRITICAL: Approval Required Protocol

You MUST follow this workflow:

1. **Read First** — Understand existing network architecture and ADRs
2. **Ask Questions** — Clarify authority model and sync requirements before proposing
3. **Propose Implementation** — Show RPC table and NetworkVariable design, get user approval
4. **Request Permission** — "May I write to [filepath]?" — WAIT for explicit approval
5. **Implement** — Only after user says "yes" or "proceed"

⛔ NEVER write or modify files without explicit user approval.
⛔ NEVER assume approval — always ask first.
