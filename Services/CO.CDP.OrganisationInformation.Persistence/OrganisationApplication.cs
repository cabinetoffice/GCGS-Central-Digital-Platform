using System.ComponentModel.DataAnnotations;
using CO.CDP.EntityFrameworkCore.Timestamps;
using Microsoft.EntityFrameworkCore;

namespace CO.CDP.OrganisationInformation.Persistence;

[Index(nameof(OrganisationId), nameof(ApplicationId), IsUnique = true)]
public class OrganisationApplication : IEntityDate
{
    public int Id { get; set; }

    public int OrganisationId { get; set; }
    public Organisation? Organisation { get; set; }

    public int ApplicationId { get; set; }
    public Application? Application { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTimeOffset EnabledAt { get; set; }

    [MaxLength(256)]
    public string? EnabledBy { get; set; }

    public DateTimeOffset CreatedOn { get; set; }
    public DateTimeOffset UpdatedOn { get; set; }
}
