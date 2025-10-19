using CO.CDP.UI.Foundation.Cookies;
using CO.CDP.UI.Foundation.Services;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Globalization;
using System.Text;
using Xunit;
using FluentAssertions;

namespace CO.CDP.UI.Foundation.Tests.Services;

public class SirsiUrlServiceTests
{
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly Mock<ICookiePreferencesService> _mockCookiePreferencesService;
    private readonly SirsiUrlOptions _sirsiUrlOptions;
    private readonly Dictionary<string, string> _sessionItems;

    public SirsiUrlServiceTests()
    {
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        var mockHttpContext = new Mock<HttpContext>();
        var mockSession = new Mock<ISession>();
        _mockCookiePreferencesService = new Mock<ICookiePreferencesService>();
        _sessionItems = new Dictionary<string, string>();

        _mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(mockHttpContext.Object);
        mockHttpContext.Setup(c => c.Session).Returns(mockSession.Object);

        mockSession.Setup(s => s.TryGetValue(It.IsAny<string>(), out It.Ref<byte[]>.IsAny!))
            .Callback((string key, out byte[] value) =>
            {
                value = (_sessionItems.TryGetValue(key, out var stringValue)
                    ? Encoding.UTF8.GetBytes(stringValue)
                    : null)!;
            })
            .Returns((string key, out byte[] value) =>
            {
                var exists = _sessionItems.TryGetValue(key, out var stringValue);
                value = (exists ? Encoding.UTF8.GetBytes(stringValue!) : null)!;
                return exists;
            });

        _sirsiUrlOptions = new SirsiUrlOptions
        {
            ServiceBaseUrl = "https://sirsi-service.example.com",
            SessionKey = "SirsiServiceOrigin"
        };

        Thread.CurrentThread.CurrentCulture = new CultureInfo("en-GB");
        Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-GB");
    }

    [Fact]
    public void BuildUrl_ShouldConstructBasicUrl_WithEndpoint()
    {
        var service = new SirsiUrlService(_sirsiUrlOptions, _mockHttpContextAccessor.Object);

        var url = service.BuildUrl("/test-endpoint");

        url.Should().Be("https://sirsi-service.example.com/test-endpoint?language=en_GB");
    }

    [Fact]
    public void BuildUrl_ShouldIncludeOrganisationId_WhenProvided()
    {
        var service = new SirsiUrlService(_sirsiUrlOptions, _mockHttpContextAccessor.Object);
        var organisationId = Guid.Parse("12345678-1234-1234-1234-123456789012");

        var url = service.BuildUrl("/test-endpoint", organisationId);

        url.Should()
            .Be(
                "https://sirsi-service.example.com/test-endpoint?language=en_GB&organisation_id=12345678-1234-1234-1234-123456789012");
    }

    [Fact]
    public void BuildUrl_ShouldIncludeRedirectUrl_WhenProvided()
    {
        var service = new SirsiUrlService(_sirsiUrlOptions, _mockHttpContextAccessor.Object);

        var url = service.BuildUrl("/test-endpoint", redirectUri: "https://return.example.com");

        url.Should()
            .Be(
                "https://sirsi-service.example.com/test-endpoint?language=en_GB&redirectUri=https%3A%2F%2Freturn.example.com");
    }

    [Fact]
    public void BuildUrl_ShouldIncludeCookieAcceptance_WhenCookieServiceProvided()
    {
        _mockCookiePreferencesService.Setup(s => s.GetValue()).Returns(CookieAcceptanceValues.Accept);
        var service = new SirsiUrlService(_sirsiUrlOptions, _mockHttpContextAccessor.Object,
            _mockCookiePreferencesService.Object);

        var url = service.BuildUrl("/test-endpoint");

        url.Should().Be("https://sirsi-service.example.com/test-endpoint?language=en_GB&cookies_accepted=true");
    }

    [Fact]
    public void BuildUrl_ShouldUseSessionOrigin_WhenAvailable()
    {
        _sessionItems.Add("SirsiServiceOrigin", "https://session-sirsi.example.com");
        var service = new SirsiUrlService(_sirsiUrlOptions, _mockHttpContextAccessor.Object);

        var url = service.BuildUrl("/test-endpoint");

        url.Should().Be("https://session-sirsi.example.com/test-endpoint?language=en_GB");
    }

    [Fact]
    public void BuildUrl_ShouldThrowException_WhenNoServiceUrlConfigured()
    {
        var emptyOptions = new SirsiUrlOptions
        {
            ServiceBaseUrl = null!,
            SessionKey = "SirsiServiceOrigin"
        };

        Action action = () => { _ = new SirsiUrlService(emptyOptions, _mockHttpContextAccessor.Object); };

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Service base URL is not configured.");
    }

