namespace CO.CDP.OrganisationApp.Models;

public class SectionQuestionsResponse
{
    public FormSection? Section { get; set; }
    public List<FormQuestion> Questions { get; set; } = [];
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
    public Guid? NextQuestion { get; set; }
    public Guid? NextQuestionAlternative { get; set; }
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

public class FormQuestionAnswerState
{
    public Guid? AnswerSetId { get; set; }
    public List<QuestionAnswer> Answers { get; set; } = [];
}

public class QuestionAnswer
{
    public Guid QuestionId { get; set; }
    public FormAnswer? Answer { get; set; }
}

public class FormAnswer
{
    public bool? BoolValue { get; init; }
    public double? NumericValue { get; init; }
    public DateTime? DateValue { get; init; }
    public DateTime? StartValue { get; init; }
    public DateTime? EndValue { get; init; }
    public string? TextValue { get; init; }
    public string? OptionValue { get; init; }
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
