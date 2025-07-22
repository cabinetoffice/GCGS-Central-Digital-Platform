namespace CO.CDP.Forms.WebApi.Model;

public record FormQuestionOptions
{
    public List<FormQuestionChoice>? Choices { get; init; } = new();
    public string? ChoiceProviderStrategy { get; init; }
    public string? AnswerFieldName { get; init; }
    public List<FormQuestionGroup>? Groups { get; set; } = new();
    public FormQuestionGrouping? Grouping { get; set; }
}