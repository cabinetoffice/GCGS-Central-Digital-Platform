using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Moq;
using Xunit;
using FluentAssertions;

namespace CO.CDP.OrganisationApp.Tests;

public class CustomQueryStringCultureProviderTests
{
    [Fact]
    public async Task DetermineProviderCultureResult_SetsCyCultureAndCookie_WhenLanguageQueryStringIsCy()
    {
        var provider = new CustomQueryStringCultureProvider();
        var (httpContext, cookies) = GivenHttpContext("cy");

        var result = await provider.DetermineProviderCultureResult(httpContext.Object);

        result.Should().NotBeNull();
        result!.Cultures.First().Value.Should().Be("cy");

        cookies.Verify(c => c.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            "c=cy|uic=cy",
            It.IsAny<CookieOptions>()), Times.Once);
    }

    [Fact]
    public async Task DetermineProviderCultureResult_SetsEnCultureAndCookie_WhenLanguageQueryStringIsEn()
    {
        var provider = new CustomQueryStringCultureProvider();
        var (httpContext, cookies) = GivenHttpContext("en");

        var result = await provider.DetermineProviderCultureResult(httpContext.Object);

        result.Should().NotBeNull();
        result!.Cultures.First().Value.Should().Be("en-GB");

        cookies.Verify(c => c.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            "c=en-GB|uic=en-GB",
            It.IsAny<CookieOptions>()), Times.Once);
    }

    [Fact]
    public async Task DetermineProviderCultureResult_SetsEnCultureAndCookie_WhenLanguageQueryStringIsEnGB()
    {
        var provider = new CustomQueryStringCultureProvider();
        var (httpContext, cookies) = GivenHttpContext("en_GB"); // The format we're expecting from FTS, with an underscore instead of the more standard dash

        var result = await provider.DetermineProviderCultureResult(httpContext.Object);

        result.Should().NotBeNull();
        result!.Cultures.First().Value.Should().Be("en-GB");

        cookies.Verify(c => c.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            "c=en-GB|uic=en-GB",
            It.IsAny<CookieOptions>()), Times.Once);
    }

    [Fact]
    public async Task DetermineProviderCultureResult_ReturnsNullAndDoesNotSetCookie_WhenLanguageQueryStringIsMissing()
    {
        var provider = new CustomQueryStringCultureProvider();
        var (httpContext, cookies) = GivenHttpContext(null);

        var result = await provider.DetermineProviderCultureResult(httpContext.Object);

        result.Should().BeNull();
        cookies.Verify(c => c.Append(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CookieOptions>()), Times.Never);
    }

    [Fact]
    public async Task DetermineProviderCultureResult_SetsDefaultCultureAndCookie_WhenLanguageQueryStringIsInvalid()
    {
        var provider = new CustomQueryStringCultureProvider();
        var (httpContext, cookies) = GivenHttpContext("invalid");

        var result = await provider.DetermineProviderCultureResult(httpContext.Object);

        result.Should().NotBeNull();
        result!.Cultures.First().Value.Should().Be("en-GB");

        cookies.Verify(c => c.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            "c=en-GB|uic=en-GB",
            It.IsAny<CookieOptions>()), Times.Once);
    }

    private (Mock<HttpContext>, Mock<IResponseCookies>) GivenHttpContext(string? language)
    {
        var httpContext = new Mock<HttpContext>();
        var request = new Mock<HttpRequest>();
        var response = new Mock<HttpResponse>();
        var cookies = new Mock<IResponseCookies>();

        var queryCollection = new QueryCollection(language != null
            ? new Dictionary<string, Microsoft.Extensions.Primitives.StringValues> { { "language", language } }
            : new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>());

        request.Setup(r => r.Query).Returns(queryCollection);
        request.Setup(r => r.IsHttps).Returns(true);
        response.Setup(r => r.Cookies).Returns(cookies.Object);

        httpContext.Setup(c => c.Request).Returns(request.Object);
        httpContext.Setup(c => c.Response).Returns(response.Object);

        return (httpContext, cookies);
    }
}
