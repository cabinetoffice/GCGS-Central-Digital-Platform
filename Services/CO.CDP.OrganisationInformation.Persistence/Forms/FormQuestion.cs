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
    Url
}

public record FormQuestionOptions
{
    public ICollection<FormQuestionChoice>? Choices { get; set; } = null;
    public string? ChoiceProviderStrategy { get; set; } = null;
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