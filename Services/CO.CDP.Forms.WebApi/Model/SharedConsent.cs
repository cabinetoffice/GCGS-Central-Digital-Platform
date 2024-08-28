using System.Text.Json.Serialization;

namespace CO.CDP.Forms.WebApi.Model;

public class SharedConsent
{
    public Guid Guid { get; set; }
    public int OrganisationId { get; set; }
    public IList<FormAnswerSet> AnswerSets { get; set; } = [];
    public SubmissionState SubmissionState { get; set; }
    public DateTimeOffset SubmittedAt { get; set; }
    public string? FormVersionId { get; set; }
    public string? ShareCode { get; set; }

}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SubmissionState
{
    Draft,
    Submitted
}