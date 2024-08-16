using Microsoft.Extensions.Configuration;

namespace CO.CDP.Configuration.Helpers;

public static class ConnectionStringHelper
{
    public static string GetConnectionString(IConfiguration configuration, string name)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        if (string.IsNullOrEmpty(name)) throw new ArgumentException("Connection string name cannot be null or empty.", nameof(name));

        var connectionSettings = configuration.GetSection(name);
        if (connectionSettings == null)
        {
            throw new InvalidOperationException($"Connection string section '{name}' is missing.");
        }

        var requiredKeys = new[] { "Server", "Database", "Username", "Password" };
        var missingKeys = requiredKeys.Where(key => string.IsNullOrEmpty(connectionSettings[key])).ToList();

        if (missingKeys.Any())
        {
            throw new InvalidOperationException($"Missing required connection string parameters: {string.Join(", ", missingKeys)}.");
        }

        var server = connectionSettings["Server"];
        var database = connectionSettings["Database"];
        var username = connectionSettings["Username"];
        var password = connectionSettings["Password"];

        return $"Server={server};Database={database};Username={username};Password={password};";
    }
}
