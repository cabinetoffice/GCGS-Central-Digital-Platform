using CO.CDP.Localization;
using CO.CDP.UI.Foundation.Cookies;
using CO.CDP.UI.Foundation.Pages;
using CO.CDP.UI.Foundation.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using Xunit;

namespace CO.CDP.UI.Foundation.Tests.Models;

public class CookiesModelTests
{
    private readonly Mock<IFlashMessageService> _mockFlashMessageService;
    private readonly Mock<ICookiePreferencesService> _mockCookiePreferencesService;
    private readonly Mock<IUrlHelper> _mockUrlHelper;
    private readonly CookiesModel _model;
    private readonly Dictionary<string, string> _requestCookies;

    public CookiesModelTests()
    {
        _mockFlashMessageService = new Mock<IFlashMessageService>();
        _mockCookiePreferencesService = new Mock<ICookiePreferencesService>();
        _mockUrlHelper = new Mock<IUrlHelper>();
        _requestCookies = new Dictionary<string, string>();

        var mockRequest = new Mock<HttpRequest>();
        var mockHttpContext = new Mock<HttpContext>();

        mockHttpContext.Setup(c => c.Request).Returns(mockRequest.Object);
        mockRequest.Setup(r => r.Cookies).Returns(new CookieTestHelper(_requestCookies));

        _model = new CookiesModel(_mockFlashMessageService.Object, _mockCookiePreferencesService.Object)
        {
            PageContext = new PageContext
            {
                HttpContext = mockHttpContext.Object
            },
            Url = _mockUrlHelper.Object
        };
    }

    [Fact]
    public void OnGet_WithNoReturnUrl_SetsReturnUrlToNull()
    {
        _model.OnGet();

        Assert.Null(_model.ReturnUrl);
    }

    [Fact]
    public void OnGet_WithReturnUrl_SetsReturnUrlProperty()
    {
        var returnUrl = "/test-page";

        _model.OnGet(returnUrl);

        Assert.Equal(returnUrl, _model.ReturnUrl);
    }

    [Theory]
    [InlineData("Accept", CookieAcceptanceValues.Accept)]
    [InlineData("Reject", CookieAcceptanceValues.Reject)]
    public void OnGet_WithExistingCookie_SetsCookieAcceptanceValue(string cookieValue, CookieAcceptanceValues expectedValue)
    {
        var cookieSettings = new CookieSettings();
        _requestCookies[cookieSettings.CookieName] = cookieValue;

        _model.OnGet();

        Assert.Equal(expectedValue, _model.CookieAcceptance);
    }

    [Fact]
    public void OnGet_WithInvalidCookieValue_LeavesCookieAcceptanceNull()
    {
        var cookieSettings = new CookieSettings();
        _requestCookies[cookieSettings.CookieName] = "InvalidValue";

        _model.OnGet();

        Assert.Null(_model.CookieAcceptance);
    }

    [Fact]
    public void OnGet_WithNoCookie_LeavesCookieAcceptanceNull()
    {
        _model.OnGet();

        Assert.Null(_model.CookieAcceptance);
    }

    [Fact]
    public void OnPost_WithInvalidModelState_ReturnsPage()
    {
        _model.ModelState.AddModelError("CookieAcceptance", "Required");

        var result = _model.OnPost();

        Assert.IsType<PageResult>(result);
    }

    [Fact]
    public void OnPost_WithNullCookieAcceptance_ReturnsPage()
    {
        _model.CookieAcceptance = null;

        var result = _model.OnPost();

        Assert.IsType<PageResult>(result);
    }

    [Fact]
    public void OnPost_WithAcceptValue_CallsAcceptOnService()
    {
        _model.CookieAcceptance = CookieAcceptanceValues.Accept;

        _model.OnPost();

        _mockCookiePreferencesService.Verify(s => s.Accept(), Times.Once);
        _mockCookiePreferencesService.Verify(s => s.Reject(), Times.Never);
    }

    [Fact]
    public void OnPost_WithRejectValue_CallsRejectOnService()
    {
        _model.CookieAcceptance = CookieAcceptanceValues.Reject;

        _model.OnPost();

        _mockCookiePreferencesService.Verify(s => s.Reject(), Times.Once);
        _mockCookiePreferencesService.Verify(s => s.Accept(), Times.Never);
    }

    [Fact]
    public void OnPost_WithValidLocalReturnUrl_RedirectsToReturnUrl()
    {
        _model.CookieAcceptance = CookieAcceptanceValues.Accept;
        _model.ReturnUrl = "/test-page";
        _mockUrlHelper.Setup(u => u.IsLocalUrl("/test-page")).Returns(true);

        var result = _model.OnPost();

        var redirectResult = Assert.IsType<LocalRedirectResult>(result);
        Assert.Contains("/test-page", redirectResult.Url);
        Assert.Contains("cookieBannerInteraction=true", redirectResult.Url);
    }

