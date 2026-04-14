#!/usr/bin/env bash
# Detect installed Unity editors on this Windows machine.
# Output: one line per install  "<version>\t<absolute path to Unity.exe>"

set -u

emit() {
    local version="$1"
    local exe="$2"
    if [[ -f "$exe" ]]; then
        printf '%s\t%s\n' "$version" "$exe"
    fi
}

scan_dir() {
    local root="$1"
    [[ -d "$root" ]] || return 0
    for entry in "$root"/*/; do
        [[ -d "$entry" ]] || continue
        local version
        version=$(basename "$entry")
        emit "$version" "${entry}Editor/Unity.exe"
    done
}

# 1. Detected custom install root
scan_dir "/e/unity"

# 2. Unity Hub default on C:
scan_dir "/c/Program Files/Unity/Hub/Editor"

# 3. Read Unity Hub secondary install path (JSON string)
hub_config="$APPDATA/UnityHub/secondaryInstallPath.json"
if [[ -f "$hub_config" ]]; then
    # File contains a single quoted JSON string, e.g. "E:\\unity"
    secondary=$(cat "$hub_config" | tr -d '"' | tr -d '\r\n')
    if [[ -n "$secondary" ]]; then
        # Convert Windows path to Git-Bash path
        drive=$(echo "$secondary" | cut -c1 | tr 'A-Z' 'a-z')
        rest=$(echo "$secondary" | cut -c3- | tr '\\' '/')
        scan_dir "/${drive}${rest}"
    fi
fi
