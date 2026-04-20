using Microsoft.AspNetCore.Authorization;
using CO.CDP.Authentication;
using CO.CDP.UserManagement.App.Authorization.Requirements;
using CO.CDP.UserManagement.Core;
using CO.CDP.UserManagement.Core.Models;

namespace CO.CDP.UserManagement.App.Authorization.Handlers;

public sealed class OrganisationOwnerOrAdminHandler(
    ISessionManager sessionManager,
    ILogger<OrganisationOwnerOrAdminHandler> logger)
    : AuthorizationHandler<OrganisationOwnerOrAdminRequirement>
{
    private static readonly HashSet<string> AllowedRoles =
        new(StringComparer.OrdinalIgnoreCase) { "Owner", "Admin" };

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OrganisationOwnerOrAdminRequirement requirement)
    {
        var httpContext = context.Resource as HttpContext;

        if (!Guid.TryParse(httpContext?.GetRouteData().Values["id"]?.ToString(), out var orgId))
            return;

        var cdpClaimsJson = await CdpClaimsResolver.ResolveAsync(context, httpContext, sessionManager, logger);

        if (string.IsNullOrWhiteSpace(cdpClaimsJson))
            return;

        var userClaims = JsonHelper.TryDeserialize<UserClaims>(cdpClaimsJson);
        var isOwnerOrAdmin = userClaims?.Organisations.Any(o =>
            o.OrganisationId == orgId &&
            AllowedRoles.Contains(o.OrganisationRole)) ?? false;

        if (isOwnerOrAdmin)
            context.Succeed(requirement);
    }
}
