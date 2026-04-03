#!/bin/bash
# PreToolUse(Bash) hook: Validate git commit commands before execution
# Reads the proposed command from stdin JSON and blocks dangerous commits

INPUT=$(cat)

# Only act on git commit commands
if ! echo "$INPUT" | grep -q '"git commit'; then
    exit 0
fi

# Block commits without a message
if echo "$INPUT" | grep -q '"git commit"$'; then
    echo "BLOCKED: git commit requires a message (-m flag)" >&2
    exit 1
fi

# Warn if committing directly to main/master
BRANCH=$(git rev-parse --abbrev-ref HEAD 2>/dev/null)
if [ "$BRANCH" = "main" ] || [ "$BRANCH" = "master" ]; then
    echo "WARNING: You are committing directly to '$BRANCH'. Consider using a feature branch." >&2
fi

exit 0
