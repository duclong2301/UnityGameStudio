#!/bin/bash
# Status line script for Claude Code — shows key project state

BRANCH=$(git rev-parse --abbrev-ref HEAD 2>/dev/null || echo "no-git")
DIRTY=$(git status --short 2>/dev/null | wc -l | tr -d ' ')

SPRINT=""
LATEST_SPRINT=$(ls -t production/sprints/sprint-*.md 2>/dev/null | head -1)
if [ -n "$LATEST_SPRINT" ]; then
    SPRINT=" | $(basename "$LATEST_SPRINT" .md)"
fi

BUG_COUNT=0
for dir in tests production; do
    if [ -d "$dir" ]; then
        count=$(find "$dir" -name "BUG-*.md" 2>/dev/null | wc -l)
        BUG_COUNT=$((BUG_COUNT + count))
    fi
done

BUG_STR=""
if [ "$BUG_COUNT" -gt 0 ]; then
    BUG_STR=" | bugs:$BUG_COUNT"
fi

DIRTY_STR=""
if [ "$DIRTY" -gt 0 ]; then
    DIRTY_STR=" | ~$DIRTY changed"
fi

echo "🎮 Unity Studio | $BRANCH$DIRTY_STR$SPRINT$BUG_STR"
