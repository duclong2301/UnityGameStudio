#!/bin/bash
# PreToolUse(Bash) hook: Block force pushes

INPUT=$(cat)

if echo "$INPUT" | grep -qE '"git push.*--force|git push.*-f '; then
    echo "BLOCKED: Force push is not allowed. Use --force-with-lease if necessary." >&2
    exit 1
fi

exit 0
