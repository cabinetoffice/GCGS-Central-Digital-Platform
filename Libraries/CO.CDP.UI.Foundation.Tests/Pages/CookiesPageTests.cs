using CO.CDP.UI.Foundation.Cookies;
using CO.CDP.UI.Foundation.Pages;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Moq;
using Xunit;

namespace CO.CDP.UI.Foundation.Tests.Pages;

public class CookiesPageTests
{
    private readonly Mock<ICookiePreferencesService> _mockCookiePreferencesService;
    private readonly CookiesModel _model;
    private readonly Mock<IRequestCookieCollection> _mockCookieCollection;

    public CookiesPageTests()
    {
        _mockCookiePreferencesService = new Mock<ICookiePreferencesService>();
        _model = new CookiesModel(_mockCookiePreferencesService.Object);

        var mockHttpContext = new Mock<HttpContext>();
        var mockRequest = new Mock<HttpRequest>();
        _mockCookieCollection = new Mock<IRequestCookieCollection>();

        var pageContext = new PageContext
        {
            HttpContext = mockHttpContext.Object,
            RouteData = new RouteData(),
            ActionDescriptor = new CompiledPageActionDescriptor(),
            ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
        };
        _model.PageContext = pageContext;

        mockHttpContext.Setup(x => x.Request).Returns(mockRequest.Object);
        mockRequest.Setup(x => x.Cookies).Returns(_mockCookieCollection.Object);
    }

    #region Page Model Tests

    [Fact]
    public void CookieAcceptance_Property_ShouldBeNullableEnum()
    {
        var property = typeof(CookiesModel).GetProperty(nameof(CookiesModel.CookieAcceptance));

        property.Should().NotBeNull();
        property!.PropertyType.Should().Be(typeof(CookieAcceptanceValues?));
    }

    #endregion

    #region OnGet Page Behavior Tests

    [Fact]
    public void OnGet_WhenCookieIsAccepted_ShouldSetCookieAcceptanceToAccept()
    {
        var cookieSettings = new CookieSettings();
        _mockCookieCollection.Setup(x => x[cookieSettings.CookieName]).Returns("Accept");

        _model.OnGet();

        _model.CookieAcceptance.Should().Be(CookieAcceptanceValues.Accept);
    }

    [Fact]
    public void OnGet_WhenCookieIsRejected_ShouldSetCookieAcceptanceToReject()
    {
        var cookieSettings = new CookieSettings();
        _mockCookieCollection.Setup(x => x[cookieSettings.CookieName]).Returns("Reject");

        _model.OnGet();

        _model.CookieAcceptance.Should().Be(CookieAcceptanceValues.Reject);
    }

    [Fact]
    public void OnGet_WhenNoCookieSet_ShouldLeaveCookieAcceptanceNull()
    {
        var cookieSettings = new CookieSettings();
        _mockCookieCollection.Setup(x => x[cookieSettings.CookieName]).Returns((string?)null);

        _model.OnGet();

        _model.CookieAcceptance.Should().BeNull();
    }

    #endregion

    #region OnPost Form Submission Tests

    [Fact]
    public void OnPost_WhenAcceptingCookies_ShouldCallAcceptService()
    {
        _model.CookieAcceptance = CookieAcceptanceValues.Accept;

        var result = _model.OnPost();

        _mockCookiePreferencesService.Verify(x => x.Accept(), Times.Once);
        result.Should().BeOfType<RedirectResult>();
        ((RedirectResult)result).Url.Should().Be("/");
    }

    [Fact]
    public void OnPost_WhenRejectingCookies_ShouldCallRejectService()
    {
        _model.CookieAcceptance = CookieAcceptanceValues.Reject;

        var result = _model.OnPost();

        _mockCookiePreferencesService.Verify(x => x.Reject(), Times.Once);
        result.Should().BeOfType<RedirectResult>();
        ((RedirectResult)result).Url.Should().Be("/");
    }

    [Fact]
    public void OnPost_WhenNoSelectionMade_ShouldReturnPageWithoutCallingService()
    {
        _model.CookieAcceptance = null;

        var result = _model.OnPost();

        _mockCookiePreferencesService.Verify(x => x.Accept(), Times.Never);
        _mockCookiePreferencesService.Verify(x => x.Reject(), Times.Never);
        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public void OnPost_WhenModelStateIsInvalid_ShouldReturnPageWithoutCallingService()
    {
        _model.CookieAcceptance = CookieAcceptanceValues.Accept;
        _model.ModelState.AddModelError("CookieAcceptance", "Test error");

        var result = _model.OnPost();

        _mockCookiePreferencesService.Verify(x => x.Accept(), Times.Never);
        _mockCookiePreferencesService.Verify(x => x.Reject(), Times.Never);
        result.Should().BeOfType<PageResult>();
    }

    #endregion

    #region Form Validation Tests

    [Fact]
    public void CookieAcceptance_WhenNull_ShouldBeValidForDisplay()
    {
        _model.CookieAcceptance = null;

        _model.CookieAcceptance.Should().BeNull();
    }

    [Fact]
    public void CookieAcceptance_WhenSetToAccept_ShouldRetainValue()
    {
        _model.CookieAcceptance = CookieAcceptanceValues.Accept;

        _model.CookieAcceptance.Should().Be(CookieAcceptanceValues.Accept);
    }

    [Fact]
    public void CookieAcceptance_WhenSetToReject_ShouldRetainValue()
    {
        _model.CookieAcceptance = CookieAcceptanceValues.Reject;

        _model.CookieAcceptance.Should().Be(CookieAcceptanceValues.Reject);
    }

    [Fact]
    public void CookieAcceptance_WhenSetToUnknown_ShouldRetainValue()
    {
        _model.CookieAcceptance = CookieAcceptanceValues.Unknown;

        _model.CookieAcceptance.Should().Be(CookieAcceptanceValues.Unknown);
    }

    #endregion

    #region Cookie Service Integration Tests

    [Fact]
    public void OnPost_WithAcceptValue_ShouldOnlyCallAcceptService()
    {
        _model.CookieAcceptance = CookieAcceptanceValues.Accept;

        _model.OnPost();

        _mockCookiePreferencesService.Verify(x => x.Accept(), Times.Once);
        _mockCookiePreferencesService.Verify(x => x.Reject(), Times.Never);
        _mockCookiePreferencesService.VerifyNoOtherCalls();
    }

    [Fact]
    public void OnPost_WithRejectValue_ShouldOnlyCallRejectService()
    {
        _model.CookieAcceptance = CookieAcceptanceValues.Reject;

        _model.OnPost();

        _mockCookiePreferencesService.Verify(x => x.Reject(), Times.Once);
        _mockCookiePreferencesService.Verify(x => x.Accept(), Times.Never);
        _mockCookiePreferencesService.VerifyNoOtherCalls();
    }

    [Fact]
    public void OnPost_WithUnknownValue_ShouldNotCallAnyService()
    {
        _model.CookieAcceptance = CookieAcceptanceValues.Unknown;

        var result = _model.OnPost();

        _mockCookiePreferencesService.Verify(x => x.Accept(), Times.Never);
        _mockCookiePreferencesService.Verify(x => x.Reject(), Times.Never);
        _mockCookiePreferencesService.VerifyNoOtherCalls();
        result.Should().BeOfType<RedirectResult>();
    }

    #endregion
}
