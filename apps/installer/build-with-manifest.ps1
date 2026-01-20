# Build mcv-installer and embed UAC manifest

param(
    [Parameter(Mandatory=$false)]
    [switch]$Release
)

$ErrorActionPreference = "Stop"

$buildType = if ($Release) { "release" } else { "debug" }
$releaseFlag = if ($Release) { " --release" } else { "" }

Write-Host "Building mcv-installer ($buildType)..."

# Move to project root
Set-Location (Join-Path $PSScriptRoot "..\..")

# Build
$buildCmd = "cargo build -p mcv-installer$releaseFlag"
Write-Host "> $buildCmd"
Invoke-Expression $buildCmd

if ($LASTEXITCODE -ne 0) {
    Write-Error "Build failed"
    exit 1
}

Write-Host ""
Write-Host "[OK] Build completed"
Write-Host ""

# Embed manifest
$exePath = "target\$buildType\mcv-installer.exe"
$manifestPath = "apps\installer\src-tauri\mcv-installer.exe.manifest"

# Find mt.exe
$mtPaths = @(
    "C:\Program Files (x86)\Windows Kits\10\bin\10.0.22621.0\x64\mt.exe",
    "C:\Program Files (x86)\Windows Kits\10\bin\10.0.19041.0\x64\mt.exe",
    "C:\Program Files (x86)\Windows Kits\10\bin\x64\mt.exe"
)

$mtExe = $null
foreach ($path in $mtPaths) {
    if (Test-Path $path) {
        $mtExe = $path
        break
    }
}

if (-not $mtExe) {
    $found = Get-ChildItem "C:\Program Files (x86)\Windows Kits\10\bin" -Filter "mt.exe" -Recurse -ErrorAction SilentlyContinue | Where-Object { $_.FullName -match "x64" } | Select-Object -First 1
    if ($found) {
        $mtExe = $found.FullName
    }
}

if (-not $mtExe) {
    Write-Error "mt.exe not found. Please install Windows SDK."
    exit 1
}

Write-Host "Using mt.exe: $mtExe"
Write-Host "Embedding manifest..."

& $mtExe -manifest $manifestPath -outputresource:"$exePath;#1"

if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to embed manifest"
    exit 1
}

Write-Host ""
Write-Host "[OK] Manifest embedded"
Write-Host ""
Write-Host "Executable: target\$buildType\mcv-installer.exe"
