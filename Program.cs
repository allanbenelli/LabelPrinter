using System;
using System.Windows.Forms;

namespace LabelPrinter;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        // Prüfen, ob b-PAC COM verfügbar ist
        if (Type.GetTypeFromProgID("bpac.Document") == null)
        {
            MessageBox.Show(
                "Brother b-PAC Runtime ist nicht installiert.\n" +
                "Bitte installieren Sie es und starten Sie die Anwendung erneut.\n\n" +
                "Hinweis: Im Ordner 'bpac' finden Sie den Installer, oder laden Sie ihn von der Brother-Website.",
                "Komponente fehlt",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
            return;
        }

        Application.Run(new MainForm());
    }
}
