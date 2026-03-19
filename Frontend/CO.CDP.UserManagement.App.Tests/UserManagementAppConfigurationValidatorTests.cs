using CO.CDP.UserManagement.App;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace CO.CDP.UserManagement.App.Tests;

public class UserManagementAppConfigurationValidatorTests
{
    [Fact]
    public void Validate_WhenAwsDevelopmentAndAwsRegionMissing_ThrowsExplicitMessage()
    {
        var configuration = new ConfigurationBuilder().Build();
        var environment = new FakeHostEnvironment("AwsDevelopment");

        Action act = () => UserManagementAppConfigurationValidator.Validate(configuration, environment, _ => null);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*AWS:Region (env: AWS_REGION)*");
    }

    [Fact]
    public void Validate_WhenAwsDevelopmentAndRequiredEnvironmentVariableMissing_ThrowsExplicitMessage()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(
        [
            new KeyValuePair<string, string?>("AWS:Region", "eu-west-2")
        ]).Build();
        var environment = new FakeHostEnvironment("AwsDevelopment");

        var environmentVariables = new Dictionary<string, string?>
        {
            ["Aws__ElastiCache__Hostname"] = "cache.example",
            ["Aws__ElastiCache__Port"] = "6379",
            ["Organisation__Authority"] = "https://authority.example",
            ["OneLogin__Authority"] = "https://oidc.example",
            ["OneLogin__ClientId"] = "client-id",
            ["OneLogin__PrivateKey"] = "private-key",
            ["AWS__SystemManager__DataProtectionPrefix"] = "/cdp/test"
        };

        Action act = () => UserManagementAppConfigurationValidator.Validate(
            configuration,
            environment,
            key => environmentVariables.GetValueOrDefault(key));

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*UserManagementApi:BaseUrl (env: UserManagementApi__BaseUrl)*");
    }

    [Fact]
    public void Validate_WhenNotAwsDevelopment_DoesNotRequireAwsSpecificEnvironmentVariables()
    {
        var configuration = new ConfigurationBuilder().Build();
        var environment = new FakeHostEnvironment("Development");

        Action act = () => UserManagementAppConfigurationValidator.Validate(configuration, environment, _ => null);

        act.Should().NotThrow();
    }

    private sealed class FakeHostEnvironment(string environmentName) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = environmentName;
        public string ApplicationName { get; set; } = "Test";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
