using Microsoft.EntityFrameworkCore;

namespace CO.CDP.Testcontainers.PostgreSql;

public static class DbContextExtensions
{
    /**
    * Invokes the given block of code in a transaction and rolls it back at the end.
    * This way each block is run in isolation despite sharing the database.
    */
    public static async Task InvokeIsolated(this DbContext dbContext, Func<Task> block)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        try
        {
            await block();
        }
        finally
        {
            await transaction.RollbackAsync();
        }
    }
}