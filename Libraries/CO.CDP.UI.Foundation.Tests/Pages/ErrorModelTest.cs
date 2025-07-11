using CO.CDP.UI.Foundation.Pages;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace CO.CDP.UI.Foundation.Tests.Pages;

public class ErrorModelTest
{
    private readonly IConfiguration _configuration;

    public ErrorModelTest()
    {
        var config = new Dictionary<string, string?>
        {
            { "OrganisationAppUrl", "http://localhost:8090/" }
        };
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(config)
            .Build();
    }

    [Fact]
    public void OnGet_ReturnsPageResultAndSetsTraceId()
    {
        var httpContext = new DefaultHttpContext();
        var model = new ErrorModel(new ConfigurationBuilder().Build())
        {
            TraceId = string.Empty,
            PageContext = new PageContext
            {
                HttpContext = httpContext
            }
        };

        var result = model.OnGet();

        result.Should().BeOfType<PageResult>();
        model.TraceId.Should().Be(httpContext.TraceIdentifier);
        model.StatusCode.Should().Be(500);
        model.PageContext.HttpContext.Response.StatusCode.Should().Be(500);
        model.FeedbackUrl.Should().BeNull();
    }

    [Fact]
    public void OnGet_WithStatusCode_ReturnsPageResultAndSetsTraceIdAndStatusCode()
    {
        var httpContext = new DefaultHttpContext();
        var model = new ErrorModel(new ConfigurationBuilder().Build())
        {
            TraceId = string.Empty,
            PageContext = new PageContext
            {
                HttpContext = httpContext
            }
        };

        var result = model.OnGet(404);

        result.Should().BeOfType<PageResult>();
        model.TraceId.Should().Be(httpContext.TraceIdentifier);
        model.StatusCode.Should().Be(404);
        model.PageContext.HttpContext.Response.StatusCode.Should().Be(404);
        model.FeedbackUrl.Should().BeNull();
    }

    [Fact]
    public void OnGet_WithFeedbackUrl_SetsFeedbackUrlOnModel()
    {
        var httpContext = new DefaultHttpContext();
        var model = new ErrorModel(_configuration)
        {
            TraceId = string.Empty,
            PageContext = new PageContext
            {
                HttpContext = httpContext
            }
        };

        model.OnGet();

        model.FeedbackUrl.Should().Be("http://localhost:8090/provide-feedback-and-contact/");
    }
}