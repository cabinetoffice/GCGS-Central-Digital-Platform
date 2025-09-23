namespace CO.CDP.RegisterOfCommercialTools.WebApiClient.Models;

public interface IHierarchicalCodeDto
{
    string Code { get; set; }
    string DescriptionEn { get; set; }
    string DescriptionCy { get; set; }
    string? ParentCode { get; set; }
    int Level { get; set; }
    bool IsActive { get; set; }
    DateTimeOffset CreatedOn { get; set; }
    DateTimeOffset UpdatedOn { get; set; }
    bool HasChildren { get; set; }
    string GetDescription(Culture culture = Culture.English);
}