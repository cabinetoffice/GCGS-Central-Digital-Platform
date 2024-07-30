namespace CO.CDP.Forms.WebApi.Model;

public record UpdateFormSectionAnswers
{
    public List<FormAnswer>? Answers { get; init; }
}
