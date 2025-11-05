using CO.CDP.UI.Foundation.Cookies;
using CO.CDP.UI.Foundation.Services;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Globalization;
using System.Text;
using Xunit;
using FluentAssertions;

namespace CO.CDP.UI.Foundation.Tests.Services;

public class FtsUrlServiceTests
{
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly Mock<ICookiePreferencesService> _mockCookiePreferencesService;
    private readonly FtsUrlOptions _ftsUrlOptions;
    private readonly Dictionary<string, string> _sessionItems;

    public FtsUrlServiceTests()
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

        _ftsUrlOptions = new FtsUrlOptions
        {
            ServiceBaseUrl = "https://fts-service.example.com",
            SessionKey = "FtsServiceOrigin"
        };

        Thread.CurrentThread.CurrentCulture = new CultureInfo("en-GB");
        Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-GB");
    }

    [Fact]
    public void BuildUrl_ShouldConstructBasicUrl_WithEndpoint()
    {
        var service = new FtsUrlService(_ftsUrlOptions, _mockHttpContextAccessor.Object);

        var url = service.BuildUrl("/test-endpoint");

        url.Should().Be("https://fts-service.example.com/test-endpoint?language=en_GB");
    }

    [Fact]
    public void BuildUrl_ShouldIncludeOrganisationId_WhenProvided()
    {
        var service = new FtsUrlService(_ftsUrlOptions, _mockHttpContextAccessor.Object);
        var organisationId = Guid.Parse("12345678-1234-1234-1234-123456789012");

        var url = service.BuildUrl("/test-endpoint", organisationId);

        url.Should()
            .Be(
                "https://fts-service.example.com/test-endpoint?language=en_GB&organisation_id=12345678-1234-1234-1234-123456789012");
    }

    [Fact]
    public void BuildUrl_ShouldIncludeRedirectUrl_WhenProvided()
    {
        var service = new FtsUrlService(_ftsUrlOptions, _mockHttpContextAccessor.Object);

        var url = service.BuildUrl("/test-endpoint", redirectUri: "https://return.example.com");

        url.Should()
            .Be(
                "https://fts-service.example.com/test-endpoint?language=en_GB&redirect_url=https%3A%2F%2Freturn.example.com");
    }

    [Fact]
    public void BuildUrl_ShouldIncludeCookieAcceptance_WhenCookieServiceProvided()
    {
        _mockCookiePreferencesService.Setup(s => s.IsAccepted()).Returns(true);
        var service = new FtsUrlService(_ftsUrlOptions, _mockHttpContextAccessor.Object,
            _mockCookiePreferencesService.Object);

        var url = service.BuildUrl("/test-endpoint", null, null, true);

        url.Should().Be("https://fts-service.example.com/test-endpoint?language=en_GB&cookies_accepted=true");
    }

    [Fact]
    public void BuildUrl_ShouldUseSessionOrigin_WhenAvailable()
    {
        _sessionItems.Add("FtsServiceOrigin", "https://session-fts.example.com");
        var service = new FtsUrlService(_ftsUrlOptions, _mockHttpContextAccessor.Object);

        var url = service.BuildUrl("/test-endpoint");

        url.Should().Be("https://session-fts.example.com/test-endpoint?language=en_GB");
    }

    [Fact]
    public void BuildUrl_ShouldThrowException_WhenNoServiceUrlConfigured()
    {
        var emptyOptions = new FtsUrlOptions
        {
            ServiceBaseUrl = null!,
            SessionKey = "FtsServiceOrigin"
        };

        Action action = () =>
        {
            _ = new FtsUrlService(emptyOptions, _mockHttpContextAccessor.Object);
        };

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Service base URL is not configured.");
    }

    [Fact]
    public void BuildUrl_ShouldTrimTrailingSlashFromBaseServiceUrl()
    {
        var optionsWithTrailingSlash = new FtsUrlOptions
        {
            ServiceBaseUrl = "https://fts-service.example.com/",
            SessionKey = "FtsServiceOrigin"
        };
        var service = new FtsUrlService(optionsWithTrailingSlash,
            _mockHttpContextAccessor.Object);

        var url = service.BuildUrl("test-endpoint");

        url.Should().Be("https://fts-service.example.com/test-endpoint?language=en_GB");
    }

    [Fact]
    public void BuildUrl_ShouldWorkWithWelshLanguage()
    {
        var service = new FtsUrlService(_ftsUrlOptions, _mockHttpContextAccessor.Object);

        var savedCulture = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = new CultureInfo("cy");
            var url = service.BuildUrl("/test-endpoint");

            url.Should().Be("https://fts-service.example.com/test-endpoint?language=cy");
        }
        finally
        {
            CultureInfo.CurrentUICulture = savedCulture;
        }
    }

    [Fact]
    public void BuildUrl_ShouldIncludeAllParameters_WhenAllProvided()
    {
        _mockCookiePreferencesService.Setup(s => s.IsAccepted()).Returns(true);
        var service = new FtsUrlService(_ftsUrlOptions, _mockHttpContextAccessor.Object,
            _mockCookiePreferencesService.Object);
        var organisationId = Guid.Parse("12345678-1234-1234-1234-123456789012");

        var url = service.BuildUrl("/test-endpoint", organisationId, "https://return.example.com", true, "redirectUri");

        url.Should()
            .Be(
                "https://fts-service.example.com/test-endpoint?language=en_GB&organisation_id=12345678-1234-1234-1234-123456789012&redirectUri=https%3A%2F%2Freturn.example.com&cookies_accepted=true");
    }

    [Theory]
    [InlineData(true, "true")]
    [InlineData(false, "false")]
    public void BuildUrl_ShouldIncludeCorrectCookieAcceptanceValue(bool isAccepted, string expectedValue)
    {
        _mockCookiePreferencesService.Setup(s => s.IsAccepted()).Returns(isAccepted);
        var service = new FtsUrlService(_ftsUrlOptions, _mockHttpContextAccessor.Object,
            _mockCookiePreferencesService.Object);

        var url = service.BuildUrl("/test-endpoint", null, null, isAccepted);

        url.Should()
            .Be($"https://fts-service.example.com/test-endpoint?language=en_GB&cookies_accepted={expectedValue}");
    }

    [Fact]
    public void BuildUrl_ShouldIncludeRedirectUrlParameterName_WhenProvided()
    {
        var service = new FtsUrlService(_ftsUrlOptions, _mockHttpContextAccessor.Object);

        var url = service.BuildUrl("/test-endpoint", redirectUri: "https://return.example.com", redirectUriParamName:"redirect_param_name_test");

        url.Should()
            .Be(
                "https://fts-service.example.com/test-endpoint?language=en_GB&redirect_param_name_test=https%3A%2F%2Freturn.example.com");
    }

    [Fact]
    public void BuildUrl_ShouldIncludeRedirectUrlParameterNameDefault_WhenNotProvided()
    {
        var service = new FtsUrlService(_ftsUrlOptions, _mockHttpContextAccessor.Object);

        var url = service.BuildUrl("/test-endpoint", redirectUri: "https://return.example.com");

        url.Should()
            .Be(
                "https://fts-service.example.com/test-endpoint?language=en_GB&redirect_url=https%3A%2F%2Freturn.example.com");
    }
}