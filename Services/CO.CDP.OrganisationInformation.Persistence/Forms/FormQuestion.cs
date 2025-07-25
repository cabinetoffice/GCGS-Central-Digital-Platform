using CO.CDP.EntityFrameworkCore.Timestamps;
using Microsoft.EntityFrameworkCore;

namespace CO.CDP.OrganisationInformation.Persistence.Forms;

[Index(nameof(Name), IsUnique = true)]
public class FormQuestion : IEntityDate
{
    public int Id { get; set; }
    public required Guid Guid { get; set; }
    public required FormQuestion? NextQuestion { get; set; } = null;
    public required FormQuestion? NextQuestionAlternative { get; set; } = null;
    public required int SortOrder { get; set; } = 0; // To return Question's sort order mirroring with NextQuestionID/NextQuestionAlternative
    public required FormSection Section { get; set; }
    public required FormQuestionType Type { get; set; }
    public required bool IsRequired { get; set; } = true;
    public required string Name { get; set; }
    public required string Title { get; set; }
    public required string? Description { get; set; } = null;
    public required string? Caption { get; set; } = null;
    public required FormQuestionOptions Options { get; set; } = new FormQuestionOptions();
    public DateTimeOffset CreatedOn { get; set; }
    public DateTimeOffset UpdatedOn { get; set; }
    public string? SummaryTitle { get; set; }
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
    Url
}

public enum InputWidthType
{
    Full,
    TwoThirds,
    OneHalf,
    OneThird,
    Width20,
    Width10,
    Width5,
    Width4,
    Width3,
    Width2
}

public enum DateValidationType
{
    None,
    PastOnly,
    FutureOnly,
    MinDate,
    MaxDate,
    DateRange
}

public enum TextValidationType
{
    None,
    Year
}

public record FormQuestionOptions
{
    public ICollection<FormQuestionChoice>? Choices { get; set; } = null;
    public string? ChoiceProviderStrategy { get; set; }
    public string? AnswerFieldName { get; set; }
    public ICollection<FormQuestionGroup>? Groups { get; set; }
    public FormQuestionGrouping? Grouping { get; set; } = null;
    public LayoutOptions? Layout { get; set; }
    public ValidationOptions? Validation { get; set; }
}

public record LayoutOptions
{
    public string? CustomYesText { get; set; }
    public string? CustomNoText { get; set; }
    public InputWidthType? InputWidth { get; set; }
    public string? InputSuffix { get; set; }
    public string? CustomCssClasses { get; set; }
    public string? PreHeadingContent { get; set; }
    public string? PostSubmitContent { get; set; }
    public string? PrimaryButtonText { get; set; }
}

public record ValidationOptions
{
    public DateValidationType? DateValidationType { get; set; }
    public DateTimeOffset? MinDate { get; set; }
    public DateTimeOffset? MaxDate { get; set; }
    public TextValidationType? TextValidationType { get; set; }
}

public class FormQuestionChoice
{
    public required Guid Id { get; set; }
    public required string Title { get; set; }
    public required string? GroupName { get; set; }
    public required FormQuestionChoiceHint? Hint { get; set; }
}

public class FormQuestionChoiceHint
{
    public required string? Title { get; set; } = null;
    public required string Description { get; set; }
}

public class FormQuestionGroup
{
    public required string Name { get; set; }
    public required string Hint { get; set; }
    public required string Caption { get; set; }
    public ICollection<FormQuestionGroupChoice>? Choices { get; set; }
}

public class FormQuestionGroupChoice
{
    public required Guid Id { get; set; }
    public required string Title { get; set; }
    public required string Value { get; set; }
}


public class FormQuestionGrouping
{
    public Guid Id { get; set; }
    public string? SummaryTitle { get; set; }
    public bool Page { get; set; }
    public bool CheckYourAnswers { get; set; }
}