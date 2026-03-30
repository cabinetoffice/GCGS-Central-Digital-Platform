using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using CO.CDP.Authentication;
using CO.CDP.UserManagement.App.Authorization.Requirements;
using CO.CDP.UserManagement.Core;
using CO.CDP.UserManagement.Core.Models;
using ApiClient = CO.CDP.UserManagement.WebApiClient;

namespace CO.CDP.UserManagement.App.Authorization.Handlers;

public sealed class OrganisationOwnerOrAdminHandler(
    ApiClient.UserManagementClient apiClient,
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
        var organisationSlug = httpContext?.GetRouteData().Values["organisationSlug"] as string;

        if (string.IsNullOrWhiteSpace(organisationSlug))
            return;

        var cdpClaimsJson = await ResolveCdpClaimsJsonAsync(context, httpContext);

        if (string.IsNullOrWhiteSpace(cdpClaimsJson))
            return;

        try
        {
            var org = await apiClient.BySlugAsync(organisationSlug);
            var orgId = org?.CdpOrganisationGuid ?? Guid.Empty;

            if (orgId == Guid.Empty)
                return;

            var userClaims = JsonHelper.TryDeserialize<UserClaims>(cdpClaimsJson);
            var isOwnerOrAdmin = userClaims?.Organisations.Any(o =>
                o.OrganisationId == orgId &&
                AllowedRoles.Contains(o.OrganisationRole)) ?? false;

            if (isOwnerOrAdmin)
                context.Succeed(requirement);
        }
        catch (ApiClient.ApiException ex)
        {
            logger.LogWarning(ex, "OrganisationOwnerOrAdminHandler: API error for slug {Slug}", organisationSlug);
        }
    }

    private async Task<string?> ResolveCdpClaimsJsonAsync(
        AuthorizationHandlerContext context,
        HttpContext? httpContext)
    {
        var fromPrincipal = context.User.FindFirst("cdp_claims")?.Value;
        if (!string.IsNullOrWhiteSpace(fromPrincipal))
            return fromPrincipal;

        var tokenSet = await sessionManager.GetTokensAsync(httpContext!);
        var authorityToken = tokenSet?.AccessToken;

        if (string.IsNullOrWhiteSpace(authorityToken))
            return null;

        try
        {
            var token = new JwtSecurityTokenHandler().ReadJwtToken(authorityToken);
            return token.Claims.FirstOrDefault(c => c.Type == "cdp_claims")?.Value;
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "OrganisationOwnerOrAdminHandler: failed to read JWT or extract cdp_claims");
            return null;
        }
    }
}
