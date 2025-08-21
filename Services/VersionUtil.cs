using System;
using System.IO;

namespace LabelPrinter.Services
{
    public static class VersionUtil
    {
        /// <summary>
        /// Liest die Basisversion aus VERSION.txt (z.B. "1.0.7".
        /// Fallback: "1.0", falls Datei fehlt/ungültig.
        /// </summary>
        public static string GetBaseVersion()
        {
            try
            {
                var path = Path.Combine(AppContext.BaseDirectory, "VERSION.txt");
                if (!File.Exists(path)) return "1.0";
                var v = File.ReadAllText(path).Trim(); // z.B. "1.1.5"
                return v;
            }
            catch
            {
                return "1.0";
            }
        }
    }
}
