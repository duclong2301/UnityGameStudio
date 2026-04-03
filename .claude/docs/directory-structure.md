# Directory Structure — Unity Game Studio

```
CLAUDE.md                               # Master configuration
.claude/
  settings.json                         # Hooks, permissions, safety rules
  agents/                               # Agent definitions (markdown + YAML frontmatter)
  skills/                               # Slash commands (one subdirectory per skill)
  hooks/                                # Hook scripts (bash)
  rules/                                # Path-scoped coding standards
  docs/
    coding-standards.md                 # C# and Unity coding standards
    coordination-rules.md               # Agent hierarchy and delegation
    collaboration-protocol.md           # How agents interact with the user
    context-management.md               # Context window best practices
    technical-preferences.md            # Engine, language, package preferences
    directory-structure.md              # This file
    templates/                          # Document templates (GDDs, ADRs, etc.)

src/                                    # Unity C# source code (Assets/Scripts/ equivalent)
  Core/                                 # Engine-layer: bootstrap, events, utilities
  Gameplay/                             # Gameplay systems (combat, movement, AI)
  UI/                                   # UI controllers and ViewModels
  Network/                              # Multiplayer and network code
  Audio/                                # Audio management and events
  Data/                                 # ScriptableObject definitions

assets/                                 # Art, audio, VFX, shaders, data files
  art/                                  # Textures, sprites, meshes, animations
  audio/                                # Sound effects and music
  vfx/                                  # VFX Graph and particle systems
  shaders/                              # Shader Graph assets and HLSL
  data/                                 # ScriptableObject instances and JSON/CSV data

design/                                 # Game Design Documents and references
  gdd/                                  # Game Design Documents
  narrative/                            # Story, characters, dialogue
  levels/                               # Level design documents and diagrams
  economy/                              # Economy model and balance documents
  pillars.md                            # Core game design pillars

docs/                                   # Technical documentation and ADRs
  architecture/                         # Architecture Decision Records (ADRs)
  engine-reference/
    unity/
      VERSION.md                        # Unity version, packages, build targets

tests/                                  # Unity Test Framework tests
  EditMode/                             # Edit Mode tests (pure logic)
  PlayMode/                             # Play Mode tests (MonoBehaviour, physics)

tools/                                  # Build and pipeline tools
prototypes/                             # Throwaway prototypes (isolated from src/)
production/                             # Sprint plans, milestones, release tracking
  sprints/                              # Sprint plan documents (sprint-001.md, etc.)
  milestones/                           # Milestone documents (alpha.md, beta.md, etc.)
  session-state/                        # Agent session state (auto-generated)
```
