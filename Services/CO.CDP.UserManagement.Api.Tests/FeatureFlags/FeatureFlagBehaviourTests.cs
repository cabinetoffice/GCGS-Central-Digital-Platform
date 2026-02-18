using CO.CDP.UserManagement.Api.Controllers;
using CO.CDP.UserManagement.Api.FeatureFlags;
using CO.CDP.UserManagement.Shared.FeatureFlags;
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
    [Fact]
    public async Task RequireFeatureFlag_WhenDisabled_ReturnsNotFoundAndSkipsAction()
    {
        var attribute = new RequireFeatureFlagAttribute(Shared.FeatureFlags.FeatureFlags.UserFlows.InviteFlowEnabled);
        var context = CreateActionExecutingContext(new Dictionary<string, string?>
        {
            [Shared.FeatureFlags.FeatureFlags.UserFlows.InviteFlowEnabled] = "false"
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
        var attribute = new RequireFeatureFlagAttribute(Shared.FeatureFlags.FeatureFlags.UserFlows.InviteFlowEnabled);
        var context = CreateActionExecutingContext(new Dictionary<string, string?>
        {
            [Shared.FeatureFlags.FeatureFlags.UserFlows.InviteFlowEnabled] = "true"
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

    [Fact]
    public void Controllers_AreDecoratedWithExpectedUserFlowFeatureFlags()
    {
        GetControllerFlag<OrganisationInvitesController>()
            .Should().Be(Shared.FeatureFlags.FeatureFlags.UserFlows.InviteFlowEnabled);
        GetControllerFlag<OrganisationInviteAcceptanceController>()
            .Should().Be(Shared.FeatureFlags.FeatureFlags.UserFlows.InviteFlowEnabled);
        GetControllerFlag<OrganisationUsersController>()
            .Should().Be(Shared.FeatureFlags.FeatureFlags.UserFlows.MembershipFlowEnabled);
        GetControllerFlag<UserAssignmentsController>()
            .Should().Be(Shared.FeatureFlags.FeatureFlags.UserFlows.MembershipFlowEnabled);
    }

    private static string GetControllerFlag<TController>()
    {
        var attribute = typeof(TController)
            .GetCustomAttributes(typeof(RequireFeatureFlagAttribute), inherit: true)
            .Cast<RequireFeatureFlagAttribute>()
            .Single();

        return attribute.FeatureFlagName;
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
