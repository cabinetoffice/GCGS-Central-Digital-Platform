namespace CO.CDP.DataSharing.WebApi.Model;

public record SupplierInformationData
{
    public required Form Form { get; init; }
    public required ICollection<FormAnswerSet> AnswerSets { get; init; } = [];
    public required ICollection<FormQuestion> Questions { get; set; } = [];
}