using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace CO.CDP.UserManagement.Api;

public static class UserManagementApiConfigurationValidator
{
    private static readonly (string EnvironmentVariable, string ConfigurationKey)[] RequiredAwsDevelopmentEnvironmentVariables =
    [
        ("Aws__ElastiCache__Hostname", "Aws:ElastiCache:Hostname"),
        ("Aws__ElastiCache__Port", "Aws:ElastiCache:Port"),
        ("Organisation__Authority", "Organisation:Authority"),
        ("PersonService", "PersonService"),
        ("OrganisationService", "OrganisationService"),
        ("ServiceKey__ApiKey", "ServiceKey:ApiKey"),
        ("UserManagementDatabase__Server", "UserManagementDatabase:Server"),
        ("UserManagementDatabase__Database", "UserManagementDatabase:Database"),
        ("UserManagementDatabase__Username", "UserManagementDatabase:Username"),
        ("UserManagementDatabase__Password", "UserManagementDatabase:Password"),
        ("OrganisationInformationDatabase__Server", "OrganisationInformationDatabase:Server"),
        ("OrganisationInformationDatabase__Database", "OrganisationInformationDatabase:Database"),
        ("OrganisationInformationDatabase__Username", "OrganisationInformationDatabase:Username"),
        ("OrganisationInformationDatabase__Password", "OrganisationInformationDatabase:Password")
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
