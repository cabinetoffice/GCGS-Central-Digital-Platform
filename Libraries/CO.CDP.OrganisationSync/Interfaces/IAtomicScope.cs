namespace CO.CDP.OrganisationSync;

/// <summary>
/// Coordinates OI and UM writes within a single PostgreSQL transaction.
/// Both DbContexts must share the same <c>NpgsqlConnection</c> (registered as scoped in DI).
/// </summary>
public interface IAtomicScope
{
    /// <summary>
    /// Executes <paramref name="action"/> within a shared transaction spanning both OI and UM contexts.
    /// Flushes tracked changes from both contexts then commits atomically.
    /// Rolls back both on any failure.
    /// </summary>
    Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> action, CancellationToken ct = default);
}
