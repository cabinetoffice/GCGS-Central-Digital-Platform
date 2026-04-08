using System.ComponentModel.DataAnnotations;
using CO.CDP.EntityFrameworkCore.Timestamps;
using Microsoft.EntityFrameworkCore;

namespace CO.CDP.OrganisationInformation.Persistence;

[Index(nameof(Guid), IsUnique = true)]
[Index(nameof(ClientId), IsUnique = true)]
public class Application : IEntityDate
{
    public int Id { get; set; }
    public required Guid Guid { get; set; }

    [MaxLength(200)]
    public required string Name { get; set; }

    [MaxLength(100)]
    public required string ClientId { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }

    [MaxLength(100)]
    public string? Category { get; set; }

    public bool IsActive { get; set; } = true;
    public bool IsEnabledByDefault { get; set; }

    [MaxLength(256)]
    public string? CreatedBy { get; set; }

    public List<ApplicationRole> Roles { get; set; } = [];
    public List<ApplicationPermission> Permissions { get; set; } = [];

    public DateTimeOffset CreatedOn { get; set; }
    public DateTimeOffset UpdatedOn { get; set; }
}
