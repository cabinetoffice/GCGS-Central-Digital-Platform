namespace CO.CDP.ApplicationRegistry.Persistence.Entities;

public class RolePermission
{
    public Guid RoleId { get; set; }
    public ApplicationRole Role { get; set; } = null!;
    public Guid PermissionId { get; set; }
    public ApplicationPermission Permission { get; set; } = null!;
}
