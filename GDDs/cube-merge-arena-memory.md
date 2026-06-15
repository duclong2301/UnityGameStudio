# Cube Merge Arena - Design Memory

**Author**: Codex + Long Tran
**Date**: 2026-06-05
**Status**: Draft
**Version**: 0.1

---

## Overview

### Working Title
Cube Merge Arena

### Elevator Pitch
Cube Merge Arena is a fast 3D top-down arena game that combines snake movement, 2048-style merging, and .io survival competition. The player controls a chain of numbered cubes, collects smaller cubes, merges matching values, eats weaker opponents, avoids stronger threats, and climbs a real-time leaderboard.

### Genre
Casual arcade / .io arena / merge action / survival.

### Target Platforms
- Primary: WebGL and Android.
- Secondary: iOS and desktop browser.

### Target Session Length
- MVP target: 3 to 5 minutes per run.
- Restart time: less than 3 seconds from game over to next run.

### Design Goal
Build a prototype that is immediately readable in motion, easy to control on PC and mobile, and expandable into multiplayer-like bot competition before considering real networking.

## Pillars

### 1. Instant Readability
The player must understand the current threat and opportunity state at a glance:
- My biggest cube value.
- Nearby edible cubes and enemies.
- Nearby dangerous enemies.
- Current leaderboard rank.
- Boost availability.

### 2. Satisfying Merge Growth
Every merge should feel rewarding through visual scale pop, sound, particles, and score feedback. The chain should evolve from weak and small to heavy and dominant.

### 3. Risk-Based Chasing
The player should constantly decide whether to collect safe pickups, attack smaller opponents, contest power-ups, or flee stronger chains.

### 4. Fast .io Flow
The game should feel like a live arena even in single-player MVP. Bots, leaderboard changes, pickups, and combat must create constant movement.

### 5. Mobile-First Simplicity
Controls and UI must work on touch with minimal buttons. The core experience should not depend on precise keyboard input.

## Core Experience

### Core Fantasy
The player becomes a growing chain of numbered cubes that snowballs from tiny prey into the largest predator in the arena.

### Player Emotion Targets
- Early game: quick collection, curiosity, low pressure.
- Mid game: opportunistic attacks, small escapes, tactical boost usage.
- Late game: dominance, leaderboard tension, fear of losing a large chain.

### Main Reference
Cubes 2048.io: snake-like movement, 2048 merging, eating smaller numbered cubes, boost, arena leaderboard.

This design should be treated as an original implementation blueprint, not a clone of UI, branding, assets, or exact content.

## Core Mechanics

### Chain Movement
The player controls the head cube. Body cubes follow the path of the head with fixed spacing.

Rules:
- The head moves continuously.
- Direction is controlled by mouse position on PC or virtual joystick/drag on mobile.
- Body cubes follow previous positions of the head/segments.
- Segment spacing should remain stable at different speeds.
- Turning should feel smooth but responsive.

Initial tuning:
- Base speed: 7 units/second.
- Boost speed: 12.5 units/second.
- Segment spacing: 1.15 units.
- Turn responsiveness: high, but not instant snapping.

### Cube Values
Every cube segment has a power-of-two value:
- 2
- 4
- 8
- 16
- 32
- 64
- 128
- 256
- 512
- 1024+

Display:
- Large number on top face.
- Color can map by value, but readability matters more than a strict rainbow.
- Higher values may increase cube scale slightly, within readable limits.

### Pickup Collection
Free cubes spawn across the arena. When the player touches a pickup:
- Pickup disappears.
- A new cube segment with that value is added to the player's chain.
- Score increases by pickup value.
- Merge check runs.

Initial tuning:
- Pickup radius: 1.2 units.
- Max active pickups: 220.
- Avoid spawning pickups directly under players or bots.

### Merge Logic
When two cubes in the same chain have the same value, they can merge into one cube with double value.

MVP merge rule:
- After adding a pickup or eating a cube, scan the chain from tail to head.
- If two adjacent cubes have equal value, remove one and double the other.
- Continue until no adjacent equal pairs remain.

Possible later rule:
- Allow same-value cubes that physically bump into each other to merge, even if not adjacent in list.

Merge feedback:
- Short scale pop.
- Small particle burst.
- Floating text: "32 -> 64".
- Higher-pitch sound for higher values.

### Eating Opponents
A cube can eat another cube if its value is strictly greater than the target cube value.

Combat rule:
- If player cube value > enemy cube value, enemy segment is removed and player gains that value.
- If enemy cube value > player cube value, player segment is removed.
- Equal values do not eat each other; they bump or slide.

