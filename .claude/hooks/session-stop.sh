#!/bin/bash
# SessionStop hook: Save session state before Claude exits

STATE_DIR="production/session-state"
STATE_FILE="$STATE_DIR/active.md"

mkdir -p "$STATE_DIR"

cat > "$STATE_FILE" << EOF
# Session State — $(date -u +"%Y-%m-%dT%H:%M:%SZ")

## Branch
$(git rev-parse --abbrev-ref HEAD 2>/dev/null || echo "unknown")

## Last 5 Commits
$(git log --oneline -5 2>/dev/null || echo "No commits")

## Modified Files
$(git status --short 2>/dev/null || echo "No changes")

## Notes
<!-- The agent should update this section before stopping -->
EOF

echo "Session state saved to $STATE_FILE"
exit 0
