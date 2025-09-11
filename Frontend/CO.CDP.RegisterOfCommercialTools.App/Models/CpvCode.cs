namespace CO.CDP.RegisterOfCommercialTools.App.Models;

public class CpvCode
{
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public List<CpvCode> Children { get; set; } = [];

    public CpvCode? Parent { get; set; }

    public int Level { get; set; }

    public bool IsSelected { get; set; }

    public bool IsExpanded { get; set; }

    public string FullDescription => $"{Code}: {Description}";

    public bool IsLeafNode => Children.Count == 0;
    public bool HasChildren => Children.Count != 0;

    public string InputId => $"cpv_codes_{Code}";

    public string LabelId => $"cpv_codes_{Code}_label";

    public string AriaLabel => HasChildren
        ? $"Expand {Description} for more codes"
        : $"Select {Description}";
}