using CO.CDP.OrganisationApp.Pages;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages;

public class ChangeLanguageTests
{
    private readonly ChangeLanguageModel _model;
    private readonly Mock<ITempDataService> _mockTempDataService;
    private readonly Mock<HttpContext> _mockHttpContext;
    private readonly Mock<HttpResponse> _mockResponse;
    private readonly Mock<HttpRequest> _mockRequest;
    private readonly Mock<IResponseCookies> _mockCookies;
    private readonly Mock<IUrlHelper> _mockUrlHelper;

    public ChangeLanguageTests()
    {
        _mockTempDataService = new Mock<ITempDataService>();
        _model = new ChangeLanguageModel(_mockTempDataService.Object);
        _mockHttpContext = new Mock<HttpContext>();
        _mockResponse = new Mock<HttpResponse>();
        _mockRequest = new Mock<HttpRequest>();
        _mockCookies = new Mock<IResponseCookies>();
        _mockUrlHelper = new Mock<IUrlHelper>();

        _mockResponse.Setup(r => r.Cookies).Returns(_mockCookies.Object);
        _mockHttpContext.Setup(c => c.Response).Returns(_mockResponse.Object);
        _mockHttpContext.Setup(c => c.Request).Returns(_mockRequest.Object);
        _model.PageContext.HttpContext = _mockHttpContext.Object;
        _model.Url = _mockUrlHelper.Object;
    }

    [Fact]
    public void OnGet_ClearsFormSectionTempData()
    {
        _mockTempDataService.Setup(c => c.Keys).Returns(["Form_Whatever_Questions"]);
        _model.OnGet("en", "/foo");

        _mockTempDataService.Verify(c => c.Remove("Form_Whatever_Questions"), Times.Once);
   }

    [Fact]
    public void OnGet_SetsCookie_WhenLanguageIsEnglish()
    {
        var cookieValue = CookieRequestCultureProvider.MakeCookieValue(new RequestCulture("en"));

        _model.OnGet("en", "/foo");

        _mockCookies.Verify(c => c.Append(It.IsAny<string>(), cookieValue, It.IsAny<CookieOptions>()), Times.Once);
    }

    [Fact]
    public void OnGet_SetsCookie_WhenLanguageIsWelsh()
    {
        var cookieValue = CookieRequestCultureProvider.MakeCookieValue(new RequestCulture("cy"));

        _model.OnGet("cy", "/foo");

        _mockCookies.Verify(c => c.Append(It.IsAny<string>(), cookieValue, It.IsAny<CookieOptions>()), Times.Once);
    }

    [Fact]
    public void OnGet_Redirects_WhenRedirectUrlIsLocal()
    {
        var cookieValue = CookieRequestCultureProvider.MakeCookieValue(new RequestCulture("cy"));

        _mockUrlHelper.Setup(u => u.IsLocalUrl(It.IsAny<string>())).Returns(true);

        var response = _model.OnGet("cy", "/foo");

        response.Should().BeOfType<LocalRedirectResult>();

        var localRedirectResult = response as LocalRedirectResult;
        localRedirectResult!.Url.Should().Be("/foo");
    }

    [Fact]
    public void OnGet_Redirects_WhenRedirectUrlIsRemote()
    {
        var cookieValue = CookieRequestCultureProvider.MakeCookieValue(new RequestCulture("cy"));

        var response = _model.OnGet("cy", "http://whatever.com");

        response.Should().BeOfType<RedirectResult>();

        var redirectResult = response as RedirectResult;
        redirectResult!.Url.Should().Be("/");
    }
}