namespace CO.CDP.OrganisationApp.Models;

public class SectionQuestionsResponse
{
    public FormSection? Section { get; set; }
    public List<FormQuestion> Questions { get; set; } = [];
}

public class FormSection
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public FormSectionType Type { get; set; }
    public bool AllowsMultipleAnswerSets { get; set; }
}

public enum FormSectionType
{
    Standard,
    Declaration
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
    public string? Caption { get; set; }
    public string? SummaryTitle { get; set; }
    public FormQuestionOptions Options { get; set; } = new();
}

public class FormQuestionOptions
{
    public Dictionary<string, string>? Choices { get; set; }
    public string? ChoiceProviderStrategy { get; set; }
    public string? ChoiceAnswerFieldName { get; set; }
}

public class FormQuestionAnswerState
{
    public Guid? AnswerSetId { get; set; }
    public List<QuestionAnswer> Answers { get; set; } = [];
    public bool FurtherQuestionsExempted { get; set; }
}

public class QuestionAnswer
{
    public Guid QuestionId { get; set; }
    public Guid AnswerId { get; set; }
    public FormAnswer? Answer { get; set; }
}

public class FormAnswer
{
    public bool? BoolValue { get; init; }
    public double? NumericValue { get; init; }
    public DateTimeOffset? DateValue { get; init; }
    public DateTimeOffset? StartValue { get; init; }
    public DateTimeOffset? EndValue { get; init; }
    public string? TextValue { get; init; }
    public string? OptionValue { get; init; }
    public Address? AddressValue { get; init; }
    public string? JsonValue { get; init; }
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
    Date,
    CheckBox,
    Address,
    MultiLine,
    Url
}