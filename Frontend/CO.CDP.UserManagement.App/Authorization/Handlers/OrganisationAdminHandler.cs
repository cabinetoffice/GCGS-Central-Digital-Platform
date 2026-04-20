using Microsoft.AspNetCore.Authorization;
using CO.CDP.Authentication;
using CO.CDP.UserManagement.App.Authorization.Requirements;
using CO.CDP.UserManagement.Core;
using CO.CDP.UserManagement.Core.Models;

namespace CO.CDP.UserManagement.App.Authorization.Handlers;

public sealed class OrganisationAdminHandler(
    ISessionManager sessionManager,
    ILogger<OrganisationAdminHandler> logger)
    : AuthorizationHandler<OrganisationAdminRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OrganisationAdminRequirement requirement)
    {
        var httpContext = context.Resource as HttpContext;

        if (!Guid.TryParse(httpContext?.GetRouteData().Values["id"]?.ToString(), out var orgId))
            return;

        var cdpClaimsJson = await CdpClaimsResolver.ResolveAsync(context, httpContext, sessionManager, logger);

        if (string.IsNullOrWhiteSpace(cdpClaimsJson))
            return;

        var userClaims = JsonHelper.TryDeserialize<UserClaims>(cdpClaimsJson);
        var isAdmin = userClaims?.Organisations.Any(o =>
            o.OrganisationId == orgId &&
            string.Equals(o.OrganisationRole, "Admin", StringComparison.OrdinalIgnoreCase)) ?? false;

        if (isAdmin)
            context.Succeed(requirement);
    }
}
