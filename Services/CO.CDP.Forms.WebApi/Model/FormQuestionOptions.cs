namespace CO.CDP.Forms.WebApi.Model;

public record FormQuestionOptions
{
    public List<FormQuestionChoice>? Choices { get; init; } = new();
    public string? ChoiceProviderStrategy { get; init; }
}
