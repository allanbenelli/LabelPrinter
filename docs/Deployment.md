# Deployment and Packaging

This guide explains how to build a Windows executable of **LabelPrinter**
and distribute it with the Brother b-PAC SDK so that customers can install
the application on Windows 11 without additional prerequisites.

## 1. Obtain the b-PAC SDK
1. Download the current b-PAC SDK from the [Brother developer site](https://support.brother.com/). Current Version: `bsdkw34014_64us.exe`
2. Extract the redistributable package.
   You will need the runtime installer (e.g. `bPACSDKSetup.exe`) and the
   `bpac` COM components contained in the SDK.

> Brother's license allows bundling the runtime with your application, but
> ensure you comply with the SDK license and distribution terms.

## 2. Build a self‑contained executable
Run the following command on a Windows machine with the .NET 8 SDK installed:

```powershell
# build a single-file executable for 64-bit Windows
 dotnet publish -c Release -r win-x64 \
    --self-contained true \
    -p:PublishSingleFile=true \
    -p:IncludeNativeLibrariesForSelfExtract=true \
    -p:EnableCompressionInSingleFile=true \
    -o publish/win-x64
```

* For 32-bit or ARM devices adjust the runtime identifier (`win-x86`,
  `win-arm64`).
* The output folder `publish/win-x64` contains `LabelPrinter.exe` and the
  files from `Templates` and `appsettings.labels.json` that are copied by
  the project file.

## 3. Bundle the b-PAC runtime
Create the folder `installer/bpac/` in the repository and place the
redistributable files from the SDK inside it:

* `installer/bpac/bPACSDKSetup.exe`
* `installer/bpac/bpac/` (folder containing `bpac.dll` and helpers)

These paths are used by the provided Inno Setup script.

## 4. Build the installer (optional but recommended)
1. Install [Inno Setup](https://jrsoftware.org/isinfo.php).
2. Run `iscc installer/LabelPrinter.iss`.
3. The installer `dist/LabelPrinterSetup.exe` is created and includes the
   b-PAC runtime.

During installation or first start you must ensure that the COM library is
registered. The Inno Setup script automatically runs `bPACSDKSetup.exe` in
silent mode. Alternatively, you can check for the `bpac.Document` COM
object on first run and launch the installer yourself if it is missing.

## 5. Distribute to customers
Package the contents of the publish directory (or use the installer from
the previous step) and deliver it via e-mail or file sharing. The customer
only needs to run the installer or unzip the archive and execute
`LabelPrinter.exe`.

## 6. Creating and updating label templates
1. Install **P-touch Editor** from Brother.
2. Design the label and save it as an `.lbx` file.
3. Place the file in the `Templates` folder of the application.
4. Update `appsettings.labels.json` to point to the template and map the
   object names to data fields. Example:

```json
{
  "templates": [
    {
      "id": "produkte_40x24_mP",
      "title": "Produkte_40x24_mP",
      "file": "Templates/Produkte_40x24_mp.lbx",
      "objectMap": {
        "ean": "ean",
        "preis": "price",
        "artikelnummer": "artikelnummer"
      }
    }
  ]
}
```

## 7. Customer installation instructions
1. Run the provided installer or extract the zip archive.
2. The installer automatically installs the b-PAC runtime.
3. Start `LabelPrinter.exe`.
4. Choose the desired template and CSV/Excel source file.
5. Optionally adjust `appsettings.labels.json` to configure template
   mappings or ignored articles.

The application now prints P‑Touch labels using the embedded b-PAC SDK.