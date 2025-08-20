using System.Drawing;
using System.Drawing.Printing;
using System.Text.Json;
using System.Windows.Forms;
using LabelPrinter.Models;
using LabelPrinter.Services;

namespace LabelPrinter;

public class MainForm : Form
{
    private TextBox _txtExcel = null!;
    private ComboBox _cmbTemplate = null!;
    private ComboBox _cmbPrinter = null!;
    private Button _btnBrowse = null!;
    private Button _btnPrint = null!;
    private Label _lblStatus = null!;

    private AppConfig _cfg = new();

    public MainForm()
    {
        // Fenster
        AutoScaleMode = AutoScaleMode.Dpi;        // DPI-freundlich
        StartPosition = FormStartPosition.CenterScreen;
        Text = "P-touch Excel Label Printer";
        MinimumSize = new Size(720, 300);

        // Hauptlayout: 3 Spalten (Label / Input / Button)
        var table = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 4,
            Padding = new Padding(12),
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink
        };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));            // Label
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));       // Eingaben
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130f));      // Buttons rechts
        table.RowStyles.Add(new RowStyle(SizeType.AutoSize));                  // Excel
        table.RowStyles.Add(new RowStyle(SizeType.AutoSize));                  // Vorlage
        table.RowStyles.Add(new RowStyle(SizeType.AutoSize));                  // Drucker
        table.RowStyles.Add(new RowStyle(SizeType.AutoSize));                  // Status+Button

        // Excel
        var lblExcel = new Label { Text = "Excel-Datei:", AutoSize = true, Anchor = AnchorStyles.Left };
        _txtExcel = new TextBox { Dock = DockStyle.Fill };
        _btnBrowse = new Button { Text = "Durchsuchen…" };
        _btnBrowse.Click += (_, __) => BrowseExcel();

        // Vorlage
        var lblTemplate = new Label { Text = "Vorlage:", AutoSize = true, Anchor = AnchorStyles.Left };
        _cmbTemplate = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };

        // Drucker
        var lblPrinter = new Label { Text = "Drucker:", AutoSize = true, Anchor = AnchorStyles.Left };
        _cmbPrinter = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };

        // Status + Drucken
        _lblStatus = new Label { Text = "Bereit.", AutoSize = true, Anchor = AnchorStyles.Left, AutoEllipsis = true };
        _btnPrint = new Button { Text = "Drucken", Dock = DockStyle.Fill, Height = 34 };
        _btnPrint.Click += async (_, __) => await PrintAsync();
        AcceptButton = _btnPrint;

        // In Tabelle einsetzen
        table.Controls.Add(lblExcel,    0, 0);
        table.Controls.Add(_txtExcel,   1, 0);
        table.Controls.Add(_btnBrowse,  2, 0);

        table.Controls.Add(lblTemplate, 0, 1);
        table.Controls.Add(_cmbTemplate,1, 1);
        table.Controls.Add(new Panel(){Dock=DockStyle.Fill}, 2, 1); // Platzhalter

        table.Controls.Add(lblPrinter,  0, 2);
        table.Controls.Add(_cmbPrinter, 1, 2);
        table.Controls.Add(new Panel(){Dock=DockStyle.Fill}, 2, 2);

        // Status über 2 Spalten, Drucken rechts
        table.Controls.Add(_lblStatus,  0, 3);
        table.SetColumnSpan(_lblStatus, 2);
        table.Controls.Add(_btnPrint,   2, 3);

        Controls.Add(table);

        Load += (_, __) =>
        {
            LoadConfig();
            LoadTemplates();
            LoadPrinters();
        };
    }

    private void LoadConfig()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "appsettings.labels.json");
        if (!File.Exists(path))
        {
            MessageBox.Show($"Konfiguration fehlt:\n{path}", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
            _cfg = new AppConfig();
            return;
        }

        _cfg = JsonSerializer.Deserialize<AppConfig>(
            File.ReadAllText(path),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        ) ?? new AppConfig();
    }

    private void LoadTemplates()
    {
        _cmbTemplate.Items.Clear();
        foreach (var t in _cfg.Templates)
            _cmbTemplate.Items.Add(new TemplateItem(t));
        if (_cmbTemplate.Items.Count > 0)
            _cmbTemplate.SelectedIndex = 0;
    }

    private void LoadPrinters()
    {
        _cmbPrinter.Items.Clear();
        string? defaultBrother = null;

        foreach (string p in PrinterSettings.InstalledPrinters)
        {
            _cmbPrinter.Items.Add(p);
            if (defaultBrother == null && p.Contains("Brother", StringComparison.OrdinalIgnoreCase))
                defaultBrother = p;
        }

        if (defaultBrother != null)
            _cmbPrinter.SelectedItem = defaultBrother;
        else if (_cmbPrinter.Items.Count > 0)
            _cmbPrinter.SelectedIndex = 0;
    }

    private void BrowseExcel()
    {
        using var ofd = new OpenFileDialog
        {
            Filter = "Excel (*.xlsx;*.xls)|*.xlsx;*.xls|Alle Dateien (*.*)|*.*"
        };
        if (ofd.ShowDialog() == DialogResult.OK)
            _txtExcel.Text = ofd.FileName;
    }

    // ---- Dein bestehendes PrintAsync() aus der letzten Version hier belassen ----
    // (nur die bereits vorgeschlagenen Preis-/Copies-Zeilen mit get("preis")/get("menge") verwenden)

    private async Task PrintAsync()
    {
        try
        {
            _btnPrint.Enabled = false;
            _lblStatus.Text = "Lese Excel…";

            var excelPath = _txtExcel.Text;
            if (string.IsNullOrWhiteSpace(excelPath) || !File.Exists(excelPath))
                throw new FileNotFoundException("Excel-Datei nicht gefunden.", excelPath);

            if (_cmbTemplate.SelectedItem is not TemplateItem ti)
                throw new InvalidOperationException("Bitte eine LBX-Vorlage wählen.");

            var printerName = _cmbPrinter.SelectedItem?.ToString();

            var reader = new ExcelReader();
            var (headers, rows) = await Task.Run(() => reader.Read(excelPath, _cfg.Defaults.SheetName));

            var map = new MappingResolver(_cfg, headers);

            _lblStatus.Text = "Starte Druck…";
            int totalLabels = 0;

            using var pt = new BrotherPtPrinter();
            pt.OpenTemplate(Path.Combine(AppContext.BaseDirectory, ti.Template.File));
            pt.SelectPrinter(printerName);
            pt.StartPrint();

            string get(string logical)
            {
                var header = headers.FirstOrDefault(h => string.Equals(map.GetLogicalByHeader(h), logical, StringComparison.OrdinalIgnoreCase));
                if (header == null) return "";
                var rowObj = _currentRow!;
                return rowObj.Cells.TryGetValue(header, out var v) ? v?.ToString() ?? "" : "";
            }

            foreach (var row in rows)
            {
                _currentRow = row;

                var artikel = get("artikelnummer");
                var name    = get("text");
                var beschr  = get("beschreibung");
                var preis   = MappingResolver.FormatPrice(get("preis"), _cfg.Defaults.DecimalSeparator ?? ",");
                var ean     = MappingResolver.NormalizeEan(get("ean"));

                foreach (var kv in ti.Template.ObjectMap)
                {
                    var value = kv.Key switch
                    {
                        "artikelnummer" => artikel,
                        "text"          => name,
                        "beschreibung"  => beschr,
                        "preis"         => preis,
                        "ean"           => ean,
                        _               => ""
                    };
                    if (!string.IsNullOrEmpty(kv.Value))
                        pt.SetField(kv.Value, value);
                }

                int copies = map.ParseCopies(get("menge"), _cfg.Defaults.CopiesIfMissing);
                if (copies > 0)
                {
                    pt.PrintCopies(copies);
                    totalLabels += copies;
                }
            }

            pt.EndPrint();
            _lblStatus.Text = $"Fertig. Gedruckte Etiketten: {totalLabels}";
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Druckfehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
            _lblStatus.Text = "Fehler.";
        }
        finally
        {
            _btnPrint.Enabled = true;
        }
    }

    // kleine Hilfe, um im get()-Lambda auf die aktuelle Zeile zugreifen zu können
    private ExcelRow? _currentRow;

    private sealed record TemplateItem(TemplateConfig Template)
    {
        public override string ToString() => Template.Title;
    }
}
