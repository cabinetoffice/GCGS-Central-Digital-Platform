namespace CO.CDP.ApplicationRegistry.Persistence.Entities;

public class UserOrganisationMembership
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string UserPrincipalId { get; set; }
    public Guid OrganisationId { get; set; }
    public Organisation Organisation { get; set; } = null!;
    public required string OrganisationRole { get; set; }
    public DateTimeOffset JoinedAt { get; set; } = DateTimeOffset.UtcNow;
    public bool IsActive { get; set; } = true;
}
