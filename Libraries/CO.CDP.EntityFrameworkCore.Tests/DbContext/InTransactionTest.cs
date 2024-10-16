using CO.CDP.EntityFrameworkCore.DbContext;
using CO.CDP.Testcontainers.PostgreSql;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace CO.CDP.EntityFrameworkCore.Tests.DbContext;

public class InTransactionTest(PostgreSqlFixture postgreSql) : IClassFixture<PostgreSqlFixture>
{
    [Fact]
    public async Task ItExecutesTheActionInTransaction()
    {
        var context = DbContext();

        await context.InTransaction(async c =>
        {
            c.Update(new User { Name = "Bob" });
            c.Update(new User { Name = "Alice" });
            await c.SaveChangesAsync();
        });

        context.Users.FirstOrDefault(s => s.Name == "Bob").Should().NotBeNull();
        context.Users.FirstOrDefault(s => s.Name == "Alice").Should().NotBeNull();
    }

    [Fact]
    public async Task ItRollsBackTheTransactionIfActionFails()
    {
        var context = DbContext();

        var act = async () => await context.InTransaction(async c =>
        {
            c.Update(new User { Name = "Sussan" });
            await c.SaveChangesAsync();
            throw new Exception("Failed in transaction");
        });

        await act.Should().ThrowAsync<Exception>("Failed in transaction");
        context.Users.FirstOrDefault(s => s.Name == "Sussan").Should().BeNull();
    }

    private TestDbContext DbContext()
    {
        var context = new TestDbContext(new DbContextOptionsBuilder<TestDbContext>()
            .UseNpgsql(postgreSql.ConnectionString)
            .Options);
        context.Database.EnsureCreated();
        context.SaveChanges();
        return context;
    }
}

internal class TestDbContext(DbContextOptions options) : Microsoft.EntityFrameworkCore.DbContext(options)
{
    public DbSet<User> Users { get; set; }
}

internal class User
{
    public int Id { get; init; }
    public required string Name { get; init; }
}