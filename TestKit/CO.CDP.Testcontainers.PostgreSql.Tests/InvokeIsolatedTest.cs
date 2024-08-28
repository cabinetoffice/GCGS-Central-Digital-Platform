using Microsoft.EntityFrameworkCore;

namespace CO.CDP.Testcontainers.PostgreSql.Tests;

public class InvokeIsolatedTest(PostgreSqlFixture postgreSql) : IClassFixture<PostgreSqlFixture>
{
    [Fact]
    public async Task ItRunsTheBlockOfCodeInATransaction()
    {
        var dbContext = CreateDbContext();

        await dbContext.InvokeIsolated(async () =>
        {
            await dbContext.Database.ExecuteSqlRawAsync("""CREATE TABLE foo(ID INTEGER)""");
        });

        var result = dbContext.Database.SqlQueryRaw<bool>(
            """SELECT EXISTS (SELECT * FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'foo')"""
        ).ToList();

        Assert.False(result.First());
    }

    private DbContext CreateDbContext()
    {
        return new DbContext(
            new DbContextOptionsBuilder<DbContext>()
                .UseNpgsql(postgreSql.ConnectionString)
                .Options
        );
    }
}