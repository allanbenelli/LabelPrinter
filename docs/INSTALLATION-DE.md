# LabelPrinter – Installationsanleitung (Windows 11)

## 1) Voraussetzungen
- Windows 11 (64-bit)
- Brother PT-E550W Drucker (USB oder WLAN)
- Brother Druckertreiber für PT-E550W (Windows)
- Brother b-PAC Runtime (SDK Runtime, 64-bit)

Downloads (offizielle Brother-Seiten):
- b-PAC SDK/Runtime: support.brother.com → b-PAC Downloadseite. 
- Treiber/Software für PT-E550W: support.brother.com → Modell [PT-E550W](https://support.brother.com/g/b/downloadlist.aspx?c=de&lang=de&prod=e550weuk&os=10069)
- (Optional) P-touch Editor zum Erstellen/Ändern von Vorlagen.

## 2) Installation
1. Drucker per USB verbinden oder ins WLAN einbinden.
2. Brother PT-E550W Treiber installieren.
3. Brother b-PAC Runtime installieren (liegt im Ordner „bpac“ dieses Pakets oder von der Brother-Website).
4. Ordner „LabelPrinter“ an einen beliebigen Ort kopieren (z. B. Desktop oder C:\Programme\LabelPrinter).
   - Enthalten sind: `LabelPrinter.exe`, `appsettings.labels.json`, Ordner `Templates`, ggf. `bpac`.
5. Starten: `LabelPrinter.exe`.

## 3) Verwendung
1. Excel-Datei auswählen (aus dem ERP exportiert).
2. Vorlage wählen (entspricht Etikett/ Band/Größe).
3. Drucker „Brother … PT-E550W …“ wählen.
4. „Drucken“ klicken – die Menge/VPE-Spalte bestimmt die Anzahl.

## 4) Excel-Format (Beispiel)
- Kopfzeile vorhanden. Spaltennamen können variieren.
- Erkannte Felder (Aliase): 
  - Artikel-Nr (z. B. „Artikel-Nr“, „ArtNr“)
  - Text/Name („Text“, „Name“)
  - Kurzbeschreibung („Kurzbeschreibung“, „Beschreibung“)
  - Preis („Preis“, „Artikelpreis-2“)
  - Menge/VPE („Menge“, „VPE“, „Anzahl“, „Qty“)
  - EAN („EAN“, „EAN-Code“), 12 oder 13 Ziffern (Prüfziffer wird ggf. ergänzt)

## 5) Häufige Probleme
- Meldung „b-PAC Runtime fehlt“ → `bpac\bPACSDKSetup.exe` ausführen oder von Brother-Website installieren.
- Drucker wird nicht angezeigt → Treiber installieren, Verbindung prüfen (USB/WLAN).
- EAN wird nicht gedruckt → EAN muss nur Ziffern enthalten; bei 12 Ziffern wird die 13. erzeugt.
- Falsches Etikettenformat → andere Vorlage wählen.

