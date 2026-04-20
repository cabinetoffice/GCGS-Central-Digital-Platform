using Microsoft.Extensions.Configuration;

namespace E2ETests.Utilities;

public static class ConfigUtility
{
    private static IConfigurationRoot? _config;
    private static TestSettings? _settings;

    static ConfigUtility()
    {
        _config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        _settings = _config.GetSection("TestSettings").Get<TestSettings>();
    }

    public static string GetBaseUrl()
    {
        return _settings?.BaseUrl ?? throw new Exception("BaseUrl is not configured.");
    }

    public static string GetOrganisationApiBaseUrl()
    {
        return _settings?.ApiUrl ?? throw new Exception("ApiUrl is not configured.");
    }

    public static string GetTestEmail()
    {
        return _settings?.Email ?? throw new Exception("Email is not configured.");
    }

    public static string GetTestPassword()
    {
        return _settings?.Password ?? throw new Exception("Password is not configured.");
    }

    public static string GetSecretKey()
    {
        return _settings?.SecretKey ?? throw new Exception("SecretKey is not configured.");
    }

    public static bool IsHeadless()
    {
        return _settings?.Headless ?? true;
    }

    public static string DatabaseConnectionString()
    {
        return _settings?.DatabaseConnectionString ?? throw new Exception("DatabaseConnectionString is not configured.");
    }

    public static string GetTestSupportAdminEmail()
    {
        return _settings?.TestSupportAdminEmail ?? throw new Exception("TestSupportAdminEmail is not configured.");
    }

    public static string GetTestSupportAdminPassword()
    {
        return _settings?.TestSupportAdminPassword ?? throw new Exception("TestSupportAdminPassword is not configured.");
    }

    public static string GetTestSupportAdminSecretKey()
    {
        return _settings?.TestSupportAdminSecretKey ?? throw new Exception("TestSupportAdminSecretKey is not configured.");
    }
}
