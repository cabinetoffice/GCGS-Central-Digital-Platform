namespace CO.CDP.EntityFrameworkCore.DbContext;

public static class DbContextExtensions
{
    public static async Task InTransaction<TDbContext>(this TDbContext context, Func<TDbContext, Task> action)
        where TDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        // When called inside an AtomicScope (or any external transaction), defer to that transaction.
        // The caller is responsible for committing or rolling back.
        if (context.Database.CurrentTransaction is not null)
        {
            await action(context);
            return;
        }

        await using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            await action(context);
            await transaction.CommitAsync();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}