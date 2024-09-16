using CO.CDP.OrganisationInformation.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

namespace CO.CDP.Authentication.Authorization;

public class OrganisationAuthorizationHandler(
    IHttpContextAccessor httpContextAccessor,
    ITenantRepository tenantRepository)
    : AuthorizationHandler<OrganisationAuthorizationRequirement>
{
    private const string RegexGuid = @"[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}";

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, OrganisationAuthorizationRequirement requirement)
    {
        if (ValidateChannel(context, requirement.Channels))
        {
            if (!requirement.Channels.Contains(AuthenticationChannel.OneLogin) || requirement.Scopes.Length == 0)
            {
                context.Succeed(requirement);
            }
            else
            {
                var userUrn = context.User.FindFirstValue("sub");
                var organisationId = await FetchOrganisationIdAsync(requirement.OrganisationIdLocation);

                if (!string.IsNullOrWhiteSpace(userUrn)
                    && !string.IsNullOrWhiteSpace(organisationId)
                    && Guid.TryParse(organisationId, out var organisationGuid))
                {
                    var lookup = await tenantRepository.LookupTenant(userUrn);
                    List<string> orgScopes = lookup?.Tenants?.SelectMany(t => t.Organisations)?
                                .FirstOrDefault(o => o.Id == organisationGuid)?.Scopes ?? [];

                    if (requirement.Scopes.Intersect(orgScopes).Any())
                    {
                        context.Succeed(requirement);
                    }
                }
            }
        }
    }

    private static bool ValidateChannel(AuthorizationHandlerContext context, AuthenticationChannel[] channels)
    {
        var hasOneLoginChannelClaim = channels.Contains(AuthenticationChannel.OneLogin)
                                        && context.User.HasClaim("channel", "one-login");

        var hasOrganisationKeyChannelClaim = channels.Contains(AuthenticationChannel.OrganisationKey)
                                        && context.User.HasClaim("channel", "organisation-key");

        var hasServiceKeyChannelClaim = channels.Contains(AuthenticationChannel.ServiceKey)
                                        && context.User.HasClaim("channel", "service-key");

        return hasOneLoginChannelClaim || hasOrganisationKeyChannelClaim || hasServiceKeyChannelClaim;
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
                if (currentRequest.Body == null && currentRequest.ContentType != Application.Json) return null;

                if (!currentRequest.Body!.CanSeek) currentRequest.EnableBuffering();
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