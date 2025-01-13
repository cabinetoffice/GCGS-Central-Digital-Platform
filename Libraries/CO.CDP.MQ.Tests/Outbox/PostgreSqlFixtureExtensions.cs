using CO.CDP.MQ.Outbox;
using CO.CDP.Testcontainers.PostgreSql;
using Microsoft.EntityFrameworkCore;

namespace CO.CDP.MQ.Tests.Outbox;

internal static class PostgreSqlFixtureExtensions
{
    public static TestDbContext TestDbContext(this PostgreSqlFixture postgreSql)
    {
        var context = new TestDbContext(postgreSql.DbContextOptions<TestDbContext>());
        context.Database.EnsureCreated();
        context.SaveChanges();
        context.Database.ExecuteSqlRaw(Migrations.NotificationFunction());
        context.Database.ExecuteSqlRaw(Migrations.NotificationTrigger(table: "OutboxMessages"));
        return context;
    }

    private static DbContextOptions<TC> DbContextOptions<TC>(this PostgreSqlFixture postgreSql) where TC : DbContext =>
        new DbContextOptionsBuilder<TC>()
            .UseNpgsql(postgreSql.ConnectionString)
            .Options;
}