# Anleitung: `appsettings.labels.json`

Diese Datei steuert, **welche Etiketten-Vorlagen** (LBX) in der App zur Auswahl stehen, **wie Excel-Spalten** den Feldern zugeordnet werden und welche **Standards** gelten.

---

## Aufbau (Beispiel)

```json
{
  "templates": [
    {
      "id": "Produkte_40x24_mP",
      "title": "Produkte_40x24_mP",
      "file": "Templates/Produkte_40x24_mP.lbx",
      "objectMap": {
        "artikelnummer": "artnr",
        "text":          "name",
        "beschreibung":  "desc",
        "preis":         "price",
        "ean":           "ean"
      }
    }
  ],

  "columnAliases": {
    "artikelnummer": ["Artikel-Nr", "ArtNr", "Artikelnummer", "Artikel ID"],
    "text":          ["Text", "Name", "Artikelname", "Bezeichnung"],
    "beschreibung":  ["Kurzbeschreibung", "Beschreibung", "Desc"],
    "preis":         ["Preis", "Artikelpreis-2", "VK", "Verkaufspreis"],
    "menge":         ["VPE", "Menge", "Anzahl", "Qty", "Stück"],
    "ean":           ["EAN", "EAN-Code", "Barcode", "EAN13"]
  },

  "defaults": {
    "sheetName": "",
    "copiesIfMissing": 1,
    "decimalSeparator": ","
  }
}
```

---

## Abschnitt `templates`

Jeder Eintrag beschreibt **eine** LBX-Vorlage:

- `id`: interne Kennung (wird in der App angezeigt; alphabetisch sortiert).
- `title`: Anzeigename (in diesem Projekt = `id`).
- `file`: Pfad zur LBX-Datei relativ zum Programmordner (z. B. `Templates/…`).
- `objectMap`: Zuordnung **logisches Feld → Objektname in der LBX**.

### Logische Felder (werden von der App verstanden)

| Logisches Feld    | Bedeutung                                  | Typ / Hinweis                                   |
|---                |---                                          |---                                              |
| `artikelnummer`   | Artikel-Nr / SKU                            | Text                                            |
| `text`            | Artikelname / Text                          | Text                                            |
| `beschreibung`    | Kurzbeschreibung                            | Text (optional)                                 |
| `preis`           | Preis                                       | Zahl/Text; Formatierung gem. `decimalSeparator` |
| `ean`             | EAN-Barcode                                 | nur Ziffern; 12→13 Stellen (Prüfziffer)        |
| `menge`*          | Anzahl zu druckender Etiketten              | kommt aus Excel; **nicht** in `objectMap`      |

\* `menge`/`VPE` wird **nicht** in der LBX befüllt, sondern steuert die **Kopienanzahl**.

### WICHTIG: Objekt-Namen in der LBX

Die **Werte** in `objectMap` (z. B. `"artnr"`, `"name"`, `"desc"`, `"price"`, `"ean"`) müssen **exakt** den **Objekt-Namen** im P-touch Editor treffen:

1. LBX im **P-touch Editor** öffnen.
2. Jedes Feld/Barcode anklicken → **Eigenschaften** → **Objektname** setzen.
3. Namen in `objectMap` wiederverwenden.

> Empfehlung: durchgängig **klein** schreiben (`artnr`, `name`, `desc`, `price`, `ean`).

### Barcode / EAN

- Excel-EAN darf nur **Ziffern** enthalten (Leerzeichen/Trennzeichen werden entfernt).
- Bei **12 Ziffern** ergänzt die App automatisch die 13. Prüfziffer (EAN-13).
- Im LBX-Objekt den **Barcode-Typ** passend einstellen (z. B. EAN13).

---

## Abschnitt `columnAliases`

Hier definierst du, **welche Excel-Spaltennamen** als welches logische Feld erkannt werden.  
Die App liest die Kopfzeile und nimmt pro logischem Feld den **ersten passenden** Alias.  
**Wichtig**: Beim Preis die Reihenfolge nicht ändern, so wird der `Aktionspreis 2` vor dem normalen Preis genommen.

Beispiele:
- `["Artikel-Nr","ArtNr"]` → beide zählen als `artikelnummer`.
- `["VPE","Menge","Anzahl"]` → steuern die Druckanzahl.

---
## Abschnitt `ignoredArticles`
In dieser Liste kannst du Artikelnummern ignorieren, damit diese nicht als Etiketten gedruckt werden, bspw. das Porto
- `ignoredArticles`: `["Porto3","andere ArtNr"]`
---
## Abschnitt `defaults`

- `sheetName`: Tabellenblattname; leer = **erstes** Blatt.
- `copiesIfMissing`: Fallback-Anzahl, wenn `menge` fehlt/leerer Wert.
- `decimalSeparator`: `","` oder `"."` – steuert die Anzeigeformatierung des Preises.

---

## Häufige Konfigurationen

**EAN + Preis (kleines Preis-Label):**
```json
"objectMap": { "ean": "ean", "preis": "price" }
```

**EAN + Artikelnummer + Name (ohne Preis):**
```json
"objectMap": { "ean": "ean", "artikelnummer": "artnr", "text": "name" }
```

**EAN + Name + Kurzbeschreibung + Preis (größeres Label):**
```json
"objectMap": { "ean": "ean", "text": "name", "beschreibung": "desc", "preis": "price" }
```

---

## Typische Fehler & Lösungen

- **Feld bleibt leer:** Objektname in der LBX stimmt nicht mit `objectMap` überein → im Editor anpassen.
- **Strichcode ungültig:** Nicht-numerische Zeichen in der Excel-EAN → bereinigen; 12-stellig ist ok (Prüfziffer wird erzeugt).
- **Preisformat „3,5“ statt „3,50“:** `defaults.decimalSeparator` prüfen; Zahlen aus Excel werden als `0.00` formatiert.
- **Falsches Etikettenformat/Band:** andere Vorlage wählen; das LBX bestimmt Größe/Schnitt/Band.

---

## Neue Vorlage hinzufügen (Checkliste)

1. LBX im P-touch Editor erstellen (richtiges Band/Format).
2. Objektnamen setzen: `artnr`, `name`, `desc`, `price`, `ean` (je nach Bedarf).
3. Datei nach `Templates/` legen (z. B. `Templates/Magnet_50x24_mP.lbx`).
4. In `appsettings.labels.json` neuen `templates`-Eintrag ergänzen.
5. App starten → Vorlage erscheint (alphabetisch sortiert).


