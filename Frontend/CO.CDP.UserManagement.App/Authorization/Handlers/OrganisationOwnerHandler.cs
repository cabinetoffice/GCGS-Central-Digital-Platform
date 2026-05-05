using Microsoft.AspNetCore.Authorization;
using CO.CDP.Authentication;
using CO.CDP.UserManagement.App.Authorization.Requirements;
using CO.CDP.UserManagement.Core;
using CO.CDP.UserManagement.Core.Models;

namespace CO.CDP.UserManagement.App.Authorization.Handlers;

public sealed class OrganisationOwnerHandler(
    ISessionManager sessionManager,
    ILogger<OrganisationOwnerHandler> logger)
    : AuthorizationHandler<OrganisationOwnerRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OrganisationOwnerRequirement requirement)
    {
        var httpContext = context.Resource as HttpContext;

        if (!Guid.TryParse(httpContext?.GetRouteData().Values["id"]?.ToString(), out var orgId))
            return;

        var cdpClaimsJson = await CdpClaimsResolver.ResolveAsync(context, httpContext, sessionManager, logger);

        if (string.IsNullOrWhiteSpace(cdpClaimsJson))
            return;

        var userClaims = JsonHelper.TryDeserialize<UserClaims>(cdpClaimsJson);
        var isOwner = userClaims?.Organisations.Any(o =>
            o.OrganisationId == orgId &&
            string.Equals(o.OrganisationRole, "Owner", StringComparison.OrdinalIgnoreCase)) ?? false;

        if (isOwner)
            context.Succeed(requirement);
    }
}
