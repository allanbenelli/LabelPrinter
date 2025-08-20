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
        Size = new Size(500, 400);

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

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            FlowDirection = FlowDirection.RightToLeft,
            AutoSize = true,
            Padding = new Padding(12)
        };
        buttons.Controls.Add(btnPrint);
        buttons.Controls.Add(btnCancel);

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

