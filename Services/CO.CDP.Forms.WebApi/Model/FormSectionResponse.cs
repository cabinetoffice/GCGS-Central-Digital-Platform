namespace CO.CDP.Forms.WebApi.Model;

public record FormSectionResponse
{
    public required List<FormSectionSummary> FormSections { get; init; } = [];
}

public record FormSectionSummary
{
    public required Guid SectionId { get; init; }
    public required string SectionName { get; init; }
    public required FormSectionType Type { get; init; }
    public required bool AllowsMultipleAnswerSets { get; init; }
    public required int AnswerSetCount { get; init; }
    public required bool AnswerSetWithFurtherQuestionExemptedExists { get; init; }
}