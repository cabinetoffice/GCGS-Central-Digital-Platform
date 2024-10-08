using CO.CDP.OrganisationInformation.Persistence.Forms;

namespace CO.CDP.DataSharing.WebApi.Model;

public record FormAnswerSetForPdf
{
    public required string SectionName { get; init; }
    public required FormSectionType SectionType { get; init; }
    public required List<Tuple<string, string>> QuestionAnswers { get; init; }
}