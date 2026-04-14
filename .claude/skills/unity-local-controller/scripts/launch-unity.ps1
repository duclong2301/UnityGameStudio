#!/usr/bin/env pwsh
# Launch Unity Editor with validation and error handling
# Usage: launch-unity.ps1 -Version <version> [-ProjectPath <path>]

param(
    [Parameter(Mandatory=$true)]
    [string]$Version,
    
    [Parameter(Mandatory=$false)]
    [string]$ProjectPath
)

$ErrorActionPreference = 'Stop'

# Find Unity installation using detect-unity.ps1
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$detectScript = Join-Path $scriptDir "detect-unity.ps1"

if (-not (Test-Path $detectScript)) {
    Write-Error "Detection script not found: $detectScript"
    exit 1
}

Write-Host "Searching for Unity $Version..." -ForegroundColor Cyan

# Run detection script and find matching version
$installations = & $detectScript | ForEach-Object {
    $parts = $_ -split "`t"
    [PSCustomObject]@{
        Version = $parts[0]
        Path = $parts[1]
        Type = $parts[2]
    }
} | Group-Object Path | ForEach-Object { 
    # Take first occurrence of each unique path (deduplication)
    $_.Group[0] 
}

$match = $installations | Where-Object { $_.Version -eq $Version }

if (-not $match) {
    Write-Host "`n[ERROR] Unity $Version not found on this machine" -ForegroundColor Red
    
    # Show similar versions
    $majorMinor = ($Version -split '\.')[0..1] -join '.'
    $similar = $installations | Where-Object { $_.Version -like "$majorMinor.*" }
    
    if ($similar) {
        Write-Host "`nSimilar versions installed:" -ForegroundColor Yellow
        $similar | ForEach-Object {
            Write-Host "   - $($_.Version) ($($_.Type))" -ForegroundColor Yellow
        }
    }
    
    Write-Host "`nTo install Unity ${Version}:" -ForegroundColor Cyan
    Write-Host "   1. Open Unity Hub" -ForegroundColor White
    Write-Host "   2. Go to Installs -> Add" -ForegroundColor White
    Write-Host "   3. Select version $Version" -ForegroundColor White
    
    exit 1
}

$unityExe = $match.Path

# Validate Unity.exe exists and is executable
if (-not (Test-Path $unityExe -PathType Leaf)) {
    Write-Error "Unity executable not found: $unityExe"
    exit 1
}

Write-Host "[OK] Found: $($match.Type) installation" -ForegroundColor Green
Write-Host "   Path: $unityExe" -ForegroundColor Gray

# If project path provided, validate it
if ($ProjectPath) {
    if (-not (Test-Path $ProjectPath -PathType Container)) {
        Write-Error "Project path does not exist: $ProjectPath"
        exit 1
    }
    
    $assetsPath = Join-Path $ProjectPath "Assets"
    $settingsPath = Join-Path $ProjectPath "ProjectSettings"
    
    if (-not (Test-Path $assetsPath -PathType Container)) {
        Write-Error "Not a valid Unity project (Assets/ folder missing): $ProjectPath"
        exit 1
    }
    
    if (-not (Test-Path $settingsPath -PathType Container)) {
        Write-Error "Not a valid Unity project (ProjectSettings/ folder missing): $ProjectPath"
        exit 1
    }
    
    Write-Host "[OK] Validated project path: $ProjectPath" -ForegroundColor Green
    Write-Host "`nLaunching Unity $Version with project..." -ForegroundColor Cyan
    
    # Launch with project
    & $unityExe -projectPath $ProjectPath
}
else {
    Write-Host "`nLaunching Unity $Version..." -ForegroundColor Cyan
    
    # Launch without project (opens project selector)
    & $unityExe
}

# Note: We don't wait for Unity to exit, it runs as a separate process
Write-Host "[OK] Unity launched successfully" -ForegroundColor Green
exit 0
