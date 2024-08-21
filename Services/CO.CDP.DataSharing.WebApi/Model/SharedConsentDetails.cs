using System.Text.Json.Serialization;

namespace CO.CDP.DataSharing.WebApi.Model;

public class SharedConsentDetails
{
    public DateTimeOffset SubmittedAt { get; set; }
    public string? ShareCode { get; set; }
    public List<SharedConsentQuestionAnswer>? SharedConsentQuestionAnswers { get; set; }
}

public class SharedConsentQuestionAnswer
{
    Guid QuestionId { get; set; }
    string? Question_Summary_Title { get; set; }
    string? Question_Answer { get; set; }
}

