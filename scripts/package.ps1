
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

# --- naechste Patch-Version aus CHANGELOG bestimmen ---
$cl = Join-Path $repo "CHANGELOG.md"
$next = 1
if (Test-Path $cl) {
  $raw = Get-Content $cl -Raw
  # Finde Zeilen wie: "## v1.0.23 - 2025-08-21"
  $regex = [regex]'(?im)^\s*##\s+v(?<maj>\d+)\.(?<min>\d+)\.(?<pat>\d+)\s*-'
  $matches = $regex.Matches($raw)
  if ($matches.Count -gt 0) {
    $parts = $base.Split('.')
    if ($parts.Length -ge 2) {
      $majBase = [int]$parts[0]
      $minBase = [int]$parts[1]
      $patches = New-Object System.Collections.Generic.List[int]
      foreach ($m in $matches) {
        $maj = [int]$m.Groups['maj'].Value
        $min = [int]$m.Groups['min'].Value
        $pat = [int]$m.Groups['pat'].Value
        if ($maj -eq $majBase -and $min -eq $minBase) { $patches.Add($pat) }
      }
      if ($patches.Count -gt 0) {
        $max = ($patches | Measure-Object -Maximum).Maximum
        $next = [int]$max + 1
      }
    }
  }
}

$version = "$base.$next"
$deliverRoot = Join-Path $repo "deliverables"
$out = Join-Path $deliverRoot ("LabelPrinter_" + $version)

# --- Publish ---
$pub = Join-Path $repo ("publish\" + $Runtime)
dotnet publish .\LabelPrinter.csproj -c $Configuration -r $Runtime `
  --self-contained true `
  -p:PublishSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true `
  -p:EnableCompressionInSingleFile=true `
  -o $pub

# --- Ausgabeordner befuellen ---
New-Item -ItemType Directory -Force -Path $deliverRoot | Out-Null
New-Item -ItemType Directory -Force -Path $out | Out-Null

# Kerndateien
Copy-Item "$pub\LabelPrinter.exe" $out -Force
if (Test-Path "$pub\appsettings.labels.json") { Copy-Item "$pub\appsettings.labels.json" $out -Force }
if (Test-Path "$pub\Templates")               { Copy-Item "$pub\Templates"             $out -Recurse -Force }

# Dokus (mitliefern, falls vorhanden)
if (Test-Path ".\docs\INSTALLATION-DE.md")       { Copy-Item ".\docs\INSTALLATION-DE.md"       (Join-Path $out "INSTALLATION-DE.md")       -Force }
if (Test-Path ".\docs\ANLEITUNG-appsettings.md") { Copy-Item ".\docs\ANLEITUNG-appsettings.md" (Join-Path $out "ANLEITUNG-appsettings.md") -Force }

# optional: b-PAC Installer beilegen
if (Test-Path ".\installer\bpac") {
  $bpacOut = Join-Path $out "bpac"
  New-Item -ItemType Directory -Force -Path $bpacOut | Out-Null
  Copy-Item ".\installer\bpac\*.exe" $bpacOut -Force -ErrorAction SilentlyContinue
}

# Version.txt
$version | Set-Content (Join-Path $out "VERSION.txt") -Encoding UTF8

# --- CHANGELOG aktualisieren ---
$today = Get-Date -Format "yyyy-MM-dd"
if ([string]::IsNullOrWhiteSpace($Notes)) { $notesLine = "- Wartungsupdate" } else { $notesLine = "- $Notes" }

# Header (ASCII, stabil)
$header = "# Changelog`r`n`r`nFormat: vX.Y.Z - YYYY-MM-DD`r`n`r`n"

# Vorhandenen Inhalt lesen und Header (inkl. optionaler Format-Zeile) entfernen
$existing = ""
if (Test-Path $cl) {
  $existing = Get-Content $cl -Raw
  $pattern = '^\s*#\s*Changelog\s*\r?\n(?:\s*Format:.*\r?\n)?\s*\r?\n'
  $existing = [System.Text.RegularExpressions.Regex]::Replace(
    $existing, $pattern, '',
    [System.Text.RegularExpressions.RegexOptions]::IgnoreCase
  )
}

# Neuen Eintrag bauen
$entry = "## v$version - $today`r`n$notesLine`r`n`r`n"
$newContent = $header + $entry + $existing

# Schreiben & mitliefern
Set-Content $cl $newContent -Encoding UTF8
Copy-Item $cl (Join-Path $out "CHANGELOG.md") -Force

Write-Host ("Fertig: " + $out)