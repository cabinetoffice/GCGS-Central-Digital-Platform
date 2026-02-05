using CO.CDP.ApplicationRegistry.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace CO.CDP.ApplicationRegistry.Api.Authorization;

/// <summary>
    /// Authorization handler for verifying organisation membership.
/// </summary>
public class OrganisationMemberHandler : AuthorizationHandler<OrganisationMemberRequirement>
{
    private readonly ILogger<OrganisationMemberHandler> _logger;
    private readonly IOrganisationRepository _organisationRepository;

    public OrganisationMemberHandler(
        ILogger<OrganisationMemberHandler> logger,
        IOrganisationRepository organisationRepository)
    {
        _logger = logger;
        _organisationRepository = organisationRepository;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OrganisationMemberRequirement requirement)
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

                // TODO: Implement actual membership verification logic
                _logger.LogDebug("Checking organisation membership for user in organisation {OrganisationId}", organisation.Id);
                context.Succeed(requirement);
            }
        }

        return;
    }
}

/// <summary>
/// Requirement for organisation membership authorization.
/// </summary>
public class OrganisationMemberRequirement : IAuthorizationRequirement
{
}
