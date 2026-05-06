using System.ComponentModel.DataAnnotations;
using CO.CDP.EntityFrameworkCore.Timestamps;
using Microsoft.EntityFrameworkCore;

namespace CO.CDP.OrganisationInformation.Persistence;

[Index(nameof(PersonId), nameof(OrganisationApplicationId), IsUnique = true)]
public class UserApplicationAssignment : IEntityDate
{
    public int Id { get; set; }

    public int PersonId { get; set; }
    public Person? Person { get; set; }

    public int OrganisationApplicationId { get; set; }
    public OrganisationApplication? OrganisationApplication { get; set; }

    public bool IsActive { get; set; } = true;

    public List<ApplicationRole> Roles { get; set; } = [];

    [MaxLength(256)]
    public string? AssignedBy { get; set; }

    public DateTimeOffset AssignedAt { get; set; }

    [MaxLength(256)]
    public string? RevokedBy { get; set; }

    public DateTimeOffset? RevokedAt { get; set; }

    public DateTimeOffset CreatedOn { get; set; }
    public DateTimeOffset UpdatedOn { get; set; }
}
