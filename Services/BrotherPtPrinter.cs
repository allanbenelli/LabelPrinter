using System.Reflection;

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
        {
            var type = _doc.GetType();
            try
            {
                type.InvokeMember(
                    "Printer",
                    BindingFlags.SetProperty,
                    null,
                    _doc,
                    new object[] { printerNameOrNull });
            }
            catch (TargetInvocationException tie)
            {
                throw tie.InnerException ?? tie;
            }
        }
    }

    public void SetField(string objectName, string text)
    {
        if (_doc == null) throw new InvalidOperationException("Vorher OpenTemplate aufrufen.");
        var obj = _doc.GetObject(objectName);
        if (obj == null) throw new ArgumentException($"LBX-Objekt '{objectName}' nicht gefunden.");
        obj.Text = text ?? "";
    }

    public void StartPrint()
    {
        if (_doc == null) throw new InvalidOperationException("Vorher OpenTemplate aufrufen.");
        var type = _doc.GetType();
        try
        {
            var ok = (bool)type.InvokeMember(
                "StartPrint",
                BindingFlags.InvokeMethod,
                null,
                _doc,
                new object[] { "", BpoDefault });
            if (!ok)
                throw new InvalidOperationException("StartPrint meldete Fehler.");
        }
        catch (TargetInvocationException tie)
        {
            throw tie.InnerException ?? tie;
        }
    }

    public void PrintCopies(int copies)
    {
        if (_doc == null) throw new InvalidOperationException("Vorher OpenTemplate aufrufen.");
        var type = _doc.GetType();
        try
        {
            type.InvokeMember(
                "PrintOut",
                BindingFlags.InvokeMethod,
                null,
                _doc,
                new object[] { Math.Max(0, copies), BpoDefault });
        }
        catch (TargetInvocationException tie)
        {
            throw tie.InnerException ?? tie;
        }
    }

    public void EndPrint()
    {
        if (_doc == null) return;
        var type = _doc.GetType();
        try
        {
            type.InvokeMember(
                "EndPrint",
                BindingFlags.InvokeMethod,
                null,
                _doc,
                Array.Empty<object>());
        }
        catch (TargetInvocationException tie)
        {
            throw tie.InnerException ?? tie;
        }
    }

    public void Close()
    {
        _doc?.Close();
        _doc = null;
    }

    public void Dispose() => Close();
}
