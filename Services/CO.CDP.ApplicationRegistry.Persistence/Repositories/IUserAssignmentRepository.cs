using CO.CDP.ApplicationRegistry.Persistence.Entities;

namespace CO.CDP.ApplicationRegistry.Persistence.Repositories;

public interface IUserAssignmentRepository
{
    Task<IEnumerable<UserApplicationAssignment>> GetAssignmentsAsync(Guid organisationId, Guid applicationId);
    Task<UserApplicationAssignment?> GetAssignmentAsync(Guid organisationId, Guid applicationId, string userPrincipalId);
    Task<UserApplicationAssignment> CreateAssignmentAsync(UserApplicationAssignment assignment);
    Task UpdateAssignmentAsync(UserApplicationAssignment assignment);
    Task RevokeAssignmentAsync(Guid organisationId, Guid applicationId, string userPrincipalId);
}
