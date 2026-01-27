using CO.CDP.ApplicationRegistry.Core.Interfaces;
using CO.CDP.ApplicationRegistry.Infrastructure.Data;

namespace CO.CDP.ApplicationRegistry.Infrastructure.Repositories;

/// <summary>
/// Implementation of the unit of work pattern.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationRegistryDbContext _context;

    public UnitOfWork(ApplicationRegistryDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
