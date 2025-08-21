param(
  [string]$Runtime = "win-x64",
  [string]$Configuration = "Release",
  [string]$Notes = ""
)

$ErrorActionPreference = "Stop"

# Pfade
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repo      = Split-Path -Parent $scriptDir
Set-Location $repo

# --- Basisversion lesen (z.B. "1.0") ---
$baseFile = Join-Path $repo "VERSION_BASE"
$base = "1.0"
if (Test-Path $baseFile) {
  $base = (Get-Content $baseFile -Raw).Trim()
}

# --- naechste Patch-Version bestimmen: LabelPrinter_<base>.<N> ---
$deliverRoot = Join-Path $repo "deliverables"
New-Item -ItemType Directory -Force -Path $deliverRoot | Out-Null

$pattern = "^LabelPrinter_$([regex]::Escape($base))\.(\d+)$"
$patches = @()
if (Test-Path $deliverRoot) {
  foreach ($d in Get-ChildItem $deliverRoot -Directory -ErrorAction SilentlyContinue) {
    if ($d.Name -match $pattern) { $patches += [int]$matches[1] }
  }
}
$next = 1
if ($patches.Count -gt 0) {
  $next = ((($patches | Measure-Object -Maximum).Maximum) + 1)
}

$version = "$base.$next"
$out     = Join-Path $deliverRoot ("LabelPrinter_" + $version)

# --- Publish ---
$pub = Join-Path $repo "publish\$Runtime"
dotnet publish .\LabelPrinter.csproj -c $Configuration -r $Runtime `
  --self-contained true `
  -p:PublishSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true `
  -p:EnableCompressionInSingleFile=true `
  -o $pub

# --- Ausgabeordner befuellen ---
New-Item -ItemType Directory -Force -Path $out | Out-Null
Copy-Item "$pub\LabelPrinter.exe" $out -Force
if (Test-Path "$pub\appsettings.labels.json") { Copy-Item "$pub\appsettings.labels.json" $out -Force }
if (Test-Path "$pub\Templates")               { Copy-Item "$pub\Templates"             $out -Recurse -Force }

# b-PAC optional beilegen
if (Test-Path ".\installer\bpac") {
  $bpacOut = Join-Path $out "bpac"
  New-Item -ItemType Directory -Force -Path $bpacOut | Out-Null
  Copy-Item ".\installer\bpac\*.exe" $bpacOut -Force -ErrorAction SilentlyContinue
}

# Installationsanleitung beilegen
$docSrc = ".\docs\INSTALLATION-DE.md"
if (Test-Path $docSrc) {
  Copy-Item $docSrc (Join-Path $out "INSTALLATION-DE.md") -Force
}

# Version.txt
$version | Set-Content (Join-Path $out "VERSION.txt") -Encoding UTF8

# --- Changelog aktualisieren (UTF-8, stabiler Header) ---
$cl = Join-Path $repo "CHANGELOG.md"
$today = Get-Date -Format "yyyy-MM-dd"

# Notes ermitteln (Prompt aus VS Code Task liefert hier den Text)
if ([string]::IsNullOrWhiteSpace($Notes)) { $notesLine = "- Wartungsupdate" } else { $notesLine = "- $Notes" }

$header = "# Changelog`r`n`r`nFormat: vX.Y.Z - YYYY-MM-DD`r`n`r`n"

# vorhandenen Inhalt lesen und ggf. alten Header entfernen
$existing = ""
if (Test-Path $cl) {
  $existing = Get-Content $cl -Raw
  # Entfernt am Dateianfang:
  #   # Changelog
  #   [leere Zeilen]
  #   (optional) Format: vX.Y.Z - YYYY-MM-DD
  #   [leere Zeilen]
  $pattern = '^\s*#\s*Changelog\s*\r?\n(?:\s*Format:.*\r?\n)?\s*\r?\n'
  $existing = [System.Text.RegularExpressions.Regex]::Replace(
    $existing, $pattern, '', 
    [System.Text.RegularExpressions.RegexOptions]::IgnoreCase
  )
}

$entry = "## v$version - $today`r`n$notesLine`r`n`r`n"
$newContent = $header + $entry + $existing
Set-Content $cl $newContent -Encoding UTF8
Copy-Item $cl (Join-Path $out "CHANGELOG.md") -Force

Write-Host ("Fertig: " + $out)
