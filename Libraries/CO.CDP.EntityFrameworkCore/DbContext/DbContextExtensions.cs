namespace CO.CDP.EntityFrameworkCore.DbContext;

public static class DbContextExtensions
{
    public static async Task InTransaction<TDbContext>(this TDbContext context, Func<TDbContext, Task> action)
        where TDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
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