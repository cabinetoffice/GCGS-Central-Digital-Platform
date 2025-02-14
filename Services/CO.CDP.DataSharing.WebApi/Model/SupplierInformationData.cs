namespace CO.CDP.DataSharing.WebApi.Model;

public record SupplierInformationData
{
    public required Form Form { get; init; }
    public required List<FormAnswerSet> AnswerSets { get; init; } = [];
    public required List<FormQuestion> Questions { get; set; } = [];
}