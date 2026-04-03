using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace LabelPrinter;

public sealed class PreviewForm : Form
{
    public BindingList<PreviewItem> Items { get; }

    public PreviewForm(IEnumerable<PreviewItem> items)
    {
        Items = new BindingList<PreviewItem>(items.ToList());

        AutoScaleMode = AutoScaleMode.Dpi;
        Font = new Font("Segoe UI", 10f);
        Text = "Druckvorschau";
        StartPosition = FormStartPosition.CenterParent;
        MinimizeBox = false;
        MaximizeBox = false;
        ClientSize = new Size(1400, 820);
        MinimumSize = new Size(1200, 700);
        
        var grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = true,
            AllowUserToResizeRows = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            RowHeadersVisible = false,
            DataSource = Items
        };
        grid.DataBindingComplete += (_, __) =>
        {
            grid.Columns[nameof(PreviewItem.Artikelnummer)].ReadOnly = true;
            grid.Columns[nameof(PreviewItem.Text)].ReadOnly = true;
            grid.Columns[nameof(PreviewItem.Ean)].ReadOnly = true;

            grid.Columns[nameof(PreviewItem.Artikelnummer)].HeaderText = "Artikelnummer";
            grid.Columns[nameof(PreviewItem.Text)].HeaderText = "Text";
            grid.Columns[nameof(PreviewItem.Ean)].HeaderText = "EAN";
            grid.Columns[nameof(PreviewItem.Preis)].HeaderText = "Preis";
            grid.Columns[nameof(PreviewItem.Menge)].HeaderText = "Menge";
        };

        var btnPrint = new Button { Text = "Drucken", DialogResult = DialogResult.OK, AutoSize = true, Margin = new Padding(12,0,0,0) };
        btnPrint.Click += (_, __) => grid.EndEdit();

        var btnCancel = new Button { Text = "Abbrechen", DialogResult = DialogResult.Cancel, AutoSize = true };

        var numAllMenge = new NumericUpDown
        {
            AutoSize = true,
            Minimum = 0,
            Maximum = 100000,
            Value = 1,
            Width = 80
        };

        var btnSetAllMenge = new Button { Text = "Menge für alle setzen", AutoSize = true };
        btnSetAllMenge.Click += (_, __) =>
        {
            grid.EndEdit();
            int menge = (int)numAllMenge.Value;
            foreach (var it in Items)
                it.Menge = menge;
            grid.Refresh();
        };

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            FlowDirection = FlowDirection.RightToLeft,
            AutoSize = true,
            Padding = new Padding(12)
        };

        // Reihenfolge wichtig wegen RightToLeft:
        buttons.Controls.Add(btnPrint);   // rechts außen
        buttons.Controls.Add(btnCancel);  // daneben links
        buttons.Controls.Add(btnSetAllMenge);
        buttons.Controls.Add(numAllMenge);

        Controls.Add(grid);
        Controls.Add(buttons);

        AcceptButton = btnPrint;
        CancelButton = btnCancel;
    }

    public sealed class PreviewItem
    {
        public PreviewItem(string artikelnummer, string text, string ean, string preis, int menge)
        {
            Artikelnummer = artikelnummer;
            Text = text;
            Ean = ean;
            Preis = preis;
            Menge = menge;
        }

        public string Artikelnummer { get; init; }
        public string Text { get; init; }
        public string Ean { get; init; }
        public string Preis { get; set; }
        public int Menge { get; set; }
    }
}
