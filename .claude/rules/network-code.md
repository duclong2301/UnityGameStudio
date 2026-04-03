---
paths:
  - "src/Network/**/*.cs"
  - "src/Multiplayer/**/*.cs"
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
