param(
  [string]$IssPath = "tools\inno\MultiCommentViewer.iss",
  [string]$CoreZipPath = "",
  [string]$BasicPluginsDir = "",
  [string]$AppExeName = ""
)

$iscc = Get-Command iscc.exe -ErrorAction SilentlyContinue
if (-not $iscc) {
  Write-Error "iscc.exe が見つかりません。Inno Setup 6 をインストールし、PATHに追加してください。"
  exit 1
}

function Resolve-CoreZipPath {
  param([string]$ExplicitCoreZipPath)

  if ($ExplicitCoreZipPath) {
    if (Test-Path $ExplicitCoreZipPath) { return (Resolve-Path $ExplicitCoreZipPath).Path }
    throw "指定された CoreZipPath が存在しません: $ExplicitCoreZipPath"
  }

  $latestZip = Get-ChildItem "output" -File -Filter "MultiCommentViewer_v*.zip" -ErrorAction SilentlyContinue |
    Sort-Object LastWriteTime -Descending |
    Select-Object -First 1

  if (-not $latestZip) {
    throw "output ディレクトリに MultiCommentViewer_v*.zip が見つかりません。先にZIPをビルドしてください。"
  }

  return $latestZip.FullName
}

function Resolve-AppVersionFromZipName {
  param([string]$ResolvedCoreZipPath)
  $base = [System.IO.Path]::GetFileNameWithoutExtension($ResolvedCoreZipPath)
  $m = [regex]::Match($base, '^MultiCommentViewer_v(?<ver>\d+\.\d+\.\d+)(?:_.+)?$')
  if ($m.Success) { return $m.Groups['ver'].Value }
  return "0.1.0"
}

function Resolve-OutputBaseFilename {
  param([string]$ResolvedCoreZipPath)
  $base = [System.IO.Path]::GetFileNameWithoutExtension($ResolvedCoreZipPath)
  if ($base -match '^MultiCommentViewer_v.+$') {
    return "$base-Setup"
  }
  return "MultiCommentViewer-Setup"
}

function Resolve-AppExeNameFromZip {
  param([string]$ResolvedCoreZipPath, [string]$ExplicitAppExeName)

  if ($ExplicitAppExeName) { return $ExplicitAppExeName }

  Add-Type -AssemblyName System.IO.Compression.FileSystem
  $zip = [System.IO.Compression.ZipFile]::OpenRead($ResolvedCoreZipPath)
  try {
    foreach ($entry in $zip.Entries) {
      $name = [System.IO.Path]::GetFileName($entry.FullName)
      if ($name -ieq "MultiCommentViewer.exe") { return "MultiCommentViewer.exe" }
    }
  } finally {
    $zip.Dispose()
  }

  return "MultiCommentViewer.exe"
}

function Resolve-BasicPluginsDir {
  param([string]$ExplicitDir)
  if ($ExplicitDir) {
    if (Test-Path $ExplicitDir) { return (Resolve-Path $ExplicitDir).Path }
    throw "指定された BasicPluginsDir が存在しません: $ExplicitDir"
  }

  $candidate = Join-Path (Get-Location) "output\plugins\basic"
  if (Test-Path $candidate) { return (Resolve-Path $candidate).Path }
  return ""
}

$resolvedCoreZipPath = Resolve-CoreZipPath -ExplicitCoreZipPath $CoreZipPath
$resolvedAppVersion = Resolve-AppVersionFromZipName -ResolvedCoreZipPath $resolvedCoreZipPath
$resolvedOutputBaseFilename = Resolve-OutputBaseFilename -ResolvedCoreZipPath $resolvedCoreZipPath
$resolvedAppExeName = Resolve-AppExeNameFromZip -ResolvedCoreZipPath $resolvedCoreZipPath -ExplicitAppExeName $AppExeName
$resolvedPluginsDir = Resolve-BasicPluginsDir -ExplicitDir $BasicPluginsDir

$args = @(
  "/DCoreZipPath=$resolvedCoreZipPath",
  "/DAppVersion=$resolvedAppVersion",
  "/DAppExeName=$resolvedAppExeName",
  "/DOutputBaseFilename=$resolvedOutputBaseFilename"
)
if ($resolvedPluginsDir) {
  $args += "/DBasicPluginsDir=$resolvedPluginsDir"
}
$args += $IssPath

Write-Host "CoreZipPath: $resolvedCoreZipPath"
Write-Host "AppVersion: $resolvedAppVersion"
Write-Host "AppExeName: $resolvedAppExeName"
Write-Host "OutputBaseFilename: $resolvedOutputBaseFilename"
if ($resolvedPluginsDir) {
  Write-Host "BasicPluginsDir: $resolvedPluginsDir"
} else {
  Write-Host "BasicPluginsDir: (not found, skipped)"
}

& $iscc.Source @args
if ($LASTEXITCODE -ne 0) {
  Write-Error "Inno Setup ビルドに失敗しました。"
  exit $LASTEXITCODE
}

Write-Host "Inno Setup インストーラのビルドが完了しました。"
