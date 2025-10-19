using CO.CDP.UI.Foundation.Cookies;
using CO.CDP.UI.Foundation.Services;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Globalization;
using System.Text;
using Xunit;
using FluentAssertions;

namespace CO.CDP.UI.Foundation.Tests.Services;

public class CommercialToolsUrlServiceTests
{
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly Mock<ICookiePreferencesService> _mockCookiePreferencesService;
    private readonly CommercialToolsUrlOptions _commercialToolsUrlOptions;
    private readonly Dictionary<string, string> _sessionItems;

    public CommercialToolsUrlServiceTests()
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

        _commercialToolsUrlOptions = new CommercialToolsUrlOptions
        {
            ServiceBaseUrl = "https://commercial-tools-service.example.com",
            SessionKey = "CommercialToolsServiceOrigin"
        };

        Thread.CurrentThread.CurrentCulture = new CultureInfo("en-GB");
        Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-GB");
    }

    [Fact]
    public void BuildUrl_ShouldConstructBasicUrl_WithEndpoint()
    {
        var service = new CommercialToolsUrlService(_commercialToolsUrlOptions, _mockHttpContextAccessor.Object);

        var url = service.BuildUrl("/test-endpoint");

        url.Should().Be("https://commercial-tools-service.example.com/test-endpoint?language=en_GB");
    }

    [Fact]
    public void BuildUrl_ShouldIncludeOrganisationId_WhenProvided()
    {
        var service = new CommercialToolsUrlService(_commercialToolsUrlOptions, _mockHttpContextAccessor.Object);
        var organisationId = Guid.Parse("12345678-1234-1234-1234-123456789012");

        var url = service.BuildUrl("/test-endpoint", organisationId);

        url.Should()
            .Be(
                "https://commercial-tools-service.example.com/test-endpoint?language=en_GB&organisation_id=12345678-1234-1234-1234-123456789012");
    }

    [Fact]
    public void BuildUrl_ShouldIncludeRedirectUrl_WhenProvided()
    {
        var service = new CommercialToolsUrlService(_commercialToolsUrlOptions, _mockHttpContextAccessor.Object);

        var url = service.BuildUrl("/test-endpoint", redirectUri: "https://return.example.com");

        url.Should()
            .Be(
                "https://commercial-tools-service.example.com/test-endpoint?language=en_GB&redirectUri=https%3A%2F%2Freturn.example.com");
    }

    [Fact]
    public void BuildUrl_ShouldIncludeCookieAcceptance_WhenCookieServiceProvided()
    {
        _mockCookiePreferencesService.Setup(s => s.GetValue()).Returns(CookieAcceptanceValues.Accept);
        var service = new CommercialToolsUrlService(_commercialToolsUrlOptions, _mockHttpContextAccessor.Object,
            _mockCookiePreferencesService.Object);

        var url = service.BuildUrl("/test-endpoint");

        url.Should().Be("https://commercial-tools-service.example.com/test-endpoint?language=en_GB&cookies_accepted=true");
    }

    [Fact]
    public void BuildUrl_ShouldUseSessionOrigin_WhenAvailable()
    {
        _sessionItems.Add("CommercialToolsServiceOrigin", "https://session-commercial-tools.example.com");
        var service = new CommercialToolsUrlService(_commercialToolsUrlOptions, _mockHttpContextAccessor.Object);

        var url = service.BuildUrl("/test-endpoint");

        url.Should().Be("https://session-commercial-tools.example.com/test-endpoint?language=en_GB");
    }

    [Fact]
    public void BuildUrl_ShouldThrowException_WhenNoServiceUrlConfigured()
    {
        var emptyOptions = new CommercialToolsUrlOptions
        {
            ServiceBaseUrl = null!,
            SessionKey = "CommercialToolsServiceOrigin"
        };

        Action action = () =>
        {
            _ = new CommercialToolsUrlService(emptyOptions, _mockHttpContextAccessor.Object);
        };

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Service base URL is not configured.");
    }

    [Fact]
    public void BuildUrl_ShouldTrimTrailingSlashFromBaseServiceUrl()
    {
        var optionsWithTrailingSlash = new CommercialToolsUrlOptions
        {
            ServiceBaseUrl = "https://commercial-tools-service.example.com/",
            SessionKey = "CommercialToolsServiceOrigin"
        };
        var service = new CommercialToolsUrlService(optionsWithTrailingSlash,
            _mockHttpContextAccessor.Object);

        var url = service.BuildUrl("test-endpoint");

        url.Should().Be("https://commercial-tools-service.example.com/test-endpoint?language=en_GB");
    }

    [Fact]
    public void BuildUrl_ShouldWorkWithWelshLanguage()
    {
        var service = new CommercialToolsUrlService(_commercialToolsUrlOptions, _mockHttpContextAccessor.Object);

        var savedCulture = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = new CultureInfo("cy");
            var url = service.BuildUrl("/test-endpoint");

            url.Should().Be("https://commercial-tools-service.example.com/test-endpoint?language=cy");
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
        var service = new CommercialToolsUrlService(_commercialToolsUrlOptions, _mockHttpContextAccessor.Object,
            _mockCookiePreferencesService.Object);
        var organisationId = Guid.Parse("12345678-1234-1234-1234-123456789012");

        var url = service.BuildUrl("/test-endpoint", organisationId, "https://return.example.com");

        url.Should()
            .Be(
                "https://commercial-tools-service.example.com/test-endpoint?language=en_GB&organisation_id=12345678-1234-1234-1234-123456789012&redirectUri=https%3A%2F%2Freturn.example.com");
    }

    [Theory]
    [InlineData(CookieAcceptanceValues.Accept, "true")]
    [InlineData(CookieAcceptanceValues.Reject, "false")]
    [InlineData(CookieAcceptanceValues.Unknown, "unknown")]
    public void BuildUrl_ShouldIncludeCorrectCookieAcceptanceValue(CookieAcceptanceValues cookieValue, string expectedValue)
    {
        _mockCookiePreferencesService.Setup(s => s.GetValue()).Returns(cookieValue);
        var service = new CommercialToolsUrlService(_commercialToolsUrlOptions, _mockHttpContextAccessor.Object,
            _mockCookiePreferencesService.Object);

        var url = service.BuildUrl("/test-endpoint");

        url.Should()
            .Be($"https://commercial-tools-service.example.com/test-endpoint?language=en_GB&cookies_accepted={expectedValue}");
    }

    [Theory]
    [InlineData(true, "true")]
    [InlineData(false, "false")]
    public void BuildUrl_ShouldUseExplicitCookieAcceptanceValue_WhenProvided(bool explicitAcceptance, string expectedValue)
    {
        _mockCookiePreferencesService.Setup(s => s.GetValue()).Returns(CookieAcceptanceValues.Reject); // Different from explicit
        var service = new CommercialToolsUrlService(_commercialToolsUrlOptions, _mockHttpContextAccessor.Object,
            _mockCookiePreferencesService.Object);

        var url = service.BuildUrl("/test-endpoint", cookieAcceptance: explicitAcceptance);

        url.Should()
            .Be($"https://commercial-tools-service.example.com/test-endpoint?language=en_GB&cookies_accepted={expectedValue}");
    }
}