namespace LabelPrinter.Models;

public sealed class AppConfig
{
    public List<TemplateConfig> Templates { get; set; } = new();
    public Dictionary<string, List<string>> ColumnAliases { get; set; } = new();
    public Defaults Defaults { get; set; } = new();
    public List<string> IgnoredArticles { get; set; } = new();
}

public sealed class TemplateConfig
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public string File { get; set; } = "";
    public Dictionary<string, string> ObjectMap { get; set; } = new(); // logicalField -> lbxObjectName
}

public sealed class Defaults
{
    public string? SheetName { get; set; }
    public int CopiesIfMissing { get; set; } = 1;
    public string? DecimalSeparator { get; set; } = ".";
}
