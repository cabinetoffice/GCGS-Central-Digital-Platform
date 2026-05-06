using CO.CDP.ApplicationRegistry.Persistence.Entities;

namespace CO.CDP.ApplicationRegistry.Persistence.Repositories;

public interface IOrganisationRepository
{
    Task<Organisation?> GetByIdAsync(Guid id);
    Task<Organisation?> GetBySlugAsync(string slug);
    Task<IEnumerable<Organisation>> GetAllAsync(string? name = null, string? type = null);
    Task<Organisation> CreateAsync(Organisation organisation);
    Task UpdateAsync(Organisation organisation);
    Task<IEnumerable<UserOrganisationMembership>> GetMembersAsync(Guid organisationId);
    Task<UserOrganisationMembership?> GetMemberAsync(Guid organisationId, string userPrincipalId);
    Task AddMemberAsync(UserOrganisationMembership membership);
    Task UpdateMemberAsync(UserOrganisationMembership membership);
    Task<IEnumerable<OrganisationApplication>> GetOrganisationApplicationsAsync(Guid organisationId);
    Task EnableApplicationAsync(OrganisationApplication organisationApplication);
    Task DisableApplicationAsync(Guid organisationId, Guid applicationId);
}
