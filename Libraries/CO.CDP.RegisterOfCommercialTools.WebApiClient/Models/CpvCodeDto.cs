namespace CO.CDP.RegisterOfCommercialTools.WebApiClient.Models;

public class CpvCodeDto
{
    public required string Code { get; set; }
    public required string DescriptionEn { get; set; }
    public required string DescriptionCy { get; set; }
    public string? ParentCode { get; set; }
    public int Level { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedOn { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedOn { get; set; } = DateTimeOffset.UtcNow;
    public List<CpvCodeDto> Children { get; set; } = [];
    
    public string GetDescription(Culture culture = Culture.English)
    {
        return culture == Culture.Welsh ? DescriptionCy : DescriptionEn;
    }
}

public enum Culture
{
    English,
    Welsh
}