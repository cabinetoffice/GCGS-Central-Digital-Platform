using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Moq;
using static CO.CDP.OrganisationApp.Tests.TagHelpers.TagHelperTestKit;

namespace CO.CDP.OrganisationApp.Tests.TagHelpers;

public class ContentSecurityPolicyNonceTagHelperTests
{
    [Fact]
    public async Task ContentSecurityPolicyNonceTagHelper_ShouldAddNonceAttribute_WhenNonceExistsInHttpContext()
    {
        const string expectedNonce = "test-nonce";
        var httpContext = new DefaultHttpContext();
        httpContext.Items["ContentSecurityPolicyNonce"] = expectedNonce;

        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        httpContextAccessorMock.Setup(accessor => accessor.HttpContext).Returns(httpContext);

        var tagHelper = new ContentSecurityPolicyNonceTagHelper(httpContextAccessorMock.Object);

        var result = await CallTagHelper(
            "script",
            "console.log('test');",
            new TagHelperAttributeList { { "nonce-csp", null } },
            tagHelper);

        result.Should()
            .Contain($"nonce=\"{expectedNonce}\"")
            .And.NotContain("nonce-csp");
    }

    [Fact]
    public async Task ContentSecurityPolicyNonceTagHelper_ShouldNotAddNonceAttribute_WhenNonceDoesNotExistInHttpContext()
    {
        var httpContext = new DefaultHttpContext();

        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        httpContextAccessorMock.Setup(accessor => accessor.HttpContext).Returns(httpContext);

        var tagHelper = new ContentSecurityPolicyNonceTagHelper(httpContextAccessorMock.Object);

        var result = await CallTagHelper(
            "script",
            "console.log('test');",
            new TagHelperAttributeList { { "nonce-csp", null } },
            tagHelper);

        result.Should()
            .NotContain("nonce=")
            .And.NotContain("nonce-csp");
    }

    [Fact]
    public async Task ContentSecurityPolicyNonceTagHelper_ShouldHandleNullHttpContextGracefully()
    {
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        httpContextAccessorMock.Setup(accessor => accessor.HttpContext).Returns((HttpContext)null!);

        var tagHelper = new ContentSecurityPolicyNonceTagHelper(httpContextAccessorMock.Object);

        var result = await CallTagHelper(
            "script",
            "console.log('test');",
            new TagHelperAttributeList { { "nonce-csp", null } },
            tagHelper);

        result.Should()
            .NotContain("nonce=")
            .And.NotContain("nonce-csp");
    }
}
