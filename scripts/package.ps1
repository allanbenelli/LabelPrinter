param(
  [string]$Runtime = "win-x64",
  [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSCommandPath
$repo = Split-Path -Parent $root
Set-Location $repo

# 1) Publish
$pub = Join-Path $repo "publish\$Runtime"
dotnet publish .\LabelPrinter.csproj -c $Configuration -r $Runtime `
  --self-contained true `
  -p:PublishSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true `
  -p:EnableCompressionInSingleFile=true `
  -o $pub

# 2) Zielordner erstellen
$stamp = Get-Date -Format "yyyyMMdd_HHmm"
$out = Join-Path $repo "deliverables\LabelPrinter_$stamp"
New-Item -ItemType Directory -Force -Path $out | Out-Null

# 3) Dateien kopieren
Copy-Item "$pub\LabelPrinter.exe" $out -Force
Copy-Item "$pub\appsettings.labels.json" $out -Force -ErrorAction SilentlyContinue
if (Test-Path "$pub\Templates") { Copy-Item "$pub\Templates" $out -Recurse -Force }

# optional: bPAC-Installer aus repo/installer/bpac -> deliverables/bpac
if (Test-Path ".\installer\bpac") {
  New-Item -ItemType Directory -Force -Path (Join-Path $out "bpac") | Out-Null
  Copy-Item ".\installer\bpac\*.exe" (Join-Path $out "bpac") -Force -ErrorAction SilentlyContinue
}

# 4) Anleitung kopieren
$docSrc = ".\docs\INSTALLATION-DE.md"
if (Test-Path $docSrc) {
  Copy-Item $docSrc (Join-Path $out "Installationsanleitung (DE).md") -Force
} else {
  # Fallback: kurze Readme erzeugen
  @"
# LabelPrinter – Kurzanleitung
1) b-PAC Runtime installieren (siehe Ordner 'bpac' oder Brother-Website).
2) LabelPrinter.exe starten.
3) Excel wählen, Vorlage wählen, Drucker wählen, Drucken.
"@ | Set-Content (Join-Path $out "Installationsanleitung (DE).md") -Encoding UTF8
}

Write-Host "Fertig: $out"
