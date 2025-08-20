using System.Globalization;
using System.Linq;
using LabelPrinter.Models;

namespace LabelPrinter.Services;

public sealed class MappingResolver
{
    private readonly AppConfig _cfg;
    private readonly Dictionary<string, string> _headerToLogical;

    public MappingResolver(AppConfig cfg, IEnumerable<string> headers)
    {
        _cfg = cfg;
        _headerToLogical = BuildHeaderMap(headers);
    }

    private Dictionary<string, string> BuildHeaderMap(IEnumerable<string> headers)
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var header in headers)
        {
            foreach (var (logical, aliases) in _cfg.ColumnAliases)
            {
                if (aliases.Any(a => string.Equals(a, header, StringComparison.OrdinalIgnoreCase)))
                {
                    map[header] = logical;
                    break;
                }
            }
        }
        return map;
    }

    public string? GetLogicalByHeader(string header)
        => _headerToLogical.TryGetValue(header, out var logical) ? logical : null;

    public string GetString(ExcelRow row, string logical)
    {
        if (_cfg.ColumnAliases.TryGetValue(logical, out var aliases))
        {
            foreach (var alias in aliases)
            {
                if (row.Cells.TryGetValue(alias, out var val))
                {
                    var s = val?.ToString()?.Trim();
                    if (!string.IsNullOrEmpty(s))
                        return s;
                }
            }
        }

        var header = row.Cells.Keys.FirstOrDefault(h =>
            string.Equals(GetLogicalByHeader(h), logical, StringComparison.OrdinalIgnoreCase));
        if (header != null && row.Cells.TryGetValue(header, out var fallback) && fallback != null)
            return fallback.ToString() ?? string.Empty;

        return string.Empty;
    }

    public static string NormalizeEan(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return "";
        var digits = new string(raw.Where(char.IsDigit).ToArray());

        if (digits.Length == 12) // fehlende Prüfziffer → berechnen
            digits += CalcEan13CheckDigit(digits);

        return digits;
    }

    private static char CalcEan13CheckDigit(string twelveDigits)
    {
        int sum = 0;
        for (int i = 0; i < twelveDigits.Length; i++)
        {
            int d = twelveDigits[i] - '0';
            sum += (i % 2 == 0) ? d : 3 * d;
        }
        int check = (10 - (sum % 10)) % 10;
        return (char)('0' + check);
    }

    public int ParseCopies(object? value, int fallback)
    {
        if (value == null) return fallback;
        if (value is double d) return Math.Max(0, (int)Math.Round(d));
        var s = value.ToString()?.Trim();
        if (string.IsNullOrEmpty(s)) return fallback;
        if (int.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var i)) return Math.Max(0, i);
        if (int.TryParse(s, out i)) return Math.Max(0, i);
        return fallback;
    }

    public static string FormatPrice(object? value, string decimalSep = ",")
    {
        if (value == null) return string.Empty;

        var culture = new CultureInfo(decimalSep == "," ? "de-CH" : "en-US");

        if (value is double d)
            return d.ToString("0.00", culture);

        var s = value.ToString()?.Trim();
        if (string.IsNullOrEmpty(s)) return string.Empty;

        s = s.Replace(decimalSep == "," ? "." : ",", decimalSep);
        if (decimal.TryParse(s, NumberStyles.Any, culture, out var dec))
            return dec.ToString("0.00", culture);

        return s;
    }
}
