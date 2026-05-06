namespace CO.CDP.ApplicationRegistry.Persistence.Entities;

public class Organisation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Name { get; set; }
    public required string Slug { get; set; }
    public string? Type { get; set; }
    public Guid? ParentOrganisationId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedOn { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedOn { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<UserOrganisationMembership> Members { get; set; } = new List<UserOrganisationMembership>();
    public ICollection<OrganisationApplication> Applications { get; set; } = new List<OrganisationApplication>();
}
