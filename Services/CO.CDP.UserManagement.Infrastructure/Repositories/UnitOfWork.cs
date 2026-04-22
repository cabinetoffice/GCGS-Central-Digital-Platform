using CO.CDP.EntityFrameworkCore.DbContext;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Infrastructure.Data;

namespace CO.CDP.UserManagement.Infrastructure.Repositories;

/// <summary>
/// Implementation of the unit of work pattern.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly UserManagementDbContext _context;

    public UnitOfWork(UserManagementDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> action,
        CancellationToken cancellationToken = default)
    {
        await _context.InTransaction(async _ => await action(cancellationToken), cancellationToken);
    }

    public async Task<T> ExecuteInTransactionAsync<T>(
        Func<CancellationToken, Task<T>> action,
        CancellationToken cancellationToken = default)
    {
        T result = default!;
        await _context.InTransaction(async _ => { result = await action(cancellationToken); }, cancellationToken);
        return result;
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}