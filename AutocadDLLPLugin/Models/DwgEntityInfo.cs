namespace AutocadDLLPLugin.Models;

public class DwgEntityInfo
{
    public string EntityType { get; set; } = string.Empty;

    public string Layer { get; set; } = string.Empty;

    public string Handle { get; set; } = string.Empty;

    public string Color { get; set; } = string.Empty;

    public string Linetype { get; set; } = string.Empty;

    public Dictionary<string, object?> Properties { get; set; } = new();
}