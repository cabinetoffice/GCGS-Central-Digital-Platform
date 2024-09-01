namespace CO.CDP.DataSharing.WebApi.Model;

public record SupplierInformationData
{
    public required Form Form { get; init; }
    public required IList<FormAnswerSet> FormAnswerSets { get; init; } = [];
}