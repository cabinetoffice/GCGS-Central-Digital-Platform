using CO.CDP.OrganisationApp.Constants;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages;

public class DisabledByFeatureAttributeTests
{
    private const string TestFeature = "TestFeature";

    // Concrete stub — PageModel is abstract
    private class StubPageModel : PageModel { }

    [Fact]
    public async Task OnPageHandlerExecutionAsync_WhenFeatureEnabled_Returns404AndDoesNotCallNext()
    {
        var featureManagerMock = new Mock<IFeatureManager>();
        featureManagerMock.Setup(fm => fm.IsEnabledAsync(TestFeature)).ReturnsAsync(true);

        var (context, nextCalled) = BuildContext(featureManagerMock.Object);
        var attribute = new DisabledByFeatureAttribute(TestFeature);

        await attribute.OnPageHandlerExecutionAsync(context, () =>
        {
            nextCalled[0] = true;
            var pageContext = new PageContext(new ActionContext(
                context.HttpContext, new RouteData(), new PageActionDescriptor()));
            return Task.FromResult(new PageHandlerExecutedContext(
                pageContext, [], context.HandlerMethod, new StubPageModel()));
        });

        context.Result.Should().BeOfType<NotFoundResult>();
        nextCalled[0].Should().BeFalse();
    }

    [Fact]
    public async Task OnPageHandlerExecutionAsync_WhenFeatureDisabled_CallsNext()
    {
        var featureManagerMock = new Mock<IFeatureManager>();
        featureManagerMock.Setup(fm => fm.IsEnabledAsync(TestFeature)).ReturnsAsync(false);

        var (context, nextCalled) = BuildContext(featureManagerMock.Object);
        var attribute = new DisabledByFeatureAttribute(TestFeature);

        await attribute.OnPageHandlerExecutionAsync(context, () =>
        {
            nextCalled[0] = true;
            var pageContext = new PageContext(new ActionContext(
                context.HttpContext, new RouteData(), new PageActionDescriptor()));
            return Task.FromResult(new PageHandlerExecutedContext(
                pageContext, [], context.HandlerMethod, new StubPageModel()));
        });

        context.Result.Should().BeNull();
        nextCalled[0].Should().BeTrue();
    }

    [Fact]
    public async Task OnPageHandlerSelectionAsync_AlwaysCompletesSuccessfully()
    {
        var attribute = new DisabledByFeatureAttribute(TestFeature);
        var httpContext = new DefaultHttpContext();
        var routeData = new RouteData();
        var actionDescriptor = new PageActionDescriptor();
        var selectedContext = new PageHandlerSelectedContext(
            new PageContext(new ActionContext(httpContext, routeData, actionDescriptor)),
            [],
            new StubPageModel());

        var act = async () => await attribute.OnPageHandlerSelectionAsync(selectedContext);

        await act.Should().NotThrowAsync();
    }

    private static (PageHandlerExecutingContext context, bool[] nextCalled) BuildContext(IFeatureManager featureManager)
    {
        var services = new ServiceCollection();
        services.AddSingleton(featureManager);
        var serviceProvider = services.BuildServiceProvider();

        var httpContext = new DefaultHttpContext { RequestServices = serviceProvider };
        var routeData = new RouteData();
        var actionDescriptor = new PageActionDescriptor();
        var pageContext = new PageContext(new ActionContext(httpContext, routeData, actionDescriptor));

        var context = new PageHandlerExecutingContext(
            pageContext,
            [],
            new HandlerMethodDescriptor(),
            new Dictionary<string, object?>(),
            new StubPageModel());

        return (context, new bool[1]);
    }
}
