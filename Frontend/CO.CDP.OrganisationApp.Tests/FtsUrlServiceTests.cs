using System.Globalization;
using Microsoft.Extensions.Configuration;
using Moq;
using FluentAssertions;

namespace CO.CDP.OrganisationApp.Tests;

public class FtsUrlServiceTests
{
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<ICookiePreferencesService> _cookiePreferencesService;
    private readonly IFtsUrlService _service;

    public FtsUrlServiceTests()
    {
        _configurationMock = new Mock<IConfiguration>();
        _cookiePreferencesService = new Mock<ICookiePreferencesService>();
        _configurationMock.Setup(c => c["FtsService"]).Returns("https://example.com/");
        _service = new FtsUrlService(_configurationMock.Object, _cookiePreferencesService.Object);
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenFtsServiceIsNotConfigured()
    {
        _configurationMock.Setup(c => c["FtsService"]).Returns((string?)null);

        Action action = () => new FtsUrlService(_configurationMock.Object, _cookiePreferencesService.Object);

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("FtsService is not configured.");
    }

    [Fact]
    public void BuildUrl_ShouldTrimTrailingSlashFromBaseServiceUrl()
    {
        _configurationMock.Setup(c => c["FtsService"]).Returns("https://example.com/");
        var service = new FtsUrlService(_configurationMock.Object, _cookiePreferencesService.Object);
        var endpoint = "test-endpoint";
        CultureInfo.CurrentUICulture = new CultureInfo("en-GB");

        var result = service.BuildUrl(endpoint);

        result.Should().Be("https://example.com/test-endpoint?language=en_GB&cookies_accepted=unknown");
    }

    [Fact]
    public void BuildUrl_ShouldConstructCorrectUrl_WhenOnlyEndpointIsProvided()
    {
        var endpoint = "test-endpoint";
        CultureInfo.CurrentUICulture = new CultureInfo("en-GB");

        var result = _service.BuildUrl(endpoint);

        result.Should().Be("https://example.com/test-endpoint?language=en_GB&cookies_accepted=unknown");
    }

    [Fact]
    public void BuildUrl_ShouldConstructCorrectUrl_WhenLanguageIsWelsh()
    {
        var endpoint = "test-endpoint";
        CultureInfo.CurrentUICulture = new CultureInfo("cy");

        var result = _service.BuildUrl(endpoint);

        result.Should().Be("https://example.com/test-endpoint?language=cy&cookies_accepted=unknown");
    }

    [Fact]
    public void BuildUrl_ShouldIncludeOrganisationId_WhenProvided()
    {
        var endpoint = "test-endpoint";
        var organisationId = Guid.NewGuid();
        CultureInfo.CurrentUICulture = new CultureInfo("en-GB");

        var result = _service.BuildUrl(endpoint, organisationId);

        result.Should().Be($"https://example.com/test-endpoint?language=en_GB&organisation_id={organisationId}&cookies_accepted=unknown");
    }

    [Fact]
    public void BuildUrl_ShouldIncludeRedirectUrl_WhenProvided()
    {
        var endpoint = "test-endpoint";
        var redirectUrl = "/redirect-path";
        CultureInfo.CurrentUICulture = new CultureInfo("en-GB");

        var result = _service.BuildUrl(endpoint, null, redirectUrl);

        result.Should().Be("https://example.com/test-endpoint?language=en_GB&redirect_url=%2Fredirect-path&cookies_accepted=unknown");
    }

    [Fact]
    public void BuildUrl_ShouldIncludeAllParameters_WhenAllAreProvided()
    {
        _cookiePreferencesService.Setup(c => c.GetValue()).Returns(CookieAcceptanceValues.Accept);
        var endpoint = "test-endpoint";
        var organisationId = Guid.NewGuid();
        var redirectUrl = "/redirect-path";
        CultureInfo.CurrentUICulture = new CultureInfo("en-GB");

        var result = _service.BuildUrl(endpoint, organisationId, redirectUrl);

        result.Should().Be($"https://example.com/test-endpoint?language=en_GB&organisation_id={organisationId}&redirect_url=%2Fredirect-path&cookies_accepted=true");
    }

    [Theory]
    [InlineData(CookieAcceptanceValues.Accept, "true")]
    [InlineData(CookieAcceptanceValues.Reject, "false")]
    [InlineData(CookieAcceptanceValues.Unknown, "unknown")]
    public void BuildUrl_ShouldIncludeCookiesAcceptedParameter_BasedOnCookiePreferences(CookieAcceptanceValues preference, string expectedValue)
    {
        _cookiePreferencesService.Setup(c => c.GetValue()).Returns(preference);
        var endpoint = "test-endpoint";
        CultureInfo.CurrentUICulture = new CultureInfo("en-GB");

        var result = _service.BuildUrl(endpoint);

        result.Should().Be($"https://example.com/test-endpoint?language=en_GB&cookies_accepted={expectedValue}");
    }
}
