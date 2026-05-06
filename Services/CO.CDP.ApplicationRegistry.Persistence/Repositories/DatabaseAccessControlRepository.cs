using CO.CDP.ApplicationRegistry.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace CO.CDP.ApplicationRegistry.Persistence.Repositories;

public class DatabaseAccessControlRepository : IAccessControlRepository
{
    private readonly ApplicationRegistryContext _context;

    public DatabaseAccessControlRepository(ApplicationRegistryContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<AccessControlEntry>> GetAclEntriesAsync(Guid reportId)
    {
        return await _context.AccessControlEntries
            .Where(e => e.ReportId == reportId && e.RevokedAt == null)
            .OrderByDescending(e => e.GrantedAt)
            .ToListAsync();
    }

    public async Task<AccessControlEntry> GrantAccessAsync(AccessControlEntry entry)
    {
        _context.AccessControlEntries.Add(entry);
        await _context.SaveChangesAsync();
        return entry;
    }

    public async Task RevokeAccessAsync(Guid entryId)
    {
        var entry = await _context.AccessControlEntries.FindAsync(entryId);
        if (entry != null)
        {
            entry.RevokedAt = DateTimeOffset.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}
