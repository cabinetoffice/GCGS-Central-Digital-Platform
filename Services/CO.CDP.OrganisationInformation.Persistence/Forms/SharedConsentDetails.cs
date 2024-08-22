namespace CO.CDP.OrganisationInformation.Persistence.Forms;

public class SharedConsentDetails
{
    public required string ShareCode { get; set; }
    public DateTimeOffset SubmittedAt { get; set; }
    public IEnumerable<SharedConsentQuestionAnswer>? QuestionAnswers { get; set; }
}

public class SharedConsentQuestionAnswer
{
    public Guid QuestionId { get; set; }
    public FormQuestionType QuestionType { get; set; }
    public required string Title { get; set; }
    public required FormAnswer Answer { get; set; }
}