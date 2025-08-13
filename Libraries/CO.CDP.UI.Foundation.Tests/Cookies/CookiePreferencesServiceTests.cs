using CO.CDP.UI.Foundation.Cookies;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace CO.CDP.UI.Foundation.Tests.Cookies;

public class CookiePreferencesServiceTests
{
    private readonly Mock<IResponseCookies> _mockResponseCookies;
    private readonly CookieSettings _cookieSettings;
    private readonly CookiePreferencesService _service;
    private readonly Dictionary<string, string> _requestCookies;

    public CookiePreferencesServiceTests()
    {
        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        var mockHttpContext = new Mock<HttpContext>();
        var mockRequest = new Mock<HttpRequest>();
        var mockResponse = new Mock<HttpResponse>();
        _mockResponseCookies = new Mock<IResponseCookies>();
        _requestCookies = new Dictionary<string, string>();

        mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(mockHttpContext.Object);
        mockHttpContext.Setup(c => c.Request).Returns(mockRequest.Object);
        mockHttpContext.Setup(c => c.Response).Returns(mockResponse.Object);
        mockResponse.Setup(r => r.Cookies).Returns(_mockResponseCookies.Object);

        mockRequest.Setup(r => r.Cookies).Returns(new RequestCookieCollection(_requestCookies));
        mockRequest.Setup(r => r.IsHttps).Returns(true);

        _cookieSettings = new CookieSettings
        {
            CookieName = "TEST_COOKIES_PREFERENCES_SET",
            ExpiryDays = 365
        };

        _service = new CookiePreferencesService(mockHttpContextAccessor.Object, _cookieSettings);
    }

    [Fact]
    public void IsUnknown_ShouldReturnTrue_WhenNoCookieExists()
    {
        var result = _service.IsUnknown();

        Assert.True(result);
    }

    [Fact]
    public void IsAccepted_ShouldReturnTrue_WhenCookieIsAccepted()
    {
        _requestCookies.Add(_cookieSettings.CookieName, CookieAcceptanceValues.Accept.ToString());

        var result = _service.IsAccepted();

        Assert.True(result);
    }

    [Fact]
    public void IsRejected_ShouldReturnTrue_WhenCookieIsRejected()
    {
        _requestCookies.Add(_cookieSettings.CookieName, CookieAcceptanceValues.Reject.ToString());

        var result = _service.IsRejected();

        Assert.True(result);
    }

    [Fact]
    public void Accept_ShouldSetAcceptCookie()
    {
        _service.Accept();

        _mockResponseCookies.Verify(c => c.Append(
            It.Is<string>(s => s == _cookieSettings.CookieName),
            It.Is<string>(s => s == CookieAcceptanceValues.Accept.ToString()),
            It.IsAny<CookieOptions>()
        ));
    }

    [Fact]
    public void Reject_ShouldSetRejectCookie()
    {
        _service.Reject();

        _mockResponseCookies.Verify(c => c.Append(
            It.Is<string>(s => s == _cookieSettings.CookieName),
            It.Is<string>(s => s == CookieAcceptanceValues.Reject.ToString()),
            It.IsAny<CookieOptions>()
        ));
    }

    [Fact]
    public void Reset_ShouldDeleteCookie()
    {
        _service.Reset();

        _mockResponseCookies.Verify(c => c.Delete(
            It.Is<string>(s => s == _cookieSettings.CookieName)
        ));
    }

    [Fact]
    public void GetValue_ShouldReturnUnknown_WhenNoCookieExists()
    {
        var result = _service.GetValue();

        Assert.Equal(CookieAcceptanceValues.Unknown, result);
    }

    [Fact]
    public void GetValue_ShouldReturnAccept_WhenCookieIsAccepted()
    {
        _requestCookies.Add(_cookieSettings.CookieName, CookieAcceptanceValues.Accept.ToString());

        var result = _service.GetValue();

        Assert.Equal(CookieAcceptanceValues.Accept, result);
    }

    [Fact]
    public void GetValue_ShouldReturnReject_WhenCookieIsRejected()
    {
        _requestCookies.Add(_cookieSettings.CookieName, CookieAcceptanceValues.Reject.ToString());

        var result = _service.GetValue();

        Assert.Equal(CookieAcceptanceValues.Reject, result);
    }

    [Fact]
    public void GetValue_ShouldReturnPendingValue_AfterAccept()
    {
        _service.Accept();

        var result = _service.GetValue();

        Assert.Equal(CookieAcceptanceValues.Accept, result);
    }

    [Fact]
    public void GetValue_ShouldReturnPendingValue_AfterReject()
    {
        _service.Reject();

        var result = _service.GetValue();

        Assert.Equal(CookieAcceptanceValues.Reject, result);
    }

    [Fact]
    public void GetValue_ShouldReturnUnknown_AfterReset()
    {
        _service.Accept();
        _service.Reset();

        var result = _service.GetValue();

        Assert.Equal(CookieAcceptanceValues.Unknown, result);
    }
}

public class RequestCookieCollection(Dictionary<string, string> cookies) : IRequestCookieCollection
{
    public string? this[string key] => cookies.TryGetValue(key, out var value) ? value : null;

    public int Count => cookies.Count;

    public ICollection<string> Keys => cookies.Keys;

    public bool ContainsKey(string key) => cookies.ContainsKey(key);

    public IEnumerator<KeyValuePair<string, string>> GetEnumerator() => cookies.GetEnumerator();

    bool IRequestCookieCollection.TryGetValue(string key, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out string? value)
    {
        if (cookies.TryGetValue(key, out var tempValue))
        {
            value = tempValue;
            return true;
        }

        value = null;
        return false;
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => cookies.GetEnumerator();
}
