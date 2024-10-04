namespace CO.CDP.Forms.WebApi.Model;

public class FormQuestionGroup
{
    public required string Name { get; set; }
    public required string Hint { get; set; }
    public required string Caption { get; set; }
    public ICollection<FormQuestionGroupChoice>? Choices { get; set; }
}

public class FormQuestionGroupChoice
{
    public required string? Title { get; set; }
    public required string? Value { get; set; } = null;
}