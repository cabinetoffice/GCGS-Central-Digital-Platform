using Microsoft.EntityFrameworkCore;

namespace CO.CDP.OrganisationInformation.Persistence.Tests;

public static class PostgreSqlFixtureExtensions
{
    public static OrganisationInformationContext OrganisationInformationContext(this OrganisationInformationPostgreSqlFixture postgreSql)
    {
        var context = new OrganisationInformationContext(postgreSql.DbContextOptions<OrganisationInformationContext>());
        context.Database.Migrate();
        context.SaveChanges();
        return context;
    }

    private static DbContextOptions<TC> DbContextOptions<TC>(this OrganisationInformationPostgreSqlFixture postgreSql) where TC : DbContext =>
       new DbContextOptionsBuilder<TC>()
                .UseNpgsql(postgreSql.DataSource)
                .Options;
}