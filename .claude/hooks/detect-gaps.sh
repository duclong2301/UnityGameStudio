#!/bin/bash
# SessionStart hook: Detect missing project documentation gaps

GAPS=()

# Check for game design document
if [ ! -f "design/gdd/game-design-document.md" ] && [ ! -f "design/gdd/GDD.md" ]; then
    GAPS+=("Missing: design/gdd/ — No Game Design Document found. Run /start to create one.")
fi

# Check for pillars document
if [ ! -f "design/pillars.md" ]; then
    GAPS+=("Missing: design/pillars.md — No design pillars defined.")
fi

# Check for coding standards
if [ ! -f ".claude/docs/coding-standards.md" ]; then
    GAPS+=("Missing: .claude/docs/coding-standards.md — No coding standards defined.")
fi

# Check for active sprint
if ! ls production/sprints/sprint-*.md 1>/dev/null 2>&1; then
    GAPS+=("Missing: production/sprints/ — No sprint plan found. Run /sprint-plan to create one.")
fi

# Check for Unity version reference
if [ ! -f "docs/engine-reference/unity/VERSION.md" ]; then
    GAPS+=("Missing: docs/engine-reference/unity/VERSION.md — Unity version not documented.")
fi

if [ ${#GAPS[@]} -gt 0 ]; then
    echo ""
    echo "=== PROJECT GAPS DETECTED ==="
    for gap in "${GAPS[@]}"; do
        echo "  ⚠ $gap"
    done
    echo "=============================="
fi

exit 0
