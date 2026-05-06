using CO.CDP.ApplicationRegistry.Persistence.Entities;

namespace CO.CDP.ApplicationRegistry.Persistence.Repositories;

public interface IAccessControlRepository
{
    Task<IEnumerable<AccessControlEntry>> GetAclEntriesAsync(Guid reportId);
    Task<AccessControlEntry> GrantAccessAsync(AccessControlEntry entry);
    Task RevokeAccessAsync(Guid entryId);
}
