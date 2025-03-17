using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.Testcontainers.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace CO.CDP.DataSharingForms.Integration.Tests;

public class OrganisationInformationPostgreSqlFixture : PostgreSqlFixture, IAsyncLifetime
{
    private DbContextOptions<OrganisationInformationContext>? _contextOptions;
    private NpgsqlDataSource? _npgsqlDataSource;

    public NpgsqlDataSource DataSource =>
      _npgsqlDataSource ??= new NpgsqlDataSourceBuilder(ConnectionString).MapEnums().Build();

    public DbContextOptions<OrganisationInformationContext> ContextOptions =>
     _contextOptions ??= new DbContextOptionsBuilder<OrganisationInformationContext>()
                    .UseNpgsql(DataSource)
                    .Options;

    public OrganisationInformationContext OrganisationInformationContext()
    {
        var context = new OrganisationInformationContext(ContextOptions);
        context.Database.Migrate();
        context.SaveChanges();
        return context;
    }
}