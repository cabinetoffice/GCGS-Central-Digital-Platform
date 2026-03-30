using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace CO.CDP.UserManagement.App;

public static class UserManagementAppConfigurationValidator
{
    private static readonly (string EnvironmentVariable, string ConfigurationKey)[] RequiredAwsDevelopmentEnvironmentVariables =
    [
        ("AWS__SystemManager__DataProtectionPrefix", "Aws:SystemManager:DataProtectionPrefix"),
        ("Aws__ElastiCache__Hostname", "Aws:ElastiCache:Hostname"),
        ("Aws__ElastiCache__Port", "Aws:ElastiCache:Port"),
        ("Organisation__Authority", "Organisation:Authority"),
        ("UserManagementApi__BaseUrl", "UserManagementApi:BaseUrl"),
        ("OneLogin__Authority", "OneLogin:Authority"),
        ("OneLogin__ClientId", "OneLogin:ClientId"),
        ("OneLogin__PrivateKey", "OneLogin:PrivateKey")
    ];

    public static void Validate(
        IConfiguration configuration,
        IHostEnvironment environment,
        Func<string, string?>? getEnvironmentVariable = null)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(environment);

        if (!string.Equals(environment.EnvironmentName, "AwsDevelopment", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        getEnvironmentVariable ??= Environment.GetEnvironmentVariable;

        var missing = new List<string>();

        var awsRegion = configuration["AWS:Region"] ?? getEnvironmentVariable("AWS_REGION");
        if (string.IsNullOrWhiteSpace(awsRegion))
        {
            missing.Add("AWS:Region (env: AWS_REGION)");
        }

        foreach (var (environmentVariable, configurationKey) in RequiredAwsDevelopmentEnvironmentVariables)
        {
            if (string.IsNullOrWhiteSpace(getEnvironmentVariable(environmentVariable)))
            {
                missing.Add($"{configurationKey} (env: {environmentVariable})");
            }
        }

        if (missing.Count > 0)
        {
            throw new InvalidOperationException(
                $"Missing required AwsDevelopment configuration: {string.Join(", ", missing)}.");
        }
    }
}