    [Fact]
    public void BuildUrl_ShouldTrimTrailingSlashFromBaseServiceUrl()
    {
        var optionsWithTrailingSlash = new SirsiUrlOptions
        {
            ServiceBaseUrl = "https://sirsi-service.example.com/",
            SessionKey = "SirsiServiceOrigin"
        };
        var service = new SirsiUrlService(optionsWithTrailingSlash,
            _mockHttpContextAccessor.Object);

        var url = service.BuildUrl("test-endpoint");

        url.Should().Be("https://sirsi-service.example.com/test-endpoint?language=en_GB");
    }

    [Fact]
    public void BuildUrl_ShouldWorkWithWelshLanguage()
    {
        var service = new SirsiUrlService(_sirsiUrlOptions, _mockHttpContextAccessor.Object);

        var savedCulture = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = new CultureInfo("cy");
            var url = service.BuildUrl("/test-endpoint");

            url.Should().Be("https://sirsi-service.example.com/test-endpoint?language=cy");
        }
        finally
        {
            CultureInfo.CurrentUICulture = savedCulture;
        }
    }

    [Fact]
    public void BuildUrl_ShouldIncludeAllParameters_WhenAllProvided()
    {
        _mockCookiePreferencesService.Setup(s => s.GetValue()).Returns(CookieAcceptanceValues.Accept);
        var service = new SirsiUrlService(_sirsiUrlOptions, _mockHttpContextAccessor.Object,
            _mockCookiePreferencesService.Object);
        var organisationId = Guid.Parse("12345678-1234-1234-1234-123456789012");

        var url = service.BuildUrl("/test-endpoint", organisationId, "https://return.example.com");

        url.Should()
            .Be(
                "https://sirsi-service.example.com/test-endpoint?language=en_GB&organisation_id=12345678-1234-1234-1234-123456789012&redirectUri=https%3A%2F%2Freturn.example.com");
    }

    [Theory]
    [InlineData(CookieAcceptanceValues.Accept, "true")]
    [InlineData(CookieAcceptanceValues.Reject, "false")]
    [InlineData(CookieAcceptanceValues.Unknown, "unknown")]
    public void BuildUrl_ShouldIncludeCorrectCookieAcceptanceValue(CookieAcceptanceValues cookieValue,
        string expectedValue)
    {
        _mockCookiePreferencesService.Setup(s => s.GetValue()).Returns(cookieValue);
        var service = new SirsiUrlService(_sirsiUrlOptions, _mockHttpContextAccessor.Object,
            _mockCookiePreferencesService.Object);

        var url = service.BuildUrl("/test-endpoint");

        url.Should()
            .Be($"https://sirsi-service.example.com/test-endpoint?language=en_GB&cookies_accepted={expectedValue}");
    }

    [Fact]
    public void BuildAuthenticatedUrl_ShouldWrapUrlInOneLoginSignIn()
    {
        var service = new SirsiUrlService(_sirsiUrlOptions, _mockHttpContextAccessor.Object);

        var url = service.BuildAuthenticatedUrl("/organisation-selection");

        url.Should()
            .Be(
                "https://sirsi-service.example.com/one-login/sign-in?language=en_GB&redirectUri=%2Forganisation-selection%3Flanguage%3Den_GB");
    }

    [Fact]
    public void BuildAuthenticatedUrl_ShouldIncludeOrganisationId_WhenProvided()
    {
        var service = new SirsiUrlService(_sirsiUrlOptions, _mockHttpContextAccessor.Object);
        var organisationId = Guid.Parse("12345678-1234-1234-1234-123456789012");

        var url = service.BuildAuthenticatedUrl("/organisation-selection", organisationId);

        url.Should()
            .Be(
                "https://sirsi-service.example.com/one-login/sign-in?language=en_GB&redirectUri=%2Forganisation-selection%3Flanguage%3Den_GB%26organisation_id%3D12345678-1234-1234-1234-123456789012");
    }

    [Fact]
    public void BuildAuthenticatedUrl_ShouldIncludeCookieAcceptance_WhenCookieServiceProvided()
    {
        _mockCookiePreferencesService.Setup(s => s.GetValue()).Returns(CookieAcceptanceValues.Accept);
        var service = new SirsiUrlService(_sirsiUrlOptions, _mockHttpContextAccessor.Object,
            _mockCookiePreferencesService.Object);

        var url = service.BuildAuthenticatedUrl("/organisation-selection");

        url.Should()
            .Contain("redirectUri=%2Forganisation-selection%3Flanguage%3Den_GB%26cookies_accepted%3Dtrue")
            .And.NotContain("&cookies_accepted=");
    }

    [Fact]
    public void BuildUrl_ShouldNotDuplicateQueryParameters()
    {
        var service = new SirsiUrlService(_sirsiUrlOptions, _mockHttpContextAccessor.Object);

        var url = service.BuildUrl("/test-endpoint");

        url.Should().Contain("language=en_GB");
        url.Split("language=").Length.Should().Be(2, "language parameter should only appear once");
    }
}