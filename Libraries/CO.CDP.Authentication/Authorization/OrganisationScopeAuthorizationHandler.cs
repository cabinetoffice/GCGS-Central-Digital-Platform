using CO.CDP.OrganisationInformation.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using static CO.CDP.Authentication.Constants;
using static System.Net.Mime.MediaTypeNames;

namespace CO.CDP.Authentication.Authorization;

public class OrganisationScopeAuthorizationHandler(
    IHttpContextAccessor httpContextAccessor,
    IOrganisationRepository organisationRepository)
    : AuthorizationHandler<OrganisationScopeAuthorizationRequirement>
{
    private const string RegexGuid = @"[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}";

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, OrganisationScopeAuthorizationRequirement requirement)
    {
        var channel = context.User.FindFirstValue(ClaimType.Channel);
        if (channel != Channel.OneLogin)
        {
            context.Succeed(requirement);
            return;
        }

        var userUrn = context.User.FindFirstValue(ClaimType.Subject);

        if (!string.IsNullOrWhiteSpace(userUrn))
        {
            if (MeetsPersonScopesRequirement(context, requirement.PersonScopes)
                || await MeetsOrganisationPersonScopesRequirement(requirement, userUrn))
            {
                context.Succeed(requirement);
            }
        }
    }

    private static bool MeetsPersonScopesRequirement(AuthorizationHandlerContext context, string[] personScopesRequired)
    {
        var personRoles = context.User.FindFirstValue(ClaimType.Roles);
        var personRolesArray = (personRoles ?? "").Split(",", StringSplitOptions.RemoveEmptyEntries);

        if (personRolesArray.Contains(PersonScope.SuperAdmin))
        {
            return true;
        }

        if (personScopesRequired.Intersect(personRolesArray).Any())
        {
            return true;
        }

        return false;
    }

    private async Task<bool> MeetsOrganisationPersonScopesRequirement(OrganisationScopeAuthorizationRequirement requirement, string userUrn)
    {
        if (requirement.OrganisationPersonScopes.Length == 0 || requirement.OrganisationIdLocation == OrganisationIdLocation.None)
        {
            return false;
        }

        var organisationId = await FetchOrganisationIdAsync(requirement.OrganisationIdLocation);

        if (!string.IsNullOrWhiteSpace(organisationId) && Guid.TryParse(organisationId, out var organisationGuid))
        {
            var orgPerson = await organisationRepository.FindOrganisationPerson(organisationGuid, userUrn);

            if (requirement.OrganisationPersonScopes.Intersect(orgPerson?.Scopes ?? []).Any())
            {
                return true;
            }
        }

        return false;
    }

    private async Task<string?> FetchOrganisationIdAsync(OrganisationIdLocation organisationIdLocation)
    {
        var currentRequest = httpContextAccessor?.HttpContext?.Request;
        if (currentRequest == null) return null;

        switch (organisationIdLocation)
        {
            case OrganisationIdLocation.Path:
                if (currentRequest.Path.Value == null) return null;
                var pathMatch = Regex.Match(currentRequest.Path.Value, $"/organisations/({RegexGuid})");
                if (pathMatch.Success) return pathMatch.Groups[1].Value;
                break;

            case OrganisationIdLocation.QueryString:
                if (currentRequest.QueryString.Value == null) return null;
                var queryMatch = Regex.Match(currentRequest.QueryString.Value, $"organisation-id=({RegexGuid})");
                if (queryMatch.Success) return queryMatch.Groups[1].Value;
                break;

            case OrganisationIdLocation.Body:
                if (currentRequest.Body == null || currentRequest.ContentType?.Contains(Application.Json) == false) return null;

                if (!currentRequest.Body.CanSeek) currentRequest.EnableBuffering();
                currentRequest.Body.Position = 0;
                var reader = new StreamReader(currentRequest.Body, Encoding.UTF8);
                var body = await reader.ReadToEndAsync().ConfigureAwait(false);
                currentRequest.Body.Position = 0;

                if (!string.IsNullOrWhiteSpace(body))
                {
                    var values = JsonSerializer.Deserialize<Dictionary<string, string>>(body);
                    if (values?.TryGetValue("organisationId", out var organisatinId) == true)
                    {
                        return organisatinId;
                    }
                }
                break;
        }

        return null;
    }
}