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
        ClientSize = new Size(1100, 700);
        MinimumSize = new Size(900, 620);
        
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
            grid.Columns[nameof(PreviewItem.Artikelnummer)].ReadOnly = true;

        var btnPrint = new Button { Text = "Drucken", DialogResult = DialogResult.OK, AutoSize = true, Margin = new Padding(12,0,0,0) };
        btnPrint.Click += (_, __) => grid.EndEdit();

        var btnCancel = new Button { Text = "Abbrechen", DialogResult = DialogResult.Cancel, AutoSize = true };

        var btnAllOne = new Button { Text = "Alle Mengen = 1", AutoSize = true };
        btnAllOne.Click += (_, __) =>
        {
            grid.EndEdit();
            foreach (var it in Items)
                it.Menge = 1;
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
        buttons.Controls.Add(btnAllOne);  // ganz links

        Controls.Add(grid);
        Controls.Add(buttons);

        AcceptButton = btnPrint;
        CancelButton = btnCancel;
    }

    public sealed class PreviewItem
    {
        public PreviewItem(string artikelnummer, string preis, int menge)
        {
            Artikelnummer = artikelnummer;
            Preis = preis;
            Menge = menge;
        }

        public string Artikelnummer { get; init; }
        public string Preis { get; set; }
        public int Menge { get; set; }
    }
}
