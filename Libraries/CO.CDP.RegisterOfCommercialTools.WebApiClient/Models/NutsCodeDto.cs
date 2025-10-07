namespace CO.CDP.RegisterOfCommercialTools.WebApiClient.Models;

public class NutsCodeDto : IHierarchicalCodeDto
{
    public required string Code { get; set; }
    public required string DescriptionEn { get; set; }
    public required string DescriptionCy { get; set; }
    public string? ParentCode { get; set; }
    public string? ParentDescriptionEn { get; set; }
    public string? ParentDescriptionCy { get; set; }
    public int Level { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsSelectable { get; set; } = true;
    public DateTimeOffset CreatedOn { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedOn { get; set; } = DateTimeOffset.UtcNow;
    public List<NutsCodeDto> Children { get; set; } = [];
    public bool HasChildren { get; set; }

    public string GetDescription(Culture culture = Culture.English)
    {
        return culture == Culture.Welsh ? DescriptionCy : DescriptionEn;
    }

    public string? GetParentDescription(Culture culture = Culture.English)
    {
        if (string.IsNullOrEmpty(ParentDescriptionEn) && string.IsNullOrEmpty(ParentDescriptionCy))
            return null;

        return culture == Culture.Welsh ? ParentDescriptionCy : ParentDescriptionEn;
    }
}