using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Http;

namespace CO.CDP.OrganisationApp.Tests;

public class CookiePreferencesServiceTests
{
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly Mock<HttpContext> _httpContextMock;
    private readonly Mock<HttpRequest> _httpRequestMock;
    private readonly Mock<HttpResponse> _httpResponseMock;
    private readonly Mock<IResponseCookies> _responseCookiesMock;
    private readonly Mock<IRequestCookieCollection> _requestCookiesMock;
    private readonly CookiePreferencesService _cookiePreferencesService;

    public CookiePreferencesServiceTests()
    {
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _httpContextMock = new Mock<HttpContext>();
        _httpRequestMock = new Mock<HttpRequest>();
        _httpResponseMock = new Mock<HttpResponse>();
        _responseCookiesMock = new Mock<IResponseCookies>();
        _requestCookiesMock = new Mock<IRequestCookieCollection>();

        _httpContextMock.Setup(c => c.Request).Returns(_httpRequestMock.Object);
        _httpContextMock.Setup(c => c.Response).Returns(_httpResponseMock.Object);
        _httpRequestMock.Setup(r => r.Cookies).Returns(_requestCookiesMock.Object);
        _httpResponseMock.Setup(r => r.Cookies).Returns(_responseCookiesMock.Object);

        _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(_httpContextMock.Object);

        _cookiePreferencesService = new CookiePreferencesService(_httpContextAccessorMock.Object);
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenHttpContextIsNull()
    {
        var accessor = new Mock<IHttpContextAccessor>();
        accessor.Setup(a => a.HttpContext).Returns((HttpContext)null!);

        Action act = () => new CookiePreferencesService(accessor.Object);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("No active HTTP context.");
    }

    [Fact]
    public void Accept_ShouldSetCookieWithAcceptValue()
    {
        _cookiePreferencesService.Accept();

        _responseCookiesMock.Verify(c => c.Append(CookieSettings.CookieName, "Accept", It.IsAny<CookieOptions>()), Times.Once);
        _cookiePreferencesService.GetValue().Should().Be(CookieAcceptanceValues.Accept);
        _cookiePreferencesService.IsAccepted().Should().BeTrue();
    }

    [Fact]
    public void Reject_ShouldSetCookieWithRejectValue()
    {
        _cookiePreferencesService.Reject();

        _responseCookiesMock.Verify(c => c.Append(CookieSettings.CookieName, "Reject", It.IsAny<CookieOptions>()), Times.Once);
        _cookiePreferencesService.GetValue().Should().Be(CookieAcceptanceValues.Reject);
        _cookiePreferencesService.IsRejected().Should().BeTrue();
    }

    [Fact]
    public void Reset_ShouldDeleteCookie_AndSetPendingValueToUnknown()
    {
        _cookiePreferencesService.Reset();

        _responseCookiesMock.Verify(c => c.Delete(CookieSettings.CookieName), Times.Once);
        _cookiePreferencesService.GetValue().Should().Be(CookieAcceptanceValues.Unknown);
        _cookiePreferencesService.IsUnknown().Should().BeTrue();
    }

    [Fact]
    public void GetValue_ShouldReturnPendingValue_WhenSet()
    {
        _cookiePreferencesService.Accept();

        _cookiePreferencesService.GetValue().Should().Be(CookieAcceptanceValues.Accept);
    }

    [Fact]
    public void GetValue_ShouldReturnValueFromRequestCookie_WhenNoPendingValue()
    {
        _requestCookiesMock.Setup(c => c.ContainsKey(CookieSettings.CookieName)).Returns(true);
        _requestCookiesMock.Setup(c => c[CookieSettings.CookieName]).Returns("Reject");

        _cookiePreferencesService.GetValue().Should().Be(CookieAcceptanceValues.Reject);
    }

    [Fact]
    public void GetValue_ShouldReturnUnknown_WhenCookieIsNotPresent()
    {
        _requestCookiesMock.Setup(c => c.ContainsKey(CookieSettings.CookieName)).Returns(false);

        _cookiePreferencesService.GetValue().Should().Be(CookieAcceptanceValues.Unknown);
    }
}

