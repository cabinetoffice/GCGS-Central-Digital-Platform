using CO.CDP.Authentication.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Text.Json;
using System.Text.RegularExpressions;
using static CO.CDP.Authentication.Constants;

namespace CO.CDP.Authentication.Authorization;

public class ApplicationScopeAuthorizationHandler(
    IHttpContextAccessor httpContextAccessor)
    : AuthorizationHandler<ApplicationScopeAuthorizationRequirement>
{
    private const string RegexGuid =
        @"[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}";

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ApplicationScopeAuthorizationRequirement requirement)
    {
        var channel = context.User.FindFirstValue(ClaimType.Channel);
        if (channel != Channel.OneLogin)
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        var personRoles = context.User.FindFirstValue(ClaimType.Roles);
        var personRolesArray = (personRoles ?? "").Split(",", StringSplitOptions.RemoveEmptyEntries);
        if (personRolesArray.Contains(PersonScope.SuperAdmin))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        var cdpClaimsJson = context.User.FindFirstValue(ClaimType.CdpClaims);
        if (string.IsNullOrWhiteSpace(cdpClaimsJson))
        {
            return Task.CompletedTask;
        }

        UserClaims? userClaims;
        try
        {
            userClaims = JsonSerializer.Deserialize<UserClaims>(cdpClaimsJson);
        }
        catch
        {
            return Task.CompletedTask;
        }

        if (userClaims == null)
        {
            return Task.CompletedTask;
        }

        var organisationId = ExtractOrganisationId();
        if (organisationId == null)
        {
            return Task.CompletedTask;
        }

        var orgClaim = userClaims.Organisations
            .FirstOrDefault(o => o.OrganisationId == organisationId.Value);
        if (orgClaim == null)
        {
            return Task.CompletedTask;
        }

        var appClaim = orgClaim.Applications
            .FirstOrDefault(a => a.ClientId == requirement.ClientId);
        if (appClaim == null)
        {
            return Task.CompletedTask;
        }

        if (requirement.ApplicationRoles.Length == 0 && requirement.ApplicationPermissions.Length == 0)
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        if (requirement.ApplicationRoles.Length > 0 &&
            requirement.ApplicationRoles.Intersect(appClaim.Roles).Any())
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        if (requirement.ApplicationPermissions.Length > 0 &&
            requirement.ApplicationPermissions.Intersect(appClaim.Permissions).Any())
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        return Task.CompletedTask;
    }

    private Guid? ExtractOrganisationId()
    {
        var path = httpContextAccessor?.HttpContext?.Request.Path.Value;
        if (path == null) return null;

        var match = Regex.Match(path, $"/organisations/({RegexGuid})");
        if (match.Success && Guid.TryParse(match.Groups[1].Value, out var orgId))
        {
            return orgId;
        }
        return null;
    }
}
