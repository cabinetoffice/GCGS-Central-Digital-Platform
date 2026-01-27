using Microsoft.AspNetCore.Authorization;

namespace CO.CDP.ApplicationRegistry.Api.Authorization;

/// <summary>
/// Authorization handler for verifying organisation membership.
/// </summary>
public class OrganisationMemberHandler : AuthorizationHandler<OrganisationMemberRequirement>
{
    private readonly ILogger<OrganisationMemberHandler> _logger;

    public OrganisationMemberHandler(ILogger<OrganisationMemberHandler> logger)
    {
        _logger = logger;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OrganisationMemberRequirement requirement)
    {
        // Get organisation ID from route data
        if (context.Resource is HttpContext httpContext)
        {
            var orgIdValue = httpContext.Request.RouteValues["orgId"]?.ToString();
            if (int.TryParse(orgIdValue, out var orgId))
            {

                // TODO: Implement actual membership verification logic
                _logger.LogDebug("Checking organisation membership for user in organisation {OrganisationId}", orgId);
                context.Succeed(requirement);
            }
        }

        return Task.CompletedTask;
    }
}

/// <summary>
/// Requirement for organisation membership authorization.
/// </summary>
public class OrganisationMemberRequirement : IAuthorizationRequirement
{
}
