namespace E2ETests.Utilities;

public class TestSettings
{
    public string BaseUrl { get; set; } = string.Empty;
    public string ApiUrl { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public bool Headless { get; set; } = true;
    public string DatabaseConnectionString { get; set; } = string.Empty;
    public string TestSupportAdminEmail { get; set; } = string.Empty;
    public string TestSupportAdminPassword { get; set; } = string.Empty;
    public string TestSupportAdminSecretKey { get; set; } = string.Empty;
}
