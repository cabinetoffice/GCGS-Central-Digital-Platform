using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using FluentAssertions;

namespace CO.CDP.OrganisationApp.Tests;

public class ContentSecurityPolicyMiddlewareTests
{
    [Fact]
    public async Task Middleware_ShouldAddNonceAndCspHeader_WhenFeatureIsEnabled()
    {
        var context = GivenHttpContext();
        var nextDelegate = new RequestDelegate(ctx => Task.CompletedTask);

        var middleware = new ContentSecurityPolicyMiddleware(nextDelegate);

        await middleware.InvokeAsync(context, GivenConfiguration(isFeatureEnabled: true));

        var cspHeader = context.Response.Headers["Content-Security-Policy"].ToString();
        cspHeader.Should().NotBeNullOrEmpty("CSP header should be set when the feature is enabled.");

        var nonce = context.Items["ContentSecurityPolicyNonce"] as string;
        nonce.Should().NotBeNullOrEmpty("Nonce should be generated and added to HttpContext.Items.");

        cspHeader.Should().Contain($"'nonce-{nonce}'", "Nonce should be included in the CSP header.");
    }

    [Fact]
    public async Task Middleware_ShouldNotAddCspHeaderOrNonce_WhenFeatureIsDisabled()
    {
        var context = GivenHttpContext();
        var nextDelegate = new RequestDelegate(ctx => Task.CompletedTask);

        var middleware = new ContentSecurityPolicyMiddleware(nextDelegate);

        await middleware.InvokeAsync(context, GivenConfiguration(isFeatureEnabled: false));

        context.Response.Headers["Content-Security-Policy"].Should().BeNullOrEmpty("CSP header should not be set when the feature is disabled.");
        context.Items["ContentSecurityPolicyNonce"].Should().BeNull("Nonce should not be generated when the feature is disabled.");
    }

    [Fact]
    public async Task Middleware_ShouldCallNextDelegate()
    {
        var configurationMock = GivenConfiguration(isFeatureEnabled: true);
        var context = GivenHttpContext();
        var nextCalled = false;
        var nextDelegate = new RequestDelegate(ctx =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        var middleware = new ContentSecurityPolicyMiddleware(nextDelegate);

        await middleware.InvokeAsync(context, GivenConfiguration(isFeatureEnabled: true));

        nextCalled.Should().BeTrue("Next delegate in the pipeline should be called.");
    }

    [Fact]
    public void GenerateNonce_ShouldReturnUniqueBase64Strings()
    {
        var middleware = new ContentSecurityPolicyMiddleware(ctx => Task.CompletedTask);

        var nonce1 = middleware.GetType()
            .GetMethod("GenerateNonce", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(middleware, null) as string;
        var nonce2 = middleware.GetType()
            .GetMethod("GenerateNonce", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(middleware, null) as string;

        nonce1.Should().NotBeNullOrEmpty();
        nonce2.Should().NotBeNullOrEmpty();
        nonce1.Should().NotBe(nonce2, "Each nonce should be unique.");
    }

    private static IConfiguration GivenConfiguration(bool isFeatureEnabled)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection([
                new("Features:ContentSecurityPolicy", isFeatureEnabled ? "true" : "false")
            ])
            .Build();
    }

    private static DefaultHttpContext GivenHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        return context;
    }
}
