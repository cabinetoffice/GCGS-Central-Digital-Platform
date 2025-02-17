using CO.CDP.Testcontainers.PostgreSql;
using Microsoft.EntityFrameworkCore;

namespace CO.CDP.EntityVerification.Persistence.Tests;

public static class PostgreSqlFixtureExtensions
{
    public static EntityVerificationContext EntityVerificationContext(this PostgreSqlFixture postgreSql)
    {
        var context = new EntityVerificationContext(postgreSql.DbContextOptions<EntityVerificationContext>());
        context.Database.Migrate();
        context.SaveChanges();
        return context;
    }

    private static DbContextOptions<TC> DbContextOptions<TC>(this PostgreSqlFixture postgreSql) where TC : DbContext =>
        new DbContextOptionsBuilder<TC>()
            .UseNpgsql(postgreSql.ConnectionString)
            .Options;
}