using System.Globalization;
using Microsoft.Extensions.Configuration;
using Moq;
using FluentAssertions;

namespace CO.CDP.OrganisationApp.Tests;
public class FtsUrlServiceTests
{
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly FtsUrlService _service;

    public FtsUrlServiceTests()
    {
        _configurationMock = new Mock<IConfiguration>();
        _configurationMock.Setup(c => c["FtsService"]).Returns("https://example.com/");
        _service = new FtsUrlService(_configurationMock.Object);
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenFtsServiceIsNotConfigured()
    {
        _configurationMock.Setup(c => c["FtsService"]).Returns((string?)null);

        Action action = () => new FtsUrlService(_configurationMock.Object);

        action.Should().Throw<InvalidOperationException>()
                .WithMessage("FtsService is not configured.");
    }

    [Fact]
    public void BuildUrl_ShouldTrimTrailingSlashFromBaseServiceUrl()
    {
        _configurationMock.Setup(c => c["FtsService"]).Returns("https://example.com/");
        var service = new FtsUrlService(_configurationMock.Object);
        var endpoint = "test-endpoint";
        CultureInfo.CurrentUICulture = new CultureInfo("en-GB");

        var result = service.BuildUrl(endpoint);

        result.Should().Be("https://example.com/test-endpoint?language=en_GB");
    }

    [Fact]
    public void BuildUrl_ShouldConstructCorrectUrl_WhenOnlyEndpointIsProvided()
    {
        var endpoint = "test-endpoint";
        CultureInfo.CurrentUICulture = new CultureInfo("en-GB");

        var result = _service.BuildUrl(endpoint);

        result.Should().Be("https://example.com/test-endpoint?language=en_GB");
    }

    [Fact]
    public void BuildUrl_ShouldConstructCorrectUrl_WhenLanguageisWelsh()
    {
        var endpoint = "test-endpoint";
        CultureInfo.CurrentUICulture = new CultureInfo("cy");

        var result = _service.BuildUrl(endpoint);

        result.Should().Be("https://example.com/test-endpoint?language=cy");
    }

    [Fact]
    public void BuildUrl_ShouldIncludeOrganisationId_WhenProvided()
    {
        var endpoint = "test-endpoint";
        var organisationId = Guid.NewGuid();
        CultureInfo.CurrentUICulture = new CultureInfo("en-GB");

        var result = _service.BuildUrl(endpoint, organisationId);

        result.Should().Be($"https://example.com/test-endpoint?language=en_GB&organisation_id={organisationId}");
    }

    [Fact]
    public void BuildUrl_ShouldIncludeRedirectUrl_WhenProvided()
    {
        var endpoint = "test-endpoint";
        var redirectUrl = "/redirect-path";
        CultureInfo.CurrentUICulture = new CultureInfo("en-GB");

        var result = _service.BuildUrl(endpoint, null, redirectUrl);

        result.Should().Be("https://example.com/test-endpoint?language=en_GB&redirect_url=%2Fredirect-path");
    }

    [Fact]
    public void BuildUrl_ShouldIncludeAllParameters_WhenAllAreProvided()
    {
        var endpoint = "test-endpoint";
        var organisationId = Guid.NewGuid();
        var redirectUrl = "/redirect-path";
        CultureInfo.CurrentUICulture = new CultureInfo("en-GB");

        var result = _service.BuildUrl(endpoint, organisationId, redirectUrl);

        result.Should().Be($"https://example.com/test-endpoint?language=en_GB&organisation_id={organisationId}&redirect_url=%2Fredirect-path");
    }
}
