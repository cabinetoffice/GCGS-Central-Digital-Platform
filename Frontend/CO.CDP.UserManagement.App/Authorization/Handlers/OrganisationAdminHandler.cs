using Microsoft.AspNetCore.Authorization;
using CO.CDP.Authentication;
using CO.CDP.UserManagement.App.Authorization.Requirements;
using CO.CDP.UserManagement.Core;
using CO.CDP.UserManagement.Core.Models;
using ApiClient = CO.CDP.UserManagement.WebApiClient;

namespace CO.CDP.UserManagement.App.Authorization.Handlers;

public sealed class OrganisationAdminHandler(
    ApiClient.UserManagementClient apiClient,
    ISessionManager sessionManager,
    ILogger<OrganisationAdminHandler> logger)
    : AuthorizationHandler<OrganisationAdminRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OrganisationAdminRequirement requirement)
    {
        var httpContext = context.Resource as HttpContext;
        var organisationSlug = httpContext?.GetRouteData().Values["organisationSlug"] as string;

        if (string.IsNullOrWhiteSpace(organisationSlug))
            return;

        var cdpClaimsJson = await CdpClaimsResolver.ResolveAsync(context, httpContext, sessionManager, logger);

        if (string.IsNullOrWhiteSpace(cdpClaimsJson))
            return;

        try
        {
            var org = await apiClient.BySlugAsync(organisationSlug);
            var orgId = org?.CdpOrganisationGuid ?? Guid.Empty;

            if (orgId == Guid.Empty)
                return;

            var userClaims = JsonHelper.TryDeserialize<UserClaims>(cdpClaimsJson);
            var isAdmin = userClaims?.Organisations.Any(o =>
                o.OrganisationId == orgId &&
                string.Equals(o.OrganisationRole, "Admin", StringComparison.OrdinalIgnoreCase)) ?? false;

            if (isAdmin)
                context.Succeed(requirement);
        }
        catch (ApiClient.ApiException ex)
        {
            logger.LogWarning(ex, "OrganisationAdminHandler: API error for slug {Slug}", organisationSlug);
        }
    }
}
