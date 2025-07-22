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

public record FormQuestionOptions
{
    public List<FormQuestionChoice>? Choices { get; init; } = new();
    public string? ChoiceProviderStrategy { get; init; }
    public string? AnswerFieldName { get; init; }
    public List<FormQuestionGroup>? Groups { get; set; } = new();
    public FormQuestionGrouping? Grouping { get; set; }
}

public class FormQuestionChoice
{
    public required Guid Id { get; set; }
    public required string Title { get; set; }
    public required string? GroupName { get; set; } = null;
    public required FormQuestionChoiceHint? Hint { get; set; } = null;
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
    public PageGrouping? Page { get; set; }
    public CheckYourAnswersGrouping? CheckYourAnswers { get; set; }
}

public class PageGrouping
{
    public int NextQuestionsToDisplay { get; set; }
    public string? PageTitleResourceKey { get; set; }
    public string? SubmitButtonTextResourceKey { get; set; }
}

public class CheckYourAnswersGrouping
{
    public string? GroupTitleResourceKey { get; set; }
}
