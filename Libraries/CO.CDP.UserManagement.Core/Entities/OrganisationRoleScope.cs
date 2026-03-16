namespace CO.CDP.UserManagement.Core.Entities;

public class OrganisationRoleScope : ISoftDelete, IAuditable
{
    public int Id { get; set; }
    public int OrganisationRoleId { get; set; }
    public OrganisationRoleScopeSource Source { get; set; }
    public string ScopeName { get; set; } = string.Empty;

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTimeOffset? ModifiedAt { get; set; }
    public string? ModifiedBy { get; set; }

    public OrganisationRoleEntity OrganisationRole { get; set; } = null!;
}
