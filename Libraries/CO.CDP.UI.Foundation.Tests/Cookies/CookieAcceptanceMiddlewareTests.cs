using CO.CDP.UI.Foundation.Cookies;
using Moq;
using Microsoft.AspNetCore.Http;
using CO.CDP.UI.Foundation.Middleware;
using Xunit;

namespace CO.CDP.UI.Foundation.Tests.Cookies;

public class CookieAcceptanceMiddlewareTests
{
    private readonly Mock<ICookiePreferencesService> _cookiePreferencesServiceMock;
    private readonly Mock<RequestDelegate> _nextDelegateMock;
    private readonly CookieAcceptanceMiddleware _middleware;
    private readonly DefaultHttpContext _httpContext;
    private readonly CookieSettings _cookieSettings = new CookieSettings();

    public CookieAcceptanceMiddlewareTests()
    {
        _cookiePreferencesServiceMock = new Mock<ICookiePreferencesService>();
        _nextDelegateMock = new Mock<RequestDelegate>();
        _middleware = new CookieAcceptanceMiddleware(_cookiePreferencesServiceMock.Object);
        _httpContext = new DefaultHttpContext();
    }

    [Fact]
    public async Task InvokeAsync_ShouldCallAccept_WhenQueryParameterIsTrue()
    {
        _httpContext.Request.QueryString = new QueryString($"?{_cookieSettings.CookiesAcceptedHandoverParameter}=true");

        await _middleware.InvokeAsync(_httpContext, _nextDelegateMock.Object);

        _cookiePreferencesServiceMock.Verify(s => s.Accept(), Times.Once);
        _nextDelegateMock.Verify(n => n(_httpContext), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_ShouldCallReject_WhenQueryParameterIsFalse()
    {
        _httpContext.Request.QueryString = new QueryString($"?{_cookieSettings.CookiesAcceptedHandoverParameter}=false");

        await _middleware.InvokeAsync(_httpContext, _nextDelegateMock.Object);

        _cookiePreferencesServiceMock.Verify(s => s.Reject(), Times.Once);
        _nextDelegateMock.Verify(n => n(_httpContext), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_ShouldCallReset_WhenQueryParameterIsUnknown()
    {
        _httpContext.Request.QueryString = new QueryString($"?{_cookieSettings.CookiesAcceptedHandoverParameter}=unknown");

        await _middleware.InvokeAsync(_httpContext, _nextDelegateMock.Object);

        _cookiePreferencesServiceMock.Verify(s => s.Reset(), Times.Once);
        _nextDelegateMock.Verify(n => n(_httpContext), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_ShouldNotCallAnyMethod_WhenQueryParameterIsMissing()
    {
        await _middleware.InvokeAsync(_httpContext, _nextDelegateMock.Object);

        _cookiePreferencesServiceMock.VerifyNoOtherCalls();
        _nextDelegateMock.Verify(n => n(_httpContext), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_ShouldNotCallAnyMethod_WhenQueryParameterHasInvalidValue()
    {
        _httpContext.Request.QueryString = new QueryString($"?{_cookieSettings.CookiesAcceptedHandoverParameter}=invalid");

        await _middleware.InvokeAsync(_httpContext, _nextDelegateMock.Object);

        _cookiePreferencesServiceMock.VerifyNoOtherCalls();
        _nextDelegateMock.Verify(n => n(_httpContext), Times.Once);
    }
}