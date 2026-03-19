using CO.CDP.UserManagement.Api;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace CO.CDP.UserManagement.Api.Tests;

public class UserManagementApiConfigurationValidatorTests
{
    [Fact]
    public void Validate_WhenAwsDevelopmentAndAwsRegionMissing_ThrowsExplicitMessage()
    {
        var configuration = new ConfigurationBuilder().Build();
        var environment = new FakeHostEnvironment("AwsDevelopment");

        Action act = () => UserManagementApiConfigurationValidator.Validate(configuration, environment, _ => null);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*AWS:Region (env: AWS_REGION)*");
    }

    [Fact]
    public void Validate_WhenAwsDevelopmentAndRequiredSecretMissing_ThrowsExplicitMessage()
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
            ["PersonService"] = "https://person.example",
            ["OrganisationService"] = "https://organisation.example",
            ["UserManagementDatabase__Server"] = "db.example",
            ["UserManagementDatabase__Database"] = "cdp",
            ["UserManagementDatabase__Username"] = "user",
            ["UserManagementDatabase__Password"] = "password",
            ["OrganisationInformationDatabase__Server"] = "db.example",
            ["OrganisationInformationDatabase__Database"] = "cdp",
            ["OrganisationInformationDatabase__Username"] = "user",
            ["OrganisationInformationDatabase__Password"] = "password"
        };

        Action act = () => UserManagementApiConfigurationValidator.Validate(
            configuration,
            environment,
            key => environmentVariables.GetValueOrDefault(key));

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*ServiceKey:ApiKey (env: ServiceKey__ApiKey)*");
    }

    [Fact]
    public void Validate_WhenNotAwsDevelopment_DoesNotRequireAwsSpecificEnvironmentVariables()
    {
        var configuration = new ConfigurationBuilder().Build();
        var environment = new FakeHostEnvironment("Development");

        Action act = () => UserManagementApiConfigurationValidator.Validate(configuration, environment, _ => null);

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
