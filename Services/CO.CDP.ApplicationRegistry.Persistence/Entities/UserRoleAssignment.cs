namespace CO.CDP.ApplicationRegistry.Persistence.Entities;

public class UserRoleAssignment
{
    public Guid UserApplicationAssignmentId { get; set; }
    public UserApplicationAssignment UserApplicationAssignment { get; set; } = null!;
    public Guid RoleId { get; set; }
    public ApplicationRole Role { get; set; } = null!;
}
