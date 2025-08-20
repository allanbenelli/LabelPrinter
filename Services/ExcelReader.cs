using ClosedXML.Excel;

namespace LabelPrinter.Services;

public sealed class ExcelRow
{
    public Dictionary<string, object?> Cells { get; } = new(StringComparer.OrdinalIgnoreCase);
}

public sealed class ExcelReader
{
    public (List<string> Headers, List<ExcelRow> Rows) Read(string path, string? sheetName = null)
    {
        using var wb = new XLWorkbook(path);
        var ws = string.IsNullOrWhiteSpace(sheetName)
            ? wb.Worksheets.First()
            : wb.Worksheet(sheetName);

        var used = ws.RangeUsed() ?? throw new InvalidOperationException("Die Tabelle enthält keine Daten.");
        var headerRow = used.FirstRow();
        var lastRow = used.LastRow();

        var headers = headerRow.Cells().Select(c => c.GetString().Trim()).ToList();
        var rows = new List<ExcelRow>();

        // Starte in der Zeile nach dem Header
        for (int r = headerRow.RowNumber() + 1; r <= lastRow.RowNumber(); r++)
        {
            var rec = new ExcelRow();
            for (int c = 1; c <= headers.Count; c++)
            {
                var header = headers[c - 1];
                var cell = ws.Cell(r, c);
                rec.Cells[header] = cell.GetString(); // als String speichern (robust für Zahl/Text)
            }
            rows.Add(rec);
        }

        return (headers, rows);
    }
}
