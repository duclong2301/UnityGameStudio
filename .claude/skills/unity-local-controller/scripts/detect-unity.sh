#!/usr/bin/env bash
# Detect installed Unity editors on Windows (Git Bash compatible)
# Output: one line per install  "<version>\t<absolute path to Unity.exe>\t<install type>"

set -u

emit() {
    local version="$1"
    local exe="$2"
    local install_type="$3"
    if [[ -f "$exe" ]]; then
        printf '%s\t%s\t%s\n' "$version" "$exe" "$install_type"
    fi
}

scan_dir() {
    local root="$1"
    local install_type="$2"
    [[ -d "$root" ]] || return 0
    for entry in "$root"/*/; do
        [[ -d "$entry" ]] || continue
        local version
        version=$(basename "$entry")
        emit "$version" "${entry}Editor/Unity.exe" "$install_type"
    done
}

# 1. Custom install location (detected on this machine)
scan_dir "/e/unity" "Custom"

# 2. Unity Hub default
scan_dir "/c/Program Files/Unity/Hub/Editor" "Hub Default"

# 3. Read Unity Hub secondary install path (JSON string)
hub_config="$APPDATA/UnityHub/secondaryInstallPath.json"
if [[ -f "$hub_config" ]]; then
    # File contains a single quoted JSON string, e.g. "E:\\unity"
    secondary=$(cat "$hub_config" | tr -d '"' | tr -d '\r\n')
    if [[ -n "$secondary" ]]; then
        # Convert Windows path to Git-Bash path
        drive=$(echo "$secondary" | cut -c1 | tr 'A-Z' 'a-z')
        rest=$(echo "$secondary" | cut -c3- | tr '\\' '/')
        scan_dir "/${drive}${rest}" "Secondary"
    fi
fi

# 4. Scan Unity Hub editors.json for additional locations (Unity Hub 3.x)
editors_json="$APPDATA/UnityHub/editors.json"
if [[ -f "$editors_json" ]]; then
    # For bash, we'll skip complex JSON parsing and rely on the directory scans above
    # Full JSON parsing would require jq which may not be available
    :
fi
