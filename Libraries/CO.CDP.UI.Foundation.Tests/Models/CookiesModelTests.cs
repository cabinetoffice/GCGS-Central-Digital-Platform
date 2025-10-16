using CO.CDP.UI.Foundation.Cookies;
using CO.CDP.UI.Foundation.Pages;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using Xunit;

namespace CO.CDP.UI.Foundation.Tests.Models;

public class CookiesModelTests
{
    private readonly Mock<ICookiePreferencesService> _mockCookiePreferencesService;
    private readonly CookiesModel _model;
    private readonly Mock<IRequestCookieCollection> _mockCookieCollection;

    public CookiesModelTests()
    {
        _mockCookiePreferencesService = new Mock<ICookiePreferencesService>();
        _model = new CookiesModel(_mockCookiePreferencesService.Object);

        var mockHttpContext = new Mock<HttpContext>();
        var mockRequest = new Mock<HttpRequest>();
        _mockCookieCollection = new Mock<IRequestCookieCollection>();

        var pageContext = new PageContext
        {
            HttpContext = mockHttpContext.Object
        };
        _model.PageContext = pageContext;

        mockHttpContext.Setup(x => x.Request).Returns(mockRequest.Object);
        mockRequest.Setup(x => x.Cookies).Returns(_mockCookieCollection.Object);
    }

    #region OnGet Tests

    [Fact]
    public void OnGet_WhenNoCookieExists_CookieAcceptanceRemainsNull()
    {
        var cookieSettings = new CookieSettings();
        _mockCookieCollection.Setup(x => x[cookieSettings.CookieName]).Returns((string?)null);

        _model.OnGet();

        _model.CookieAcceptance.Should().BeNull();
    }

    [Fact]
    public void OnGet_WhenCookieExistsWithAcceptValue_SetsCookieAcceptanceToAccept()
    {
        var cookieSettings = new CookieSettings();
        _mockCookieCollection.Setup(x => x[cookieSettings.CookieName]).Returns("Accept");

        _model.OnGet();

        _model.CookieAcceptance.Should().Be(CookieAcceptanceValues.Accept);
    }

    [Fact]
    public void OnGet_WhenCookieExistsWithRejectValue_SetsCookieAcceptanceToReject()
    {
        var cookieSettings = new CookieSettings();
        _mockCookieCollection.Setup(x => x[cookieSettings.CookieName]).Returns("Reject");

        _model.OnGet();

        _model.CookieAcceptance.Should().Be(CookieAcceptanceValues.Reject);
    }

    [Fact]
    public void OnGet_WhenCookieExistsWithUnknownValue_SetsCookieAcceptanceToUnknown()
    {
        var cookieSettings = new CookieSettings();
        _mockCookieCollection.Setup(x => x[cookieSettings.CookieName]).Returns("Unknown");

        _model.OnGet();

        _model.CookieAcceptance.Should().Be(CookieAcceptanceValues.Unknown);
    }

    [Fact]
    public void OnGet_WhenCookieExistsWithInvalidValue_CookieAcceptanceRemainsNull()
    {
        var cookieSettings = new CookieSettings();
        _mockCookieCollection.Setup(x => x[cookieSettings.CookieName]).Returns("InvalidValue");

        _model.OnGet();

        _model.CookieAcceptance.Should().BeNull();
    }

    [Fact]
    public void OnGet_WhenCookieExistsWithEmptyValue_CookieAcceptanceRemainsNull()
    {
        var cookieSettings = new CookieSettings();
        _mockCookieCollection.Setup(x => x[cookieSettings.CookieName]).Returns(string.Empty);

        _model.OnGet();

        _model.CookieAcceptance.Should().BeNull();
    }

    [Fact]
    public void OnGet_WhenCookieExistsWithWhitespaceValue_CookieAcceptanceRemainsNull()
    {
        var cookieSettings = new CookieSettings();
        _mockCookieCollection.Setup(x => x[cookieSettings.CookieName]).Returns("   ");

        _model.OnGet();

        _model.CookieAcceptance.Should().BeNull();
    }

    #endregion

    #region OnPost Tests

    [Fact]
    public void OnPost_WhenModelStateIsInvalid_ReturnsPageResult()
    {
        _model.ModelState.AddModelError("CookieAcceptance", "Required");
        _model.CookieAcceptance = CookieAcceptanceValues.Accept;

        var result = _model.OnPost();

        result.Should().BeOfType<PageResult>();
        _mockCookiePreferencesService.Verify(x => x.Accept(), Times.Never);
        _mockCookiePreferencesService.Verify(x => x.Reject(), Times.Never);
    }

    [Fact]
    public void OnPost_WhenCookieAcceptanceIsNull_ReturnsPageResult()
    {
        _model.CookieAcceptance = null;

        var result = _model.OnPost();

        result.Should().BeOfType<PageResult>();
        _mockCookiePreferencesService.Verify(x => x.Accept(), Times.Never);
        _mockCookiePreferencesService.Verify(x => x.Reject(), Times.Never);
    }

    [Fact]
    public void OnPost_WhenCookieAcceptanceIsAccept_CallsAcceptAndRedirectsToRoot()
    {
        _model.CookieAcceptance = CookieAcceptanceValues.Accept;

        var result = _model.OnPost();

        _mockCookiePreferencesService.Verify(x => x.Accept(), Times.Once);
        _mockCookiePreferencesService.Verify(x => x.Reject(), Times.Never);

        var redirectResult = result.Should().BeOfType<RedirectResult>().Subject;
        redirectResult.Url.Should().Be("/");
    }

    [Fact]
    public void OnPost_WhenCookieAcceptanceIsReject_CallsRejectAndRedirectsToRoot()
    {
        _model.CookieAcceptance = CookieAcceptanceValues.Reject;

        var result = _model.OnPost();

        _mockCookiePreferencesService.Verify(x => x.Reject(), Times.Once);
        _mockCookiePreferencesService.Verify(x => x.Accept(), Times.Never);

        var redirectResult = result.Should().BeOfType<RedirectResult>().Subject;
        redirectResult.Url.Should().Be("/");
    }

    [Fact]
    public void OnPost_WhenCookieAcceptanceIsUnknown_DoesNotCallServiceAndRedirectsToRoot()
    {
        _model.CookieAcceptance = CookieAcceptanceValues.Unknown;

        var result = _model.OnPost();

        _mockCookiePreferencesService.Verify(x => x.Accept(), Times.Never);
        _mockCookiePreferencesService.Verify(x => x.Reject(), Times.Never);

        var redirectResult = result.Should().BeOfType<RedirectResult>().Subject;
        redirectResult.Url.Should().Be("/");
    }

    [Fact]
    public void OnPost_WhenModelStateIsValidAndCookieAcceptanceIsAccept_CallsAcceptOnce()
    {
        _model.CookieAcceptance = CookieAcceptanceValues.Accept;

        _model.OnPost();

        _mockCookiePreferencesService.Verify(x => x.Accept(), Times.Once);
    }

    [Fact]
    public void OnPost_WhenModelStateIsValidAndCookieAcceptanceIsReject_CallsRejectOnce()
    {
        _model.CookieAcceptance = CookieAcceptanceValues.Reject;

        _model.OnPost();

        _mockCookiePreferencesService.Verify(x => x.Reject(), Times.Once);
    }

    #endregion
}