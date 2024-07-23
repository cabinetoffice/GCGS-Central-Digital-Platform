using CO.CDP.EntityFrameworkCore.Timestamps;
using Microsoft.EntityFrameworkCore;

namespace CO.CDP.OrganisationInformation.Persistence;

[Index(nameof(Guid), IsUnique = true)]
[Index(nameof(Email), IsUnique = true)]
public class Person : IEntityDate
{
    public int Id { get; set; }
    public required Guid Guid { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public string? Phone { get; set; }
    public string? UserUrn { get; set; }
    public List<Tenant> Tenants { get; } = [];
    public List<Organisation> Organisations { get; } = [];
    public List<OrganisationPerson> PersonOrganisations { get; init; } = [];
    public DateTimeOffset CreatedOn { get; set; }
    public DateTimeOffset UpdatedOn { get; set; }
}