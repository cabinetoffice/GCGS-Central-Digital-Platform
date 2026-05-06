using System.ComponentModel.DataAnnotations;
using CO.CDP.EntityFrameworkCore.Timestamps;
using Microsoft.EntityFrameworkCore;

namespace CO.CDP.OrganisationInformation.Persistence;

[Index(nameof(Guid), IsUnique = true)]
[Index(nameof(ApplicationId), nameof(Name), IsUnique = true)]
public class ApplicationRole : IEntityDate
{
    public int Id { get; set; }
    public required Guid Guid { get; set; }

    [MaxLength(100)]
    public required string Name { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public int ApplicationId { get; set; }
    public Application? Application { get; set; }

    public bool IsActive { get; set; } = true;

    public List<ApplicationPermission> Permissions { get; set; } = [];

    public DateTimeOffset CreatedOn { get; set; }
    public DateTimeOffset UpdatedOn { get; set; }
}
