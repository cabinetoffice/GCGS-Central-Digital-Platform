using CO.CDP.Configuration.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace CO.CDP.OrganisationInformation.Persistence;

public class MigrationsDbContextFactory : IDesignTimeDbContextFactory<OrganisationInformationContext>
{
    public OrganisationInformationContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("OrganisationInformationDatabaseMigrationConfig/appsettings.json", optional: false)
                .AddEnvironmentVariables()
                .Build();

        var connectionString = ConnectionStringHelper.GetConnectionString(configuration, "OrganisationInformationDatabase");

        var npgsqlDataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString)
                        .MapEnums()
                        .Build();

        var optionsBuilder = new DbContextOptionsBuilder<OrganisationInformationContext>();
        optionsBuilder.UseNpgsql(npgsqlDataSourceBuilder,
                                 npgsqlOptions => npgsqlOptions.CommandTimeout(configuration.GetValue<int>("OrganisationInformationDatabase:CommandTimeout")));

        return new OrganisationInformationContext(optionsBuilder.Options);
    }
}