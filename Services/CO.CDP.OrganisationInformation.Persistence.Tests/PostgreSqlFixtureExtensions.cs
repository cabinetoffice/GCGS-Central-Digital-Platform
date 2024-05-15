using CO.CDP.Testcontainers.PostgreSql;
using Microsoft.EntityFrameworkCore;

namespace CO.CDP.OrganisationInformation.Persistence.Tests;

public static class PostgreSqlFixtureExtensions
{
    public static OrganisationInformationContext OrganisationInformationContext(this PostgreSqlFixture postgreSql)
    {
        var context = new OrganisationInformationContext(postgreSql.DbContextOptions<OrganisationInformationContext>());
        context.Database.Migrate();
        context.SaveChanges();
        return context;
    }

    private static DbContextOptions<TC> DbContextOptions<TC>(this PostgreSqlFixture postgreSql) where TC : DbContext =>
        new DbContextOptionsBuilder<TC>()
            .UseNpgsql(postgreSql.ConnectionString)
            .Options;
}