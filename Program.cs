using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace LabelPrinter;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        if (!EnsureBpacPresent())
        {
            MessageBox.Show(
                "Das Brother b-PAC Runtime ist nicht installiert oder konnte nicht installiert werden.\n" +
                "Bitte installieren Sie es manuell und starten Sie die Anwendung erneut.",
                "b-PAC erforderlich", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        Application.Run(new MainForm());
    }

    private static bool EnsureBpacPresent()
    {
        if (IsBpacAvailable()) return true;

        var bpacDir = Path.Combine(AppContext.BaseDirectory, "bpac");
        string? installer = null;
        if (Directory.Exists(bpacDir))
            installer = Directory.EnumerateFiles(bpacDir, "*.exe", SearchOption.TopDirectoryOnly).FirstOrDefault();

        if (installer == null) return false; // Kein Installer mitgeliefert

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = installer,
                Arguments = "/S",          // ggf. /VERYSILENT oder /quiet, je nach Version
                UseShellExecute = true,
                Verb = "runas"             // UAC Elevation
            };
            using var p = Process.Start(psi);
            p?.WaitForExit();
        }
        catch { return false; }

        return IsBpacAvailable();
    }

    private static bool IsBpacAvailable()
    {
        return Type.GetTypeFromProgID("bpac.Document") != null;
    }
}
