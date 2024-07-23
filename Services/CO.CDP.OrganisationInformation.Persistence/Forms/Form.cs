using CO.CDP.EntityFrameworkCore.Timestamps;
using Microsoft.EntityFrameworkCore;

namespace CO.CDP.OrganisationInformation.Persistence.Forms;

public class Form : IEntityDate
{
    public int Id { get; set; }

    public required Guid Guid { get; set; }

    public required string Name { get; set; }

    public required string Version { get; set; }

    public required bool IsRequired { get; set; } = true;

    public required FormType Type { get; set; }

    public required FormScope Scope { get; set; }

    public required ICollection<FormSection> Sections { get; set; } = [];
    public DateTimeOffset CreatedOn { get; set; }
    public DateTimeOffset UpdatedOn { get; set; }
}

public enum FormType
{
    Standard,
    Declaration
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
    public required Form Form { get; set; }
    public required ICollection<FormQuestion> Questions { get; set; } = [];
    public required bool AllowsMultipleAnswerSets = false;
    public DateTimeOffset CreatedOn { get; set; }
    public DateTimeOffset UpdatedOn { get; set; }
}