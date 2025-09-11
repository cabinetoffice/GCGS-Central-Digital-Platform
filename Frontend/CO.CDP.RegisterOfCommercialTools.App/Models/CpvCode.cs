namespace CO.CDP.RegisterOfCommercialTools.App.Models;

public class CpvCode
{
    public string Code { get; set; } = string.Empty;
    public string DescriptionEn { get; set; } = string.Empty;
    public string DescriptionCy { get; set; } = string.Empty;

    public List<CpvCode> Children { get; set; } = [];

    public CpvCode? Parent { get; set; }

    public int Level { get; set; }

    public bool IsSelected { get; set; }

    public bool IsExpanded { get; set; }

    public string GetDescription(Culture culture = Culture.English)
    {
        return culture.IsWelsh() ? DescriptionCy : DescriptionEn;
    }

    public string GetFullDescription(Culture culture = Culture.English) => $"{Code}: {GetDescription(culture)}";

    public bool IsLeafNode => Children.Count == 0;
    public bool HasChildren => Children.Count != 0;

    public string InputId => $"cpv_codes_{Code}";

    public string LabelId => $"cpv_codes_{Code}_label";
}