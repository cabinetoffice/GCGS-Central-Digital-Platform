namespace CO.CDP.ApplicationRegistry.Persistence.Entities;

public class UserApplicationAssignment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string UserPrincipalId { get; set; }
    public Guid ApplicationId { get; set; }
    public Application Application { get; set; } = null!;
    public Guid OrganisationId { get; set; }
    public Organisation Organisation { get; set; } = null!;
    public DateTimeOffset AssignedAt { get; set; } = DateTimeOffset.UtcNow;
    public required string AssignedBy { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<UserRoleAssignment> RoleAssignments { get; set; } = new List<UserRoleAssignment>();
}
