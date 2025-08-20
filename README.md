# LabelPrinter

A Windows Forms utility for printing Brother P-touch labels based on CSV or Excel data using the b‑PAC SDK.

## Build and Package
See [docs/DEPLOYMENT.md](docs/DEPLOYMENT.md) for step‑by‑step instructions on
creating a self‑contained Windows executable, bundling the b‑PAC runtime and
building the optional Inno Setup installer.

## Configuration
* `appsettings.labels.json` – lists available label templates and mappings
  from CSV/Excel columns to b‑PAC object names.
* `Templates/` – contains `.lbx` files created with P‑touch Editor.

## Running
After building or installing, start `LabelPrinter.exe`, select the desired
template and data file, then print.