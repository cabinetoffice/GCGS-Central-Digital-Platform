namespace CO.CDP.OrganisationSync;

using System.Data;
using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.UserManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Executes OI + UM writes in a single PostgreSQL transaction by sharing the underlying
/// <c>NpgsqlConnection</c>. Both contexts are enlisted via <c>UseTransactionAsync</c>
/// before the action runs; <c>SaveChangesAsync</c> is called on each context after the
/// action completes, then the transaction is committed.
/// </summary>
public sealed class AtomicScope(
    OrganisationInformationContext oiContext,
    UserManagementDbContext umContext) : IAtomicScope
{
    public async Task<T> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> action,
        CancellationToken ct = default)
    {
        var connection = oiContext.Database.GetDbConnection();

        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync(ct);

        await using var transaction = await connection.BeginTransactionAsync(ct);
        await oiContext.Database.UseTransactionAsync(transaction, ct);
        await umContext.Database.UseTransactionAsync(transaction, ct);

        try
        {
            var result = await action(ct);
            await oiContext.SaveChangesAsync(ct);
            await umContext.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            return result;
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }
}
