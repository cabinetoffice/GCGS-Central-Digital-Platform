namespace CO.CDP.Forms.WebApi.Model;

public record FormAnswerSet
{
    public required Guid Id { get; init; }
    public required List<FormAnswer> Answers { get; init; }
    public required bool FurtherQuestionsExempted { get; set; }
}