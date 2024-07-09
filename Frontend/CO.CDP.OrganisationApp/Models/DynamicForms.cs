namespace CO.CDP.OrganisationApp.Models;

public class SectionQuestionsResponse
{
    public FormSection? Section { get; set; }
    public List<FormQuestion>? Questions { get; set; }
}

public class FormSection
{
    public Guid Id { get; set; }
    public string? Title { get; set; }
    public bool AllowsMultipleAnswerSets { get; set; }
}

public class FormQuestion
{
    public Guid Id { get; set; }
    public FormQuestion? NextQuestion { get; set; }
    public FormQuestion? NextQuestionAlternative { get; set; }
    public FormQuestionType Type { get; set; }
    public bool IsRequired { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public FormQuestionOptions Options { get; set; } = new();
}

public class FormQuestionOptions
{
    public List<string>? Choices { get; set; }
    public string? ChoiceProviderStrategy { get; set; }
}

public enum FormQuestionType
{
    NoInput,
    Text,
    FileUpload,
    YesOrNo,
    SingleChoice,
    MultipleChoice,
    CheckYourAnswers,
    Date
}
