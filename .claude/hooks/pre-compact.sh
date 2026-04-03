#!/bin/bash
# PreCompact hook: Save critical context before context window compaction

STATE_DIR="production/session-state"
mkdir -p "$STATE_DIR"

COMPACT_FILE="$STATE_DIR/pre-compact-$(date -u +%Y%m%dT%H%M%S).md"

cat > "$COMPACT_FILE" << EOF
# Pre-Compact State — $(date -u +"%Y-%m-%dT%H:%M:%SZ")

## Current Branch
$(git rev-parse --abbrev-ref HEAD 2>/dev/null)

## Recent Commits
$(git log --oneline -10 2>/dev/null)

## Uncommitted Changes
$(git status --short 2>/dev/null)

## Active Sprint
$(ls -t production/sprints/sprint-*.md 2>/dev/null | head -1 | xargs basename 2>/dev/null || echo "None")
EOF

echo "Pre-compact state saved to $COMPACT_FILE"
exit 0
