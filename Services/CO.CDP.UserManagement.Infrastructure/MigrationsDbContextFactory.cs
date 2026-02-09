using CO.CDP.Configuration.Helpers;
using CO.CDP.UserManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace CO.CDP.UserManagement.Infrastructure;

public class MigrationsDbContextFactory : IDesignTimeDbContextFactory<UserManagementDbContext>
{
    private const string ConfigFileRelativePath = "UserManagementDatabaseMigrationConfig/appsettings.json";
    private const string ConfigFileRepoPath = "Services/CO.CDP.UserManagement.Infrastructure/UserManagementDatabaseMigrationConfig/appsettings.json";

    public UserManagementDbContext CreateDbContext(string[] args)
    {
        var configPath = ResolveConfigPath();
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.GetDirectoryName(configPath)!)
            .AddJsonFile(Path.GetFileName(configPath), optional: false)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = ConnectionStringHelper.GetConnectionString(configuration, "UserManagementDatabase");

        var optionsBuilder = new DbContextOptionsBuilder<UserManagementDbContext>();
        optionsBuilder.UseNpgsql(connectionString,
            npgsqlOptions => npgsqlOptions.CommandTimeout(configuration.GetValue<int>("UserManagementDatabase:CommandTimeout")));

        return new UserManagementDbContext(optionsBuilder.Options);
    }

    private static string ResolveConfigPath()
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        var directory = new DirectoryInfo(currentDirectory);

        while (directory != null)
        {
            var directPath = Path.Combine(directory.FullName, ConfigFileRelativePath);
            if (File.Exists(directPath))
            {
                return directPath;
            }

            var repoPath = Path.Combine(directory.FullName, ConfigFileRepoPath);
            if (File.Exists(repoPath))
            {
                return repoPath;
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException(
            $"The configuration file '{ConfigFileRelativePath}' was not found starting from '{currentDirectory}'.");
    }
}
