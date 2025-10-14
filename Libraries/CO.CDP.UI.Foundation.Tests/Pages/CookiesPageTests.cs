using CO.CDP.Localization;
using CO.CDP.UI.Foundation.Cookies;
using CO.CDP.UI.Foundation.Pages;
using CO.CDP.UI.Foundation.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using Xunit;

namespace CO.CDP.UI.Foundation.Tests.Pages;

public class CookiesPageTests
{
    private readonly Mock<IFlashMessageService> _mockFlashMessageService;
    private readonly Mock<ICookiePreferencesService> _mockCookiePreferencesService;
    private readonly Mock<IUrlHelper> _mockUrlHelper;
    private readonly CookiesModel _model;
    private readonly Dictionary<string, string> _requestCookies;

    public CookiesPageTests()
    {
        _mockFlashMessageService = new Mock<IFlashMessageService>();
        _mockCookiePreferencesService = new Mock<ICookiePreferencesService>();
        _mockUrlHelper = new Mock<IUrlHelper>();
        _requestCookies = new Dictionary<string, string>();

        var mockRequest = new Mock<HttpRequest>();
        var mockHttpContext = new Mock<HttpContext>();

        mockHttpContext.Setup(c => c.Request).Returns(mockRequest.Object);
        mockRequest.Setup(r => r.Cookies).Returns(new TestRequestCookieCollection(_requestCookies));

        _model = new CookiesModel(_mockFlashMessageService.Object, _mockCookiePreferencesService.Object)
        {
            PageContext = new PageContext
            {
                HttpContext = mockHttpContext.Object,
                ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
            },
            Url = _mockUrlHelper.Object
        };
    }

    #region Page Structure and Attributes Tests

