namespace CO.CDP.DataSharing.WebApi.Model;

public record SupplierInformationData
{
    public required Form Form { get; init; }
    public required List<FormAnswer> Answers { get; init; } = new();
    public required List<FormQuestion> Questions { get; init; } = new();
}