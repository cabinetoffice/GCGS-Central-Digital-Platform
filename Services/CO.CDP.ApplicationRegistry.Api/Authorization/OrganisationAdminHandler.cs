using Microsoft.AspNetCore.Authorization;

namespace CO.CDP.ApplicationRegistry.Api.Authorization;

/// <summary>
/// Authorization handler for verifying organisation admin/owner role.
/// </summary>
public class OrganisationAdminHandler : AuthorizationHandler<OrganisationAdminRequirement>
{
    private readonly ILogger<OrganisationAdminHandler> _logger;

    public OrganisationAdminHandler(ILogger<OrganisationAdminHandler> logger)
    {
        _logger = logger;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OrganisationAdminRequirement requirement)
    {
        // Get organisation ID from route data
        if (context.Resource is HttpContext httpContext)
        {
            var orgIdValue = httpContext.Request.RouteValues["orgId"]?.ToString();
            if (int.TryParse(orgIdValue, out var orgId))
            {
                // In a real implementation, verify the user is an admin/owner of the organisation
                // For now, we'll mark as succeeded
                // TODO: Implement actual admin verification logic
                _logger.LogDebug("Checking organisation admin role for user in organisation {OrganisationId}", orgId);
                context.Succeed(requirement);
            }
        }

        return Task.CompletedTask;
    }
}

/// <summary>
/// Requirement for organisation admin authorization.
/// </summary>
public class OrganisationAdminRequirement : IAuthorizationRequirement
{
}
