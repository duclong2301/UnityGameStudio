#!/usr/bin/env pwsh
# Detect installed Unity editors on Windows
# Output: one line per install  "<version>`t<absolute path to Unity.exe>`t<install type>"

$ErrorActionPreference = 'SilentlyContinue'

function Emit-Unity {
    param(
        [string]$Version,
        [string]$ExePath,
        [string]$InstallType
    )
    
    if (Test-Path $ExePath -PathType Leaf) {
        Write-Output "$Version`t$ExePath`t$InstallType"
    }
}

function Scan-UnityDirectory {
    param(
        [string]$RootPath,
        [string]$InstallType
    )
    
    if (-not (Test-Path $RootPath -PathType Container)) {
        return
    }
    
    Get-ChildItem -Path $RootPath -Directory | ForEach-Object {
        $version = $_.Name
        $unityExe = Join-Path $_.FullName "Editor\Unity.exe"
        Emit-Unity -Version $version -ExePath $unityExe -InstallType $InstallType
    }
}

# 1. Custom install location (detected on this machine)
Scan-UnityDirectory -RootPath "E:\unity" -InstallType "Custom"

# 2. Unity Hub default installation path
Scan-UnityDirectory -RootPath "C:\Program Files\Unity\Hub\Editor" -InstallType "Hub Default"

# 3. Secondary install path from Unity Hub configuration
$hubConfigPath = Join-Path $env:APPDATA "UnityHub\secondaryInstallPath.json"
if (Test-Path $hubConfigPath -PathType Leaf) {
    try {
        $secondaryPath = Get-Content $hubConfigPath -Raw | ConvertFrom-Json
        if ($secondaryPath -and (Test-Path $secondaryPath -PathType Container)) {
            Scan-UnityDirectory -RootPath $secondaryPath -InstallType "Secondary"
        }
    }
    catch {
        # Ignore JSON parse errors
    }
}

# 4. Scan Unity Hub editors.json for additional locations (Unity Hub 3.x)
$editorsJsonPath = Join-Path $env:APPDATA "UnityHub\editors.json"
if (Test-Path $editorsJsonPath -PathType Leaf) {
    try {
        $editors = Get-Content $editorsJsonPath -Raw | ConvertFrom-Json
        foreach ($editor in $editors) {
            if ($editor.location -and (Test-Path $editor.location -PathType Leaf)) {
                $version = $editor.version
                if (-not $version) {
                    # Extract version from path if not in JSON
                    $version = Split-Path (Split-Path (Split-Path $editor.location)) -Leaf
                }
                Emit-Unity -Version $version -ExePath $editor.location -InstallType "Hub Registered"
            }
        }
    }
    catch {
        # Ignore JSON parse errors
    }
}
