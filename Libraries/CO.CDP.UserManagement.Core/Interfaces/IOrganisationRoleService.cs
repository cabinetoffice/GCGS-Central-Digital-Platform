using CO.CDP.UserManagement.Core.Entities;

namespace CO.CDP.UserManagement.Core.Interfaces;

public interface IOrganisationRoleService
{
    Task<IReadOnlyList<OrganisationRoleEntity>> GetActiveAsync(CancellationToken cancellationToken = default);
}
