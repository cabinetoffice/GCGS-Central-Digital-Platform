using CO.CDP.Configuration.Helpers;
using CO.CDP.EntityVerification.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace CO.CDP.OrganisationInformation.Persistence;

public class MigrationsDbContextFactory : IDesignTimeDbContextFactory<EntityVerificationContext>
{
    public EntityVerificationContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("EntityVerificationDatabaseMigrationConfig/appsettings.json", optional: false)
                .AddEnvironmentVariables()
                .Build();

        var connectionString = ConnectionStringHelper.GetConnectionString(configuration, "EntityVerificationDatabase");

        var optionsBuilder = new DbContextOptionsBuilder<EntityVerificationContext>();
        optionsBuilder.UseNpgsql(connectionString,
                                 npgsqlOptions => npgsqlOptions.CommandTimeout(configuration.GetValue<int>("EntityVerificationDatabase:CommandTimeout")));

        return new EntityVerificationContext(optionsBuilder.Options);
    }
}
