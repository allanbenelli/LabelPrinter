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
    private ToolStripStatusLabel _status = null!;

    private AppConfig _cfg = new();
    private ExcelRow? _currentRow;

    public MainForm()
    {
        // Fenster-Basics
        AutoScaleMode = AutoScaleMode.Dpi;
        Font = new Font("Segoe UI", 10f);
        Text = "P-touch Excel Label Printer";
        StartPosition = FormStartPosition.CenterScreen;
        Padding = new Padding(12);
        ClientSize = new Size(900, 480);
        MinimumSize = new Size(760, 480);

        // === Inputs (oben) =====================================================
        var inputs = new TableLayoutPanel
        {
            Dock = DockStyle.Top,     // NICHT füllen -> bleibt kompakt
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 3,
            Padding = new Padding(0, 0, 0, 8)
        };
        inputs.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));       // Label
        inputs.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));  // Eingabe
        inputs.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));       // Button

        // Zeilen: Excel / Vorlage / Drucker
        inputs.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        inputs.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        inputs.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        // Excel
        var lblExcel = new Label { Text = "Excel-Datei:", AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 6, 12, 6) };
        _txtExcel = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right, Width = 600, PlaceholderText = "Pfad zur Excel-Datei…", Margin = new Padding(0, 2, 12, 2) };
        _txtExcel.AllowDrop = true;
        _txtExcel.DragEnter += (s, e) => { if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true) e.Effect = DragDropEffects.Copy; };
        _txtExcel.DragDrop += (s, e) =>
        {
            if (e.Data?.GetData(DataFormats.FileDrop) is string[] files && files.Length > 0)
                _txtExcel.Text = files[0];
        };
        _btnBrowse = new Button { Text = "Durchsuchen…", AutoSize = true, Margin = new Padding(0, 0, 0, 0) };
        _btnBrowse.Click += (_, __) => BrowseExcel();

        inputs.Controls.Add(lblExcel,   0, 0);
        inputs.Controls.Add(_txtExcel,  1, 0);
        inputs.Controls.Add(_btnBrowse, 2, 0);

        // Vorlage
        var lblTemplate = new Label { Text = "Vorlage:", AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 6, 12, 6) };
        _cmbTemplate = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Anchor = AnchorStyles.Left | AnchorStyles.Right, Width = 600, Margin = new Padding(0, 2, 12, 2) };
        inputs.Controls.Add(lblTemplate,  0, 1);
        inputs.Controls.Add(_cmbTemplate, 1, 1);
        inputs.Controls.Add(new Panel { Width = 1 }, 2, 1); // Platzhalter

        // Drucker
        var lblPrinter = new Label { Text = "Drucker:", AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 6, 12, 6) };
        _cmbPrinter = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Anchor = AnchorStyles.Left | AnchorStyles.Right, Width = 600, Margin = new Padding(0, 2, 12, 2) };
        inputs.Controls.Add(lblPrinter,  0, 2);
        inputs.Controls.Add(_cmbPrinter, 1, 2);
        inputs.Controls.Add(new Panel { Width = 1 }, 2, 2);

        Controls.Add(inputs);

        // === Bottom-Bar: Status + Buttons =====================================
        var bottom = new Panel { Dock = DockStyle.Bottom, Height = 56, Padding = new Padding(0, 8, 0, 0) };
        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Right,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false
        };
        _btnPrint = new Button
        {
            Text = "Drucken",
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Padding = new Padding(16, 6, 16, 6),
            Margin = new Padding(12, 0, 0, 0)
        };
        _btnPrint.Click += async (_, __) => await PrintAsync();
        AcceptButton = _btnPrint;
        buttons.Controls.Add(_btnPrint);
        bottom.Controls.Add(buttons);
        Controls.Add(bottom);

        // Statusleiste
        var strip = new StatusStrip { Dock = DockStyle.Bottom, SizingGrip = false };
        _status = new ToolStripStatusLabel("Bereit.");
        strip.Items.Add(_status);
        Controls.Add(strip);

        // Laden
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
        _cfg = File.Exists(path)
            ? (JsonSerializer.Deserialize<AppConfig>(File.ReadAllText(path),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new AppConfig())
            : new AppConfig();
        if (!File.Exists(path))
            MessageBox.Show($"Konfiguration fehlt:\n{path}", "Hinweis", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void LoadTemplates()
    {
        _cmbTemplate.Items.Clear();
        foreach (var t in _cfg.Templates)
            _cmbTemplate.Items.Add(new TemplateItem(t));
        if (_cmbTemplate.Items.Count > 0) _cmbTemplate.SelectedIndex = 0;
    }

    private void LoadPrinters()
    {
        _cmbPrinter.Items.Clear();
        string? brother = null;
        foreach (string p in PrinterSettings.InstalledPrinters)
        {
            _cmbPrinter.Items.Add(p);
            if (brother == null && p.Contains("Brother", StringComparison.OrdinalIgnoreCase))
                brother = p;
        }
        if (brother != null) _cmbPrinter.SelectedItem = brother;
        else if (_cmbPrinter.Items.Count > 0) _cmbPrinter.SelectedIndex = 0;
    }

    private void BrowseExcel()
    {
        using var ofd = new OpenFileDialog { Filter = "Excel (*.xlsx;*.xls)|*.xlsx;*.xls|Alle Dateien (*.*)|*.*" };
        if (ofd.ShowDialog() == DialogResult.OK)
            _txtExcel.Text = ofd.FileName;
    }

    private async Task PrintAsync()
    {
        try
        {
            _btnPrint.Enabled = false;
            _status.Text = "Lese Excel…";

            var excelPath = _txtExcel.Text;
            if (string.IsNullOrWhiteSpace(excelPath) || !File.Exists(excelPath))
                throw new FileNotFoundException("Excel-Datei nicht gefunden.", excelPath);

            if (_cmbTemplate.SelectedItem is not TemplateItem ti)
                throw new InvalidOperationException("Bitte eine LBX-Vorlage wählen.");

            var printerName = _cmbPrinter.SelectedItem?.ToString();

            var reader = new ExcelReader();
            var (headers, rows) = await Task.Run(() => reader.Read(excelPath, _cfg.Defaults.SheetName));
            var map = new MappingResolver(_cfg, headers);

            _status.Text = "Starte Druck…";
            int totalLabels = 0;

            using var pt = new BrotherPtPrinter();
            pt.OpenTemplate(Path.Combine(AppContext.BaseDirectory, ti.Template.File));
            pt.SelectPrinter(printerName);
            pt.StartPrint();

            string get(string logical)
            {
                var header = headers.FirstOrDefault(h => string.Equals(map.GetLogicalByHeader(h), logical, StringComparison.OrdinalIgnoreCase));
                if (header == null) return "";
                return _currentRow!.Cells.TryGetValue(header, out var v) ? v?.ToString() ?? "" : "";
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
            _status.Text = $"Fertig. Gedruckte Etiketten: {totalLabels}";
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Druckfehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
            _status.Text = "Fehler.";
        }
        finally
        {
            _btnPrint.Enabled = true;
        }
    }

    private sealed record TemplateItem(TemplateConfig Template)
    {
        public override string ToString() => Template.Title;
    }
}
