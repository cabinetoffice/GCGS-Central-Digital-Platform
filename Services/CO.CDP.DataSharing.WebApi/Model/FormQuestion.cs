namespace CO.CDP.DataSharing.WebApi.Model;

internal record FormQuestion
{
    public FormQuestionType? Type { get; init; }
    public string? Name { get; init; }
    public string? Text { get; init; }
    public bool IsRequired { get; init; }
    public string? SectionName { get; init; }
    public List<QuestionOption> Options { get; init; } = new List<QuestionOption>();
}