Head danger:
- If the player's head is eaten, the run ends.
- If a body segment is eaten, the player survives but loses power.

MVP simplification:
- Use sphere trigger checks around segments.
- Resolve only one eat interaction per segment per short cooldown to avoid chain explosions.

### Score
Score is the sum of all cube values in the chain.

Additional stats:
- Biggest cube reached.
- Cubes collected.
- Segments eaten.
- Opponents eliminated.
- Survival time.

### Boost
Boost lets the player chase or escape.

Rules:
- Hold left mouse button or Space on PC.
- Press and hold boost button on mobile.
- Boost duration: 2 seconds.
- Cooldown: 6 seconds.
- Boost cannot be reused until cooldown completes.

Initial tuning:
- Base speed multiplier: 1.8x.
- Optional later cost: consume a small amount of score or shed low-value tail segments.

Feedback:
- Circular cooldown UI.
- Trail streaks.
- Slight camera zoom/FOV change.
- Soft whoosh sound.

### Hazards
MVP hazard: division tile.

Rules:
- A division tile reduces touched cube value by one tier.
- Example: 64 becomes 32.
- Minimum value remains 2.
- If several segments pass through, apply limited cooldown per chain to avoid excessive punishment.

Purpose:
- Create route planning.
- Punish careless boost.
- Add map texture without complex obstacles.

## Arena Design

### Shape and Size
MVP arena:
- Square or rounded-square arena.
- Size: 120 x 120 Unity units.
- Soft boundary walls that push players back.

### Zones
Outer zone:
- Safer.
- More 2 and 4 pickups.
- Fewer bots.

Middle zone:
- More 8, 16, and 32 pickups.
- More bot traffic.
- More power-up contests.

Center zone:
- Highest reward density.
- Highest danger.
- Best place for late-game fights.

### Pickup Spawn Table
Base table:
- 70% value 2
- 18% value 4
- 8% value 8
- 3% value 16
- 1% value 32

Dynamic adjustment:
- After 60 seconds, slightly increase 8/16/32 chance.
- If arena population is low, increase low-value pickup spawn rate.

### Power-Up Spawn
Initial tuning:
- Spawn interval: 12 to 18 seconds.
- Max active power-ups: 4.
- Spawn away from arena edges.
- Prefer contested middle areas.

## Power-Ups

### Magnet
Effect:
- Pulls free pickups toward the player.
- Does not pull enemy segments.

Tuning:
- Radius: 8 units.
- Duration: 8 seconds.

### Shield
Effect:
- Blocks one enemy eat interaction against the player's segment.
- Shield breaks after one block or duration ends.

Tuning:
- Duration: 10 seconds.

### Sprint
Effect:
- Instantly resets boost cooldown.
- Grants stronger boost.

Tuning:
- Sprint speed multiplier: 2.2x.
- Duration: 2 seconds.

### Merge Frenzy
Effect:
- Merge checks happen faster and can chain more aggressively.
- Visual merge feedback is amplified.

Tuning:
- Duration: 8 seconds.

## Bot Design

### MVP Bot Count
- Start with 10 bots.
- Scale to 15 if performance and readability are stable.

### Bot Personalities
Aggressive:
- Chases weaker opponents.
- Takes higher risks near center.

Collector:
- Prioritizes pickups and power-ups.
- Avoids combat unless clearly stronger.

Coward:
- Flees from larger chains early.
- Stays near outer zone.

Balanced:
- Blends collection, chase, and flee behavior.

### Bot Steering
Bots do not need full pathfinding for MVP.

Use weighted steering:
- Seek target pickup.
- Seek weaker opponent segment.
- Flee stronger enemy within danger radius.
- Avoid boundary.
- Avoid division hazards.
- Contest power-up if nearby and not dangerous.

### Bot Target Priority
1. Flee immediate danger.
2. Eat nearby weaker segment.
3. Collect nearby high-value pickup.
4. Move toward safer area or center depending on personality.

## UI Design

## In-Game HUD

### Top Center: Score
Display:
- Large score text: "SCORE 1024".
- Optional subtext: biggest cube value.

Behavior:
- Animates upward on pickup/merge.
- Brief flash on major merge.

### Top Right: Leaderboard
Display top 5:
- #1 YOU
- BOT 512
- BOT 256
- BOT 128
- BOT 64

Rules:
- Sort by total score.
- Highlight player row.
- Use compact typography for mobile.

### Bottom Right: Boost Button
Mobile:
- Large circular button.
- Label: "BOOST".
- Cooldown ring around button.

