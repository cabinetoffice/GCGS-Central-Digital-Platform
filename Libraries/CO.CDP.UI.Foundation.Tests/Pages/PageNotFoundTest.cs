using CO.CDP.UI.Foundation.Pages;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Xunit;

namespace CO.CDP.UI.Foundation.Tests.Pages;

public class PageNotFoundTest
{
    [Fact]
    public void OnGet_ReturnsPageResultAndSetsStatusCode()
    {
        var model = new PageNotFoundModel();
        var httpContext = new DefaultHttpContext();
        model.PageContext = new PageContext
        {
            HttpContext = httpContext
        };

        var result = model.OnGet();

        result.Should().BeOfType<PageResult>();
        httpContext.Response.StatusCode.Should().Be(404);
    }
}

