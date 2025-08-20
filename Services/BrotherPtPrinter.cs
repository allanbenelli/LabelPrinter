using System.Net;

namespace LabelPrinter.Services;

public sealed class BrotherPtPrinter : IDisposable
{
    private const int BpoDefault = 0;
    private dynamic? _doc;

    public void OpenTemplate(string templatePath)
    {
        var t = Type.GetTypeFromProgID("bpac.Document")
                ?? throw new InvalidOperationException("b-PAC nicht installiert (ProgID 'bpac.Document' fehlt).");
        _doc = Activator.CreateInstance(t) ?? throw new Exception("b-PAC Instanz konnte nicht erstellt werden.");

        if (!File.Exists(templatePath))
            throw new FileNotFoundException("LBX Vorlage nicht gefunden", templatePath);

        if (!_doc.Open(templatePath))
            throw new InvalidOperationException("LBX Vorlage konnte nicht geöffnet werden.");
    }

    public void SelectPrinter(string? printerNameOrNull)
    {
        if (_doc == null) throw new InvalidOperationException("Vorher OpenTemplate aufrufen.");
        if (!string.IsNullOrWhiteSpace(printerNameOrNull))
            _doc.Printer = printerNameOrNull;
    }

    public void SetField(string objectName, string text)
    {
        if (_doc == null) throw new InvalidOperationException("Vorher OpenTemplate aufrufen.");
        var obj = _doc.GetObject(objectName);
        if (obj == null) throw new ArgumentException($"LBX-Objekt '{objectName}' nicht gefunden.");
        obj.Text = text ?? "";
    }

    public void StartPrint() => _doc?.StartPrint("", BpoDefault);
    public void PrintCopies(int copies) => _doc?.PrintOut(Math.Max(0, copies), BpoDefault);
    public void EndPrint() => _doc?.EndPrint();

    public void Close()
    {
        _doc?.Close();
        _doc = null;
    }

    public void Dispose() => Close();
}