PC:
- Same UI visible or smaller hint.
- Reacts to Space / left mouse hold.

### Bottom Left: Radar / Minimap
MVP optional, but useful for readability.

Display:
- Player at center.
- Nearby large threats as red dots.
- Power-ups as yellow dots.
- High-value pickups as small white/gold dots.

### Floating Feedback
Events:
- Pickup: "+8".
- Merge: "32 -> 64".
- Eat: "+128".
- Danger: screen edge red indicator.

Rules:
- Keep text short.
- Avoid clutter.
- Fade quickly.

## Menu UI

### Main Screen
Required:
- Game title.
- Play button.
- Best score.
- Player name field.
- Skin preview.

Optional:
- Daily reward.
- Quests.
- Settings.

### Skin Selector
MVP:
- Color swatches for cube material.
- Locked skins can be shown but disabled.

Later:
- Trails.
- Number font styles.
- Merge particle styles.

### Post-Game Screen
Display:
- Final rank.
- Score.
- Biggest cube.
- Cubes collected.
- Segments eaten.
- Time survived.
- Coins earned.

Buttons:
- Play Again.
- Main Menu.

## Visual Direction

### Camera
Use a top-down 3D camera:
- Orthographic is recommended for readability.
- Perspective with low angle can be tested if it improves energy.
- Follow the head with smoothing.
- Slightly lead the camera in movement direction.
- Zoom out with chain length.

### Art Style
Vibrant stylized 3D:
- Glossy cube materials.
- Soft shadows.
- Clean arena floor.
- Subtle grid for motion.
- Clear value labels.

Avoid:
- Visual noise.
- Tiny unreadable numbers.
- Overly dark arena.
- Brand references from external games.

### Color Language
Player:
- Cyan or blue-green by default.

Opponents:
- Orange, pink, green, purple, yellow.

Danger:
- Red highlight and warning indicators.

Power-ups:
- Magnet: blue glow.
- Shield: cyan shield ring.
- Sprint: yellow/orange lightning.
- Merge Frenzy: purple/gold sparkle.

## Audio Direction

### Required Sounds
- Pickup click.
- Merge pop.
- Boost whoosh.
- Segment eaten.
- Warning tick for danger.
- Game over stinger.

### Audio Rules
- Sounds must be short.
- Merge pitch should rise with value.
- Avoid constant noisy loops.
- Boost loop should be subtle.

## Unity Implementation Plan

## Scenes

### GameScene
Contains:
- Arena.
- Camera rig.
- GameManager.
- SpawnManager.
- UI canvas.
- Player prefab.
- Bot prefabs.

### MenuScene
Optional for MVP. Menu can initially be in GameScene with state panels.

## Core Prefabs

### PlayerSnake.prefab
Components:
- PlayerController
- MovementMotor
- SnakeChain
- BoostController
- Collision/eat trigger manager

### BotSnake.prefab
Components:
- BotController
- MovementMotor
- SnakeChain
- Collision/eat trigger manager

### CubeSegment.prefab
Components:
- MeshRenderer
- Collider
- CubeEntity
- Number label/TextMeshPro

### PickupCube.prefab
Components:
- MeshRenderer
- Trigger collider
- PickupCube
- Value label

### PowerUp.prefab
Components:
- Trigger collider
- PowerUpPickup
- Icon/mesh
- Glow effect

### HUDCanvas.prefab
Components:
- UIHudController
- Score display
- Leaderboard display
- Boost button
- Radar/minimap
- Floating text layer

## Core Scripts

### GameManager
Responsibilities:
- Own match state.
- Start run.
- End run.
- Pause/resume.
- Dispatch match events.

States:
- Boot
- Menu
- Countdown
- Playing
- GameOver

### ArenaManager
Responsibilities:
- Store arena bounds.
- Provide random spawn points.
- Enforce boundary pushback.
- Define zones.

### MovementMotor
Responsibilities:
- Move chain head.
- Apply speed/boost modifiers.
- Smooth rotation.
- Provide current velocity for camera and VFX.

### PlayerController
Responsibilities:
- Read mouse/touch/keyboard input.
- Convert input into desired direction.
- Request boost.

### BotController
Responsibilities:
- Choose target.
- Calculate steering intent.
- Request boost when chasing or escaping.

### SnakeChain
Responsibilities:
- Store ordered cube segments.
- Add segment.
- Remove segment.
- Track score.
- Maintain spacing.
- Trigger merge checks.

### CubeEntity
Responsibilities:
- Store value.
- Store owner chain.
- Update material/color.
- Update number label.

