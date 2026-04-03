#!/bin/bash
# PostToolUse(Write|Edit) hook: Validate Unity asset conventions after file writes

INPUT=$(cat)

# Extract file path from input
FILE_PATH=$(echo "$INPUT" | python3 -c "import sys,json; d=json.load(sys.stdin); print(d.get('filePath',''))" 2>/dev/null)

if [ -z "$FILE_PATH" ]; then
    exit 0
fi

WARNINGS=()

# Check C# files in src/ follow Unity naming conventions
if [[ "$FILE_PATH" == src/*.cs ]] || [[ "$FILE_PATH" == */src/*.cs ]]; then
    FILENAME=$(basename "$FILE_PATH" .cs)
    # PascalCase check: first char must be uppercase
    FIRST_CHAR="${FILENAME:0:1}"
    if [[ "$FIRST_CHAR" =~ [a-z] ]]; then
        WARNINGS+=("C# file '$FILENAME.cs' should use PascalCase naming.")
    fi
fi

# Warn if large binary assets are written (shouldn't happen via Claude)
if [[ "$FILE_PATH" == *.png ]] || [[ "$FILE_PATH" == *.jpg ]] || [[ "$FILE_PATH" == *.wav ]] || [[ "$FILE_PATH" == *.mp3 ]]; then
    WARNINGS+=("Binary asset '$FILE_PATH' written by agent — ensure this is intentional.")
fi

# Output warnings (non-blocking)
for warn in "${WARNINGS[@]}"; do
    echo "ASSET WARNING: $warn" >&2
done

exit 0
