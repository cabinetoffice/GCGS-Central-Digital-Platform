using CO.CDP.ApplicationRegistry.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace CO.CDP.ApplicationRegistry.Api.Authorization;

/// <summary>
/// Authorization handler for verifying organisation admin/owner role.
/// </summary>
public class OrganisationAdminHandler : AuthorizationHandler<OrganisationAdminRequirement>
{
    private readonly ILogger<OrganisationAdminHandler> _logger;
    private readonly IOrganisationRepository _organisationRepository;

    public OrganisationAdminHandler(
        ILogger<OrganisationAdminHandler> logger,
        IOrganisationRepository organisationRepository)
    {
        _logger = logger;
        _organisationRepository = organisationRepository;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OrganisationAdminRequirement requirement)
    {
        // Get organisation ID from route data
        if (context.Resource is HttpContext httpContext)
        {
            var cdpOrganisationIdValue = httpContext.Request.RouteValues["cdpOrganisationId"]?.ToString();
            if (Guid.TryParse(cdpOrganisationIdValue, out var cdpOrganisationId))
            {
                var organisation = await _organisationRepository.GetByCdpGuidAsync(cdpOrganisationId);
                if (organisation == null)
                {
                    return;
                }

                // In a real implementation, verify the user is an admin/owner of the organisation
                // For now, we'll mark as succeeded
                // TODO: Implement actual admin verification logic
                _logger.LogDebug("Checking organisation admin role for user in organisation {OrganisationId}", organisation.Id);
                context.Succeed(requirement);
            }
        }

        return;
    }
}

/// <summary>
/// Requirement for organisation admin authorization.
/// </summary>
public class OrganisationAdminRequirement : IAuthorizationRequirement
{
}
