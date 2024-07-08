namespace CO.CDP.Forms.WebApi.Model;

public record FormQuestionChoiceHint
{
    public string? Title { get; init; }
    public required string Description { get; init; }
}