### MergeSystem
Responsibilities:
- Resolve adjacent equal-value merges.
- Apply merge VFX/audio.
- Notify score and UI.

Can initially live inside SnakeChain, then split when complexity grows.

### CollisionEatSystem
Responsibilities:
- Detect cube-vs-cube interactions.
- Compare values.
- Resolve eating.
- Apply shield/damage rules.

### SpawnManager
Responsibilities:
- Maintain pickup population.
- Spawn power-ups.
- Spawn bots.
- Recycle destroyed objects if object pooling is used.

### LeaderboardSystem
Responsibilities:
- Track all chains.
- Sort by score.
- Publish top entries to UI.

### UIHudController
Responsibilities:
- Display score.
- Display leaderboard.
- Display boost cooldown.
- Display warning indicators.
- Display match timer/stats.

## Data Assets

Use ScriptableObjects for balancing:

### GameBalanceConfig
Fields:
- arenaSize
- playerBaseSpeed
- boostSpeedMultiplier
- boostDuration
- boostCooldown
- segmentSpacing
- pickupMaxCount
- botStartCount
- matchTargetDuration

### CubeValueConfig
Fields:
- value
- color
- scale
- scoreWeight
- labelStyle

### PowerUpConfig
Fields:
- type
- duration
- cooldown
- icon
- material
- effectPrefab

### BotPersonalityConfig
Fields:
- aggression
- collectBias
- fleeDistance
- centerBias
- boostUsageChance

## MVP Milestones

### Milestone 1: Movement Prototype
- Player cube moves in arena.
- Camera follows.
- PC input works.
- Mobile drag/joystick placeholder works.

### Milestone 2: Chain Prototype
- Body segments follow head.
- Segment spacing is stable.
- Add segments by debug key or pickup.

### Milestone 3: Pickups and Merge
- Pickups spawn.
- Player collects pickups.
- Adjacent equal values merge.
- Score UI updates.

### Milestone 4: Bots and Leaderboard
- Bots spawn and move.
- Bots collect pickups.
- Leaderboard updates in real time.

### Milestone 5: Combat
- Player and bots eat smaller segments.
- Head loss triggers game over.
- Segment loss is readable.

### Milestone 6: Boost and Power-Ups
- Boost with cooldown.
- Magnet, Shield, Sprint, Merge Frenzy.
- Division hazard.

### Milestone 7: UI Flow
- Main menu.
- In-game HUD.
- Post-game summary.
- Restart flow.

### Milestone 8: Polish and Balance
- VFX and audio.
- Better bot personalities.
- Spawn tuning.
- WebGL/Android performance pass.

## Initial Balance Values

| Setting | Value |
| --- | --- |
| Arena size | 120 x 120 |
| Player base speed | 7 |
| Boost speed multiplier | 1.8 |
| Boost duration | 2s |
| Boost cooldown | 6s |
| Segment spacing | 1.15 |
| Pickup radius | 1.2 |
| Eat check radius | 1.0 |
| Start bots | 10 |
| Max pickups | 220 |
| Power-up spawn interval | 12-18s |
| Target run length | 3-5 minutes |

## Technical Risks

### Segment Follow Stability
Long chains may jitter or compress during fast turns. Use path history sampling rather than direct follow-transform chasing if needed.

### Collision Explosion
Many cube segments colliding at once can cause chaotic removals. Add short per-segment interaction cooldowns and deterministic resolution order.

### Number Label Readability
Labels must remain readable from camera distance. Use TextMeshPro or texture atlases with high contrast.

### Bot Fairness
Bots must feel alive without feeling like they cheat. Keep bot reaction delay and imperfect targeting.

### Mobile Performance
Many segments, labels, shadows, and particles can hurt mobile. Use pooling, simple materials, and limited real-time lights.

## Open Questions

- Should equal-value enemy cubes bounce, merge, or do nothing on contact?
- Should boost consume score/segments, or only use cooldown?
- Should the game have a hard time limit or endless survival?
- Should the arena be square, circular, or rounded square?
- Should progression be only cosmetic, or include light passive upgrades?
- Should the first prototype use orthographic camera or perspective camera?
- Should bots respawn during a match or only at match start?
- What is the final theme: abstract arcade, toy blocks, sci-fi arena, or food/candy style?

## Next Discussion Topics

1. Finalize the MVP feature boundary.
2. Decide camera style and arena shape.
3. Decide exact UI layout for PC and mobile.
4. Define prefab hierarchy and script responsibilities in Unity.
5. Build the first playable prototype milestone.
