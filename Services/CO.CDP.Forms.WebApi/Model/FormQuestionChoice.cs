namespace CO.CDP.Forms.WebApi.Model;

public record FormQuestionChoice
{
    /// <example>"d5a9c1a1-2a3b-4c9a-9c7b-2d1f4c3a9a1d"</example>
    public required Guid Id { get; init; }
    public required string Title { get; init; }
    public string? GroupName { get; init; }
    public FormQuestionChoiceHint? Hint { get; init; }
}
