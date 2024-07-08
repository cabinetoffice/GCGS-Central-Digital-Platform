namespace CO.CDP.Forms.WebApi.Model;

public record FormSection
{
    /// <example>"a3f828e7-93c4-4b44-82d3-ded3ab5f8b0d"</example>
    public required Guid Id { get; init; }
    public required string Title { get; init; }
    public required bool AllowsMultipleAnswerSets { get; init; }
}