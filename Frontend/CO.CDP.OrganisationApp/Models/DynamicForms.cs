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
    public FormQuestionBranchType BranchType { get; set; } = FormQuestionBranchType.Main;
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
    public List<FormQuestionGroup> Groups { get; set; } = [];
    public string? ChoiceProviderStrategy { get; set; }
    public string? AnswerFieldName { get; set; }
    public FormQuestionGrouping? Grouping { get; set; }
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
    public bool? BoolValue { get; set; }
    public double? NumericValue { get; set; }
    public DateTimeOffset? DateValue { get; set; }
    public DateTimeOffset? StartValue { get; set; }
    public DateTimeOffset? EndValue { get; set; }
    public string? TextValue { get; set; }
    public string? OptionValue { get; set; }
    public Address? AddressValue { get; set; }
    public string? JsonValue { get; set; }
}

public class FormQuestionGroup
{
    public string? Name { get; set; }
    public string? Hint { get; set; }
    public string? Caption { get; set; }
    public List<FormQuestionGroupChoice> Choices { get; set; } = [];
}

public class FormQuestionGroupChoice
{
    public string? Title { get; set; }
    public string? Value { get; set; } = null;
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
    GroupedSingleChoice,
    Url,
}

public enum FormQuestionBranchType
{
    /// <summary>
    /// Represents questions that are part of the main flow of the form.
    /// These questions are connected via NextQuestion links in sequence.
    /// </summary>
    Main,

    /// <summary>
    /// Represents questions that are part of an alternative path in the form.
    /// A question is considered an alternative path if it:
    /// 1. Is linked to by another question as a NextQuestionAlternative, or
    /// 2. Follows on from a NextQuestionAlternative.
    /// Alternative paths typically represent conditional branches following a 'no' response in the form flow.
    /// </summary>
    Alternative
}


/// <summary>
/// Represents a collection of questions that should be displayed on the same page
/// </summary>
public class MultiQuestionPageModel
{
    public List<FormQuestion> Questions { get; set; } = [];
    public string? PageTitleResourceKey { get; set; }
    public string? SubmitButtonTextResourceKey { get; set; }
}