using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using System.Data;

namespace CO.CDP.Authentication.Authorization;

public class OrganisationAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options) : IAuthorizationPolicyProvider
{
    private readonly DefaultAuthorizationPolicyProvider _fallbackPolicyProvider = new(options);

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
    {
        return Task.FromResult(new AuthorizationPolicyBuilder()
                    .AddAuthenticationSchemes(Extensions.JwtBearerOrApiKeyScheme)
                    .RequireAuthenticatedUser()
                    .Build());
    }

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
    {
        return _fallbackPolicyProvider.GetFallbackPolicyAsync();
    }

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (!string.IsNullOrWhiteSpace(policyName))
        {
            var requirements = GetRequirements(policyName);

            if (requirements.Length > 0)
            {
                var policy = new AuthorizationPolicyBuilder(Extensions.JwtBearerOrApiKeyScheme)
                    .RequireAuthenticatedUser()
                    .AddRequirements(requirements)
                    .Build();

                return Task.FromResult<AuthorizationPolicy?>(policy);
            }
        }

        return _fallbackPolicyProvider.GetPolicyAsync(policyName);
    }

    private static IAuthorizationRequirement[] GetRequirements(string policyName)
    {
        if (!policyName.StartsWith(OrganisationAuthorizeAttribute.PolicyPrefix))
        {
            return [];
        }

        var policyTokens = policyName[OrganisationAuthorizeAttribute.PolicyPrefix.Length..]
                        .Split(';', StringSplitOptions.RemoveEmptyEntries);

        AuthenticationChannel[] channels = [];
        string[] scopes = [];
        var organisationIdLocation = OrganisationIdLocation.None;

        foreach (var token in policyTokens)
        {
            var pair = token.Split('$', StringSplitOptions.RemoveEmptyEntries);

            if (pair.Length == 2)
            {
                switch (pair[0])
                {
                    case OrganisationAuthorizeAttribute.ChannelsGroup:
                        channels = pair[1].Split('|', StringSplitOptions.RemoveEmptyEntries)
                                    .Where(t => Enum.IsDefined(typeof(AuthenticationChannel), t))
                                    .Select(t => (AuthenticationChannel)Enum.Parse(typeof(AuthenticationChannel), t)).ToArray();
                        break;

                    case OrganisationAuthorizeAttribute.ScopesGroup:
                        scopes = pair[1].Split('|', StringSplitOptions.RemoveEmptyEntries);
                        break;

                    case OrganisationAuthorizeAttribute.OrgIdLocGroup:
                        if (Enum.TryParse(pair[1], true, out OrganisationIdLocation value))
                        {
                            organisationIdLocation = value;
                        }
                        break;
                }
            }
        }

        List<IAuthorizationRequirement> requirements = [new ChannelAuthorizationRequirement(channels)];

        if (channels.Contains(AuthenticationChannel.OneLogin) && scopes.Length > 0)
        {
            requirements.Add(new OrganisationScopeAuthorizationRequirement(scopes, organisationIdLocation));
        }

        return [.. requirements];
    }
}