    [Fact]
    public void OnPost_WithInvalidReturnUrl_DoesNotRedirect()
    {
        _model.CookieAcceptance = CookieAcceptanceValues.Accept;
        _model.ReturnUrl = "http://external-site.com";
        _mockUrlHelper.Setup(u => u.IsLocalUrl("http://external-site.com")).Returns(false);

        var result = _model.OnPost();

        Assert.IsType<RedirectToPageResult>(result);
    }

    [Fact]
    public void OnPost_WithNullReturnUrl_DoesNotRedirect()
    {
        _model.CookieAcceptance = CookieAcceptanceValues.Accept;
        _model.ReturnUrl = null;

        var result = _model.OnPost();

        Assert.IsType<RedirectToPageResult>(result);
    }

    [Fact]
    public void OnPost_WithEmptyReturnUrl_DoesNotRedirect()
    {
        _model.CookieAcceptance = CookieAcceptanceValues.Accept;
        _model.ReturnUrl = "";

        var result = _model.OnPost();

        Assert.IsType<RedirectToPageResult>(result);
    }

    [Fact]
    public void OnPost_WithoutValidReturnUrl_SetsFlashMessage()
    {
        _model.CookieAcceptance = CookieAcceptanceValues.Accept;
        _model.ReturnUrl = null;

        _model.OnPost();

        _mockFlashMessageService.Verify(
            s => s.SetFlashMessage(
                FlashMessageType.Success,
                StaticTextResource.Cookies_SetCookiePreferences,
                null,
                null,
                null,
                null),
            Times.Once);
    }

    [Fact]
    public void OnPost_WithoutValidReturnUrl_RedirectsToCookiesPage()
    {
        _model.CookieAcceptance = CookieAcceptanceValues.Accept;

        var result = _model.OnPost();

        var redirectResult = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/Cookies", redirectResult.PageName);
    }

    [Theory]
    [InlineData(CookieAcceptanceValues.Accept)]
    [InlineData(CookieAcceptanceValues.Reject)]
    public void OnPost_WithValidCookieAcceptance_ProcessesSuccessfully(CookieAcceptanceValues acceptance)
    {
        _model.CookieAcceptance = acceptance;

        var result = _model.OnPost();

        Assert.IsType<RedirectToPageResult>(result);

        if (acceptance == CookieAcceptanceValues.Accept)
        {
            _mockCookiePreferencesService.Verify(s => s.Accept(), Times.Once);
        }
        else
        {
            _mockCookiePreferencesService.Verify(s => s.Reject(), Times.Once);
        }
    }

    [Fact]
    public void OnPost_WithValidReturnUrlAndAccept_AddsCorrectQueryParameter()
    {
        _model.CookieAcceptance = CookieAcceptanceValues.Accept;
        _model.ReturnUrl = "/search?query=test";
        _mockUrlHelper.Setup(u => u.IsLocalUrl("/search?query=test")).Returns(true);

        var result = _model.OnPost();

        var redirectResult = Assert.IsType<LocalRedirectResult>(result);
        Assert.Contains("/search", redirectResult.Url);
        Assert.Contains("query=test", redirectResult.Url);
        Assert.Contains("cookieBannerInteraction=true", redirectResult.Url);
    }

    [Fact]
    public void OnPost_ModelStateInvalidWithReturnUrl_StillReturnsPage()
    {
        _model.CookieAcceptance = CookieAcceptanceValues.Accept;
        _model.ReturnUrl = "/test-page";
        _model.ModelState.AddModelError("Test", "Test Error");

        var result = _model.OnPost();

        Assert.IsType<PageResult>(result);
        _mockCookiePreferencesService.Verify(s => s.Accept(), Times.Never);
        _mockCookiePreferencesService.Verify(s => s.Reject(), Times.Never);
    }
}

internal class CookieTestHelper : IRequestCookieCollection
{
    private readonly Dictionary<string, string> _cookies;

    public CookieTestHelper(Dictionary<string, string> cookies)
    {
        _cookies = cookies;
    }

    public string? this[string key] => _cookies.TryGetValue(key, out var value) ? value : null;

    public int Count => _cookies.Count;

    public ICollection<string> Keys => _cookies.Keys;

    public bool ContainsKey(string key) => _cookies.ContainsKey(key);

    public IEnumerator<KeyValuePair<string, string>> GetEnumerator() => _cookies.GetEnumerator();

    public bool TryGetValue(string key, out string? value) => _cookies.TryGetValue(key, out value!);

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}