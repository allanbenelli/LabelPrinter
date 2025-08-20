using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace LabelPrinter;

public sealed class PreviewForm : Form
{
    public PreviewForm(List<PreviewItem> items)
    {
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
            ReadOnly = true,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AllowUserToResizeRows = false,
            RowHeadersVisible = false,
            DataSource = items
        };

        var btnPrint = new Button { Text = "Drucken", DialogResult = DialogResult.OK, AutoSize = true, Margin = new Padding(12,0,0,0) };
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

    public sealed record PreviewItem(string Artikelnummer, string Preis, int Menge);
}