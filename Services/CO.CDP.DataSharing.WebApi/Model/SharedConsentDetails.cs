namespace CO.CDP.DataSharing.WebApi.Model;

public class SharedConsentDetails
{
    public required string ShareCode { get; set; }
    public DateTimeOffset SubmittedAt { get; set; }
    public IEnumerable<SharedConsentQuestionAnswer>? QuestionAnswers { get; set; }
}

public class SharedConsentQuestionAnswer
{
    public Guid QuestionId { get; set; }
    public required string Title { get; set; }
    public required string? Answer { get; set; }
}