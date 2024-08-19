using Microsoft.Extensions.Configuration;
using CO.CDP.Configuration.Helpers;
using Xunit;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace CO.CDP.Configuration.Tests.Helpers;
public class ConnectionStringHelperTests
{
    [Fact]
    public void GetConnectionString_ReturnsCorrectConnectionString_WhenAllValuesAreProvidedx()
    {
        var configuration = GivenConfiguration(new()
        {
            {"OrganisationInformationDatabase", new()
            {
                {"Server", "localhost"},
                {"Database", "cdp"},
                {"Username", "cdp_user"},
                {"Password", "password"},
            }}
        });

        var connectionString = ConnectionStringHelper.GetConnectionString(configuration, "OrganisationInformationDatabase");

        Assert.Equal("Server=localhost;Database=cdp;Username=cdp_user;Password=password;", connectionString);
    }

    [Fact]
    public void GetConnectionString_ThrowsArgumentNullException_WhenConfigurationIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            ConnectionStringHelper.GetConnectionString(null!, "OrganisationInformationDatabase"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void GetConnectionString_ThrowsArgumentException_WhenNameIsNullOrEmpty(string? name)
    {
        var configuration = GivenConfiguration(new());
        Assert.ThrowsAny<ArgumentException>(() =>
            ConnectionStringHelper.GetConnectionString(configuration, name!));
    }

    [Fact]
    public void GetConnectionString_ThrowsInvalidOperationException_WhenSectionIsMissing()
    {
        var configuration = GivenConfiguration(new());
        var ex = Assert.Throws<InvalidOperationException>(() =>
            ConnectionStringHelper.GetConnectionString(configuration, "NonExistentSection"));
        Assert.Equal("Connection string section 'NonExistentSection' is missing.", ex.Message);
    }

    [Theory]
    [InlineData("Server")]
    [InlineData("Database")]
    [InlineData("Username")]
    [InlineData("Password")]
    public void GetConnectionString_ThrowsInvalidOperationException_WhenRequiredKeyIsMissing(string missingKey)
    {
        var configData = new Dictionary<string, string>
            {
                {"Server", "localhost"},
                {"Database", "testdb"},
                {"Username", "testuser"},
                {"Password", "testpass"}
            };
        configData.Remove(missingKey);

        var configuration = GivenConfiguration(new() { { "TestDatabase", configData } });

        var ex = Assert.Throws<InvalidOperationException>(() =>
            ConnectionStringHelper.GetConnectionString(configuration, "TestDatabase"));
        Assert.Equal($"Missing required connection string parameters: {missingKey}.", ex.Message);
    }


    private static IConfiguration GivenConfiguration(Dictionary<string, Dictionary<string, string>> configuration)
    {
        return new ConfigurationBuilder()
            .AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(configuration))))
            .Build();
    }

}