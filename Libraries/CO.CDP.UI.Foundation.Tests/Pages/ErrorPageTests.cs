using CO.CDP.UI.Foundation.Pages;
using CO.CDP.UI.Foundation.Services;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace CO.CDP.UI.Foundation.Tests.Pages;

public class ErrorPageTests
{
    [Fact]
    public void Constructor_SetsPropertiesCorrectly()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.TraceIdentifier = "trace-123";
        var sirsiUrlService = new Mock<ISirsiUrlService>();
        sirsiUrlService.Setup(s => s.BuildUrl(It.IsAny<string>(), null, null))
            .Returns("https://example.com/provide-feedback-and-contact/");

        var page = new ErrorPage(httpContext, sirsiUrlService.Object, 501);

        Assert.Equal(501, page.StatusCode);
        Assert.Equal("trace-123", page.TraceId);
    }

    [Fact]
    public void Render_IncludesTraceIdAndGovUkMarkup()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.TraceIdentifier = "trace-abc";
        var sirsiUrlService = new Mock<ISirsiUrlService>();
        sirsiUrlService.Setup(s => s.BuildUrl(It.IsAny<string>(), null, null))
            .Returns("https://example.com/provide-feedback-and-contact/");
        var page = new ErrorPage(httpContext, sirsiUrlService.Object, 500);

        var html = page.Render();

        Assert.Contains("govuk-width-container", html);
        Assert.Contains("govuk-main-wrapper", html);
        Assert.Contains("trace-abc", html);
        Assert.Contains("contact the support team", html);
        Assert.Contains("https://example.com/provide-feedback-and-contact/", html);
    }

    [Fact]
    public void Render_WithoutFeedbackUrl_DoesNotRenderFeedbackLink()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.TraceIdentifier = "trace-xyz";
        var sirsiUrlService = new Mock<ISirsiUrlService>();
        sirsiUrlService.Setup(s => s.BuildUrl(It.IsAny<string>(), null, null)).Returns(string.Empty);
        var page = new ErrorPage(httpContext, sirsiUrlService.Object, 500);

        var html = page.Render();

        Assert.DoesNotContain("contact the support team", html);
        Assert.DoesNotContain("provide-feedback-and-contact", html);
    }
}