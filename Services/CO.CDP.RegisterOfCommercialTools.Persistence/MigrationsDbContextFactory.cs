using CO.CDP.Configuration.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace CO.CDP.RegisterOfCommercialTools.Persistence;

public class MigrationsDbContextFactory : IDesignTimeDbContextFactory<RegisterOfCommercialToolsContext>
{
    public RegisterOfCommercialToolsContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("RegisterOfCommercialToolsDatabaseMigrationConfig/appsettings.json", optional: false)
                .AddEnvironmentVariables()
                .Build();

        var connectionString = ConnectionStringHelper.GetConnectionString(configuration, "OrganisationInformationDatabase");

        var optionsBuilder = new DbContextOptionsBuilder<RegisterOfCommercialToolsContext>();
        optionsBuilder.UseNpgsql(connectionString,
                                 npgsqlOptions => npgsqlOptions.CommandTimeout(configuration.GetValue<int>("OrganisationInformationDatabase:CommandTimeout")));

        return new RegisterOfCommercialToolsContext(optionsBuilder.Options);
    }
}
