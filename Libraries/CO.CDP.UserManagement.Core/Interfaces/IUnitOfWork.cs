namespace CO.CDP.UserManagement.Core.Interfaces;

/// <summary>
/// Defines the unit of work pattern for managing database transactions.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Saves all changes made in this unit of work to the database.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes the given action within a single DB transaction, committing atomically on success
    /// or rolling back on failure. If already inside an outer transaction (e.g. AtomicScope),
    /// the action runs within that transaction without creating a nested one.
    /// </summary>
    Task ExecuteInTransactionAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken = default);

    /// <summary>
    /// Same as <see cref="ExecuteInTransactionAsync"/> but returns a value.
    /// </summary>
    Task<T> ExecuteInTransactionAsync<T>(Func<CancellationToken, Task<T>> action,
        CancellationToken cancellationToken = default);
}