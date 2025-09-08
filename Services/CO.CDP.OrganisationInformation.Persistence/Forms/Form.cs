using CO.CDP.EntityFrameworkCore.Timestamps;
using System.ComponentModel.DataAnnotations.Schema;

namespace CO.CDP.OrganisationInformation.Persistence.Forms;

public class Form : IEntityDate
{
    public int Id { get; set; }

    public required Guid Guid { get; set; }

    public required string Name { get; set; }

    public required string Version { get; set; }

    public required bool IsRequired { get; set; } = true;

    public required FormScope Scope { get; set; }

    public required ICollection<FormSection> Sections { get; set; } = [];
    public DateTimeOffset CreatedOn { get; set; }
    public DateTimeOffset UpdatedOn { get; set; }
}

public enum FormSectionType
{
    Standard,
    Declaration,
    Exclusions,
    AdditionalSection,
    WelshAdditionalSection
}

public enum FormScope
{
    SupplierInformation,
    LegalCompliance
}

public class FormSection : IEntityDate
{
    public int Id { get; set; }
    public required Guid Guid { get; set; }
    public required string Title { get; set; }
    public required FormSectionType Type { get; set; }

    [ForeignKey(nameof(Form))]
    public required int FormId { get; set; }
    public required Form Form { get; set; }
    public required ICollection<FormQuestion> Questions { get; set; } = [];
    public required bool AllowsMultipleAnswerSets { get; set; }
    public required bool CheckFurtherQuestionsExempted { get; set; }
    public required int DisplayOrder { get; set; }
    public required FormSectionConfiguration Configuration;
    public DateTimeOffset CreatedOn { get; set; }
    public DateTimeOffset UpdatedOn { get; set; }
}

public record FormSectionConfiguration
{
    public string? SingularSummaryHeadingHint { get; set; }
    public string? SingularSummaryHeading { get; set; }
    public string? PluralSummaryHeadingFormat { get; set; }
    public string? PluralSummaryHeadingHintFormat { get; set; }
    public string? AddAnotherAnswerLabel { get; set; }
    public string? RemoveConfirmationCaption { get; set; }
    public string? RemoveConfirmationHeading { get; set; }
    public string? FurtherQuestionsExemptedHeading { get; set; }
    public string? FurtherQuestionsExemptedHint { get; set; }
    public SummaryRenderFormatter? SummaryRenderFormatter { get; set; }
}
public record SummaryRenderFormatter
{
    public required string KeyExpression { get; set; }
    public List<string> KeyParams { get; set; } = [];
    public required string KeyExpressionOperation { get; set; }
    public required string ValueExpression { get; set; }
    public List<string> ValueParams { get; set; } = [];
    public required string ValueExpressionOperation { get; set; }
}