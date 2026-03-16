using CO.CDP.UserManagement.Api.FeatureFlags;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CO.CDP.UserManagement.Api.Tests.FeatureFlags;

public class FeatureFlagBehaviourTests
{
    private const string TestFeatureFlag = "Features:TestFlag";

    [Fact]
    public async Task RequireFeatureFlag_WhenDisabled_ReturnsNotFoundAndSkipsAction()
    {
        var attribute = new RequireFeatureFlagAttribute(TestFeatureFlag);
        var context = CreateActionExecutingContext(new Dictionary<string, string?>
        {
            [TestFeatureFlag] = "false"
        });
        var nextCalled = false;

        await attribute.OnActionExecutionAsync(context, () =>
        {
            nextCalled = true;
            return Task.FromResult(new ActionExecutedContext(context, [], controller: null));
        });

        context.Result.Should().BeOfType<NotFoundResult>();
        nextCalled.Should().BeFalse();
    }

    [Fact]
    public async Task RequireFeatureFlag_WhenEnabled_ExecutesAction()
    {
        var attribute = new RequireFeatureFlagAttribute(TestFeatureFlag);
        var context = CreateActionExecutingContext(new Dictionary<string, string?>
        {
            [TestFeatureFlag] = "true"
        });
        var nextCalled = false;

        await attribute.OnActionExecutionAsync(context, () =>
        {
            nextCalled = true;
            return Task.FromResult(new ActionExecutedContext(context, [], controller: null));
        });

        context.Result.Should().BeNull();
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public void SubscriberFeatureFlags_FromConfiguration_ReadsConfiguredValues()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [Shared.FeatureFlags.FeatureFlags.Subscribers.OrganisationRegisteredEnabled] = "true",
                [Shared.FeatureFlags.FeatureFlags.Subscribers.OrganisationUpdatedEnabled] = "false",
                [Shared.FeatureFlags.FeatureFlags.Subscribers.PersonInviteClaimedEnabled] = "true"
            })
            .Build();

        var flags = SubscriberFeatureFlags.FromConfiguration(configuration);

        flags.OrganisationRegisteredEnabled.Should().BeTrue();
        flags.OrganisationUpdatedEnabled.Should().BeFalse();
        flags.PersonInviteClaimedEnabled.Should().BeTrue();
    }

    private static ActionExecutingContext CreateActionExecutingContext(
        IDictionary<string, string?> settings)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();
        var services = new ServiceCollection()
            .AddSingleton<IConfiguration>(configuration)
            .BuildServiceProvider();
        var httpContext = new DefaultHttpContext
        {
            RequestServices = services
        };
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        return new ActionExecutingContext(actionContext, [], new Dictionary<string, object?>(), controller: null);
    }
}
