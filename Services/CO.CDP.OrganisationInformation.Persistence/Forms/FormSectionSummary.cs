namespace CO.CDP.OrganisationInformation.Persistence.Forms;

public record FormSectionSummary
{
    public required Guid SectionId { get; init; }
    public required string SectionName { get; init; }
    public required bool AllowsMultipleAnswerSets { get; init; }
    public required int AnswerSetCount { get; init; }
}