    [Fact]
    public void Page_HasAllowAnonymousAttribute()
    {
        var allowAnonymousAttribute = typeof(CookiesModel).GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute), false);
        Assert.NotEmpty(allowAnonymousAttribute);
    }

    [Fact]
    public void CookieAcceptance_HasRequiredAttribute()
    {
        var property = typeof(CookiesModel).GetProperty(nameof(CookiesModel.CookieAcceptance));
        Assert.NotNull(property);
        var requiredAttribute = property.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.RequiredAttribute), false);
        Assert.NotEmpty(requiredAttribute);
    }

    [Fact]
    public void CookieAcceptance_HasBindPropertyAttribute()
    {
        var property = typeof(CookiesModel).GetProperty(nameof(CookiesModel.CookieAcceptance));
        Assert.NotNull(property);
        var bindPropertyAttribute = property.GetCustomAttributes(typeof(BindPropertyAttribute), false);
        Assert.NotEmpty(bindPropertyAttribute);
    }

    [Fact]
    public void ReturnUrl_HasBindPropertyAttribute()
    {
        var property = typeof(CookiesModel).GetProperty(nameof(CookiesModel.ReturnUrl));
        Assert.NotNull(property);
        var bindPropertyAttribute = property.GetCustomAttributes(typeof(BindPropertyAttribute), false);
        Assert.NotEmpty(bindPropertyAttribute);
    }

    [Fact]
    public void Properties_HaveCorrectTypes()
    {
        var cookieAcceptanceProperty = typeof(CookiesModel).GetProperty(nameof(CookiesModel.CookieAcceptance));
        var returnUrlProperty = typeof(CookiesModel).GetProperty(nameof(CookiesModel.ReturnUrl));

        Assert.NotNull(cookieAcceptanceProperty);
        Assert.NotNull(returnUrlProperty);
        Assert.Equal(typeof(CookieAcceptanceValues?), cookieAcceptanceProperty.PropertyType);
        Assert.Equal(typeof(string), returnUrlProperty.PropertyType);
    }

    #endregion

    #region OnGet Method Tests

    [Fact]
    public void OnGet_WithoutReturnUrl_SetsReturnUrlToNull()
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

    [Fact]
    public void OnGet_WithAcceptedCookies_SetsCookieAcceptanceToAccept()
    {
        var cookieSettings = new CookieSettings();
        _requestCookies[cookieSettings.CookieName] = CookieAcceptanceValues.Accept.ToString();

        _model.OnGet();

        Assert.Equal(CookieAcceptanceValues.Accept, _model.CookieAcceptance);
    }

    [Fact]
    public void OnGet_WithRejectedCookies_SetsCookieAcceptanceToReject()
    {
        var cookieSettings = new CookieSettings();
        _requestCookies[cookieSettings.CookieName] = CookieAcceptanceValues.Reject.ToString();

        _model.OnGet();

        Assert.Equal(CookieAcceptanceValues.Reject, _model.CookieAcceptance);
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

    [Theory]
    [InlineData("Accept")]
    [InlineData("Reject")]
    [InlineData("Unknown")]
    public void OnGet_WithDifferentCookieValues_ParsesCorrectly(string cookieValue)
    {
        var cookieSettings = new CookieSettings();
        _requestCookies[cookieSettings.CookieName] = cookieValue;

        _model.OnGet();

        if (System.Enum.TryParse<CookieAcceptanceValues>(cookieValue, out var expectedValue))
        {
            Assert.Equal(expectedValue, _model.CookieAcceptance);
        }
        else
        {
            Assert.Null(_model.CookieAcceptance);
        }
    }

    [Fact]
    public void OnGet_WithComplexReturnUrl_PreservesUrl()
    {
        var complexReturnUrl = "/search?query=test&page=2&filter=active";

        _model.OnGet(complexReturnUrl);

        Assert.Equal(complexReturnUrl, _model.ReturnUrl);
    }

    #endregion

    #region OnPost Method Tests

    [Fact]
    public void OnPost_WithValidAcceptance_CallsCorrectServiceMethod()
    {
        _model.CookieAcceptance = CookieAcceptanceValues.Accept;

        _model.OnPost();

        _mockCookiePreferencesService.Verify(s => s.Accept(), Times.Once);
        _mockCookiePreferencesService.Verify(s => s.Reject(), Times.Never);
    }

    [Fact]
    public void OnPost_WithValidRejection_CallsCorrectServiceMethod()
    {
        _model.CookieAcceptance = CookieAcceptanceValues.Reject;

        _model.OnPost();

        _mockCookiePreferencesService.Verify(s => s.Reject(), Times.Once);
        _mockCookiePreferencesService.Verify(s => s.Accept(), Times.Never);
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
    public void OnPost_WithModelValidationError_ReturnsPageResult()
    {
        _model.ModelState.AddModelError(nameof(_model.CookieAcceptance), "This field is required");

        var result = _model.OnPost();

        Assert.IsType<PageResult>(result);
        _mockCookiePreferencesService.Verify(s => s.Accept(), Times.Never);
        _mockCookiePreferencesService.Verify(s => s.Reject(), Times.Never);
    }

    [Fact]
    public void OnPost_WithNullCookieAcceptance_ReturnsPageResult()
    {
        _model.CookieAcceptance = null;

        var result = _model.OnPost();

        Assert.IsType<PageResult>(result);
        _mockCookiePreferencesService.Verify(s => s.Accept(), Times.Never);
        _mockCookiePreferencesService.Verify(s => s.Reject(), Times.Never);
    }

    #endregion

    #region URL Redirection Tests

    [Fact]
    public void OnPost_WithLocalReturnUrl_RedirectsToReturnUrlWithQueryString()
    {
        _model.CookieAcceptance = CookieAcceptanceValues.Accept;
        _model.ReturnUrl = "/test-page";
        _mockUrlHelper.Setup(u => u.IsLocalUrl("/test-page")).Returns(true);

        var result = _model.OnPost();

        var redirectResult = Assert.IsType<LocalRedirectResult>(result);
        Assert.Contains("/test-page", redirectResult.Url);

        var cookieSettings = new CookieSettings();
        Assert.Contains($"{cookieSettings.CookieBannerInteractionQueryString}=true", redirectResult.Url);
    }

    [Fact]
    public void OnPost_WithNonLocalReturnUrl_DoesNotRedirectToReturnUrl()
    {
        _model.CookieAcceptance = CookieAcceptanceValues.Accept;
        _model.ReturnUrl = "https://external-site.com";
        _mockUrlHelper.Setup(u => u.IsLocalUrl("https://external-site.com")).Returns(false);

        var result = _model.OnPost();

        var redirectResult = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/Cookies", redirectResult.PageName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void OnPost_WithEmptyOrNullReturnUrl_RedirectsToCookiesPage(string? returnUrl)
    {
        _model.CookieAcceptance = CookieAcceptanceValues.Accept;
        _model.ReturnUrl = returnUrl;

        var result = _model.OnPost();

        var redirectResult = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/Cookies", redirectResult.PageName);
    }

    [Fact]
    public void OnPost_WithComplexReturnUrl_PreservesOriginalQueryParameters()
    {
        _model.CookieAcceptance = CookieAcceptanceValues.Accept;
        _model.ReturnUrl = "/search?query=test&page=2";
        _mockUrlHelper.Setup(u => u.IsLocalUrl("/search?query=test&page=2")).Returns(true);

        var result = _model.OnPost();

        var redirectResult = Assert.IsType<LocalRedirectResult>(result);
        Assert.Contains("query=test", redirectResult.Url);
        Assert.Contains("page=2", redirectResult.Url);
        Assert.Contains("cookieBannerInteraction=true", redirectResult.Url);
    }

    #endregion

    #region Flash Message Tests

    [Fact]
    public void OnPost_WithoutReturnUrl_SetsFlashMessageAndRedirectsToCookiesPage()
    {
        _model.CookieAcceptance = CookieAcceptanceValues.Accept;

        var result = _model.OnPost();

        _mockFlashMessageService.Verify(
            s => s.SetFlashMessage(
                FlashMessageType.Success,
                StaticTextResource.Cookies_SetCookiePreferences,
                null,
                null,
                null,
                null),
            Times.Once);

        var redirectResult = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/Cookies", redirectResult.PageName);
    }

    [Fact]
    public void OnPost_WithValidReturnUrl_DoesNotSetFlashMessage()
    {
        _model.CookieAcceptance = CookieAcceptanceValues.Accept;
        _model.ReturnUrl = "/test-page";
        _mockUrlHelper.Setup(u => u.IsLocalUrl("/test-page")).Returns(true);

        _model.OnPost();

        _mockFlashMessageService.Verify(
            s => s.SetFlashMessage(It.IsAny<FlashMessageType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<Dictionary<string, string>>()),
            Times.Never);
    }

    #endregion

    #region Configuration Tests

    [Fact]
    public void CookieSettings_HasCorrectDefaultValues()
    {
        var cookieSettings = new CookieSettings();

        Assert.Equal("CookieAcceptance", cookieSettings.CookieAcceptanceFieldName);
        Assert.Equal("ReturnUrl", cookieSettings.CookieSettingsPageReturnUrlFieldName);
        Assert.Equal("cookieBannerInteraction", cookieSettings.CookieBannerInteractionQueryString);
        Assert.Equal("SIRSI_COOKIES_PREFERENCES_SET", cookieSettings.CookieName);
        Assert.Equal("cookies_accepted", cookieSettings.FtsHandoverParameter);
        Assert.Equal(365, cookieSettings.ExpiryDays);
    }

    #endregion
}

internal class TestRequestCookieCollection : IRequestCookieCollection
{
    private readonly Dictionary<string, string> _cookies;

    public TestRequestCookieCollection(Dictionary<string, string> cookies)
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
