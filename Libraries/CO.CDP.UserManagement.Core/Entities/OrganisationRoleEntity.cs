using CO.CDP.UserManagement.Shared.Enums;

namespace CO.CDP.UserManagement.Core.Entities;

public class OrganisationRoleEntity : ISoftDelete, IAuditable
{
    public int Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool SyncToOrganisationInformation { get; set; }
    public bool AutoAssignDefaultApplications { get; set; }

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTimeOffset? ModifiedAt { get; set; }
    public string? ModifiedBy { get; set; }

    public List<string> OrganisationInformationScopes { get; set; } = [];

    public ICollection<UserOrganisationMembership> Memberships { get; set; } = new List<UserOrganisationMembership>();
    public ICollection<InviteRoleMapping> InviteRoleMappings { get; set; } = new List<InviteRoleMapping>();
}
