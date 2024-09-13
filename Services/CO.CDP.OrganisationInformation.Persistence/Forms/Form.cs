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
    Exclusions
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
    public required FormSectionConfiguration Configuration;
    public DateTimeOffset CreatedOn { get; set; }
    public DateTimeOffset UpdatedOn { get; set; }
}

public record FormSectionConfiguration
{
    public string? SingularSummaryHeading { get; set; }
    public string? PluralSummaryHeadingFormat { get; set; }
    public string? AddAnotherAnswerLabel { get; set; }
    public string? RemoveConfirmationCaption { get; set; }
    public string? RemoveConfirmationHeading { get; set; }
}