using CO.CDP.UI.Foundation.Pages;
using CO.CDP.UI.Foundation.Services;
using Moq;
using Xunit;

namespace CO.CDP.UI.Foundation.Tests.Pages;

public class ErrorPageTests
{
    [Fact]
    public void Constructor_SetsPropertiesCorrectly()
    {
        var traceId = "trace-123";
        var sirsiUrlService = new Mock<ISirsiUrlService>();
        sirsiUrlService.Setup(s => s.BuildUrl(It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<string?>(), It.IsAny<bool?>()))
            .Returns("https://example.com/provide-feedback-and-contact/");

        var page = new ErrorPage(traceId, sirsiUrlService.Object);

        Assert.Equal("trace-123", page.TraceId);
    }

    [Fact]
    public void Render_IncludesTraceIdAndGovUkMarkup()
    {
        var traceId = "trace-abc";
        var sirsiUrlService = new Mock<ISirsiUrlService>();
        sirsiUrlService.Setup(s => s.BuildUrl(It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<string?>(), It.IsAny<bool?>()))
            .Returns("https://example.com/provide-feedback-and-contact/");
        var page = new ErrorPage(traceId, sirsiUrlService.Object);

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
        var traceId = "trace-xyz";
        var sirsiUrlService = new Mock<ISirsiUrlService>();
        var page = new ErrorPage(traceId, sirsiUrlService.Object);

        var html = page.Render();

        Assert.DoesNotContain("contact the support team", html);
        Assert.DoesNotContain("provide-feedback-and-contact", html);
    }

    [Fact]
    public void Render_WithoutTraceId_DoesNotRenderTraceIdMarkup()
    {
        var traceId = string.Empty;
        var sirsiUrlService = new Mock<ISirsiUrlService>();
        sirsiUrlService.Setup(s => s.BuildUrl(It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<string?>(), It.IsAny<bool?>()))
            .Returns("https://example.com/provide-feedback-and-contact/");
        var page = new ErrorPage(traceId, sirsiUrlService.Object);

        var html = page.Render();

        Assert.DoesNotContain("Trace ID:", html);
    }

    [Fact]
    public void Render_WithNullTraceId_DoesNotRenderTraceIdMarkup()
    {
        string? traceId = null;
        var sirsiUrlService = new Mock<ISirsiUrlService>();
        sirsiUrlService.Setup(s => s.BuildUrl(It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<string?>(), It.IsAny<bool?>()))
            .Returns("https://example.com/provide-feedback-and-contact/");
        var page = new ErrorPage(traceId, sirsiUrlService.Object);

        var html = page.Render();

        Assert.DoesNotContain("Trace ID:", html);
    }
}