using CO.CDP.UserManagement.Core.Entities;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CO.CDP.UserManagement.Infrastructure.Services;

public class OrganisationRoleService(UserManagementDbContext context) : IOrganisationRoleService
{
    public async Task<IReadOnlyList<OrganisationRoleEntity>> GetActiveAsync(CancellationToken cancellationToken = default) =>
        await context.OrganisationRoles
            .AsNoTracking()
            .OrderBy(x => x.Id)
            .ToListAsync(cancellationToken